using OpenTK.Mathematics;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Reflection;

namespace rt004;

public interface ICommand
{
    Command GetCommand();
}

internal class FrontController
{
    RootCommand rootCommand;

    private RootCommand RootCMD()
    {
        rootCommand = new RootCommand();

        var configOption = new Option<string[]>
            (
            aliases: new string[] { "--config", "-c", "--conf" },
            description: "path to the configuration file"
            );
        configOption.AllowMultipleArgumentsPerToken = true;
        configOption.Arity = ArgumentArity.OneOrMore;
        rootCommand.AddOption(configOption);

        rootCommand.SetHandler((config) =>
        {
            if (config != null && config.Length > 0)
                foreach(var c in ParseConfig(config))
                    Invoke(c);
            else
                //TODO write the help message immediately
                Console.WriteLine("RT004 program. Use --help option for help.");
        }, configOption);

        return rootCommand;
    }

    private void LoadAllCommands()
    {
        Assembly assembly = Assembly.Load("rt004");

        // Get all types in the assembly
        Type[] types = assembly.GetTypes();

        // Filter and instantiate types that implement ICommand
        var commandTypes = types
            .Where(type => type.IsClass && typeof(ICommand).IsAssignableFrom(type))
            .Select(type => (ICommand)Activator.CreateInstance(type));

        foreach (var command in commandTypes)
            if (command != null)
            {
                var comm = command.GetCommand();
                foreach(var o in comm.Options)
                    o.AllowMultipleArgumentsPerToken = true;
                rootCommand.AddCommand(comm);
                //Console.WriteLine($"Loaded {command.GetType().FullName}");
            }

    }

    /// <summary>
    /// Parses the config file and merges it with the command line arguments
    /// </summary>
    /// <param name="configArgs"> first element is path to the config file. Other elements will override loaded file</param>
    /// <returns>Loaded (overriden) arguments</returns>
    private List<string[]> ParseConfig(string[] configArgs)
    {
        string path = configArgs[0];
        if (!File.Exists(path))
        {
            Console.WriteLine($"Config file '{path}' not found");
            return new List<string[]>() { };
        }
        var configParsedByLines = File.ReadAllLines(path);
        List<string[]> result = new List<string[]>();
        foreach (var line in configParsedByLines) { 
            var configParsedByQuotes = line.Split('"');
            string[] configFileArgs = new string[0];
            for (int i = 0; i < configParsedByQuotes.Length; i++)
                if (i % 2 == 1)
                    configFileArgs = configFileArgs.Append(configParsedByQuotes[i]).ToArray();
                else
                    foreach (var s in configParsedByQuotes[i].Split(' '))
                        if (s != "")
                            configFileArgs = configFileArgs.Append(s).ToArray();
            
            result.Add(configFileArgs);
        }
        return result;
    }

    public FrontController()
    {
     
        rootCommand = RootCMD();
        LoadAllCommands();

    }
    public void Invoke(string[] args)
    {
        rootCommand.Invoke(args);
        
    }
}

