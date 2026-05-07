using Microsoft.Data.SqlClient;
using System.Text.Json;

internal static class SchemaReader
{
    private const string ColumnSql = """
        SELECT
            C.TABLE_NAME,
            C.COLUMN_NAME,
            C.DATA_TYPE,
            C.IS_NULLABLE,
            C.CHARACTER_MAXIMUM_LENGTH,
            CASE WHEN PK.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_PRIMARY_KEY
        FROM
            INFORMATION_SCHEMA.COLUMNS C
        LEFT JOIN (
            SELECT
                KCU.TABLE_NAME, KCU.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU
                ON TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME
                AND TC.TABLE_SCHEMA = KCU.TABLE_SCHEMA
            WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
        ) PK ON C.TABLE_NAME = PK.TABLE_NAME AND C.COLUMN_NAME = PK.COLUMN_NAME
        WHERE
            C.TABLE_SCHEMA = 'dbo'
        ORDER BY
            C.TABLE_NAME,
            C.ORDINAL_POSITION
        """;

    private const string ForeignKeySql = """
        SELECT
            FK.TABLE_NAME AS SourceTable,
            FKC.COLUMN_NAME AS SourceColumn,
            PK.TABLE_NAME AS ReferencedTable,
            PKC.COLUMN_NAME AS ReferencedColumn
        FROM
            INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
        JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK
            ON RC.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE FKC
            ON FK.CONSTRAINT_NAME = FKC.CONSTRAINT_NAME
        JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK
            ON RC.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE PKC
            ON PK.CONSTRAINT_NAME = PKC.CONSTRAINT_NAME
            AND PKC.ORDINAL_POSITION = FKC.ORDINAL_POSITION
        ORDER BY
            FK.TABLE_NAME,
            FKC.COLUMN_NAME
        """;

    public static async Task<string> GetSchemaJsonAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        Dictionary<string, List<object>> tableList = new(StringComparer.OrdinalIgnoreCase);
        await using (var cmd = new SqlCommand(ColumnSql, connection))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                string tableName = reader.GetString(0);
                if (!tableList.ContainsKey(tableName))
                {
                    tableList[tableName] = new List<object>();
                }
                tableList[tableName].Add(new
                {
                    ColumnName = reader.GetString(1),
                    DataType = reader.GetString(2),
                    IsNullable = reader.GetString(3) == "YES",
                    CharacterMaximumLength = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4),
                    IsPrimaryKey = reader.GetInt32(5) == 1
                });
            }
        }

        List<(string SourceTable, string SourceColumn, string ReferencedTable, string ReferencedColumn)> foreignKeys = new();
        await using (var cmd = new SqlCommand(ForeignKeySql, connection))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                foreignKeys.Add((
                    SourceTable: reader.GetString(0),
                    SourceColumn: reader.GetString(1),
                    ReferencedTable: reader.GetString(2),
                    ReferencedColumn: reader.GetString(3)
                ));
            }
        }

        object schemaTable = tableList.Select(t => new
        {
            TableName = t.Key,
            Columns = t.Value,
            ForeignKeys = foreignKeys.Where(fk => fk.SourceTable == t.Key).Select(fk => new
            {
                fk.SourceColumn,
                fk.ReferencedTable,
                fk.ReferencedColumn
            }).ToList()
        }).ToList();

        object schema = new
        {
            connection.Database,
            TableCount = tableList.Count,
            Tables = schemaTable
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }
}