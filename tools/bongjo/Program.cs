using System.CommandLine;

var rootCommand = new RootCommand("Support for sarif files and github review comments");
rootCommand.Add(new ReviewGithubCommand());
var parseResult = rootCommand.Parse(args);
parseResult.InvocationConfiguration.Output = Console.Error;
parseResult.InvocationConfiguration.Error = Console.Error;
return await parseResult.InvokeAsync();
