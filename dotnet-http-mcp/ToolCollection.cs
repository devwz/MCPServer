using System.ComponentModel;
using ModelContextProtocol.Server;

[McpServerToolType]
public class HelloWorldTool
{
    [McpServerTool(Name = "hello_world"), Description("Return a greeting message")]
    public string HelloWorld() => "Hello, World!";
}

[McpServerToolType]
public class RandomNumberTool
{
    [McpServerTool(Name = "get_random_number"), Description("Get a random number between 1 and the given number")]
    public int GetRandomNumber(int number)
    {
        Random random = new ();
        return random.Next(1, number + 1);
    }
}

[McpServerToolType]
public class CheckEvenOrOddTool
{
    [McpServerTool(Name = "check_even_or_odd"), Description("Check if a number is even or odd")]
    public string CheckEvenOrOdd(int number) => number % 2 == 0 ? "even" : "odd";
}