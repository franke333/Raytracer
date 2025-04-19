using OpenTK.Mathematics;
using rt004.Rendering;
using rt004.Utility;
using System.CommandLine;
using System.Globalization;
using Util;

namespace rt004;

class RenderCommand : ICommand
{
    public Command GetCommand()
    {
        var renderCommand = new Command("render", "Renders a scene");

        var pathArgument = new Argument<string>
            (
            name: "fileName",
            description: "name/path of the output image",
            getDefaultValue: () => "cameraDefault.pfm"
            );

        var sceneOption = new Option<List<string>>
            (
            aliases: new string[] { "--scene", "-s" },
            description: "path to the scene json file",
            parseArgument: argResult =>
            {
                return argResult.Tokens.Select(t => t.Value).ToList();
            }
            );

        sceneOption.AllowMultipleArgumentsPerToken = true;
        sceneOption.Arity = ArgumentArity.OneOrMore;

        renderCommand.Add(pathArgument);
        renderCommand.Add(sceneOption);



        renderCommand.SetHandler((context) =>
        {
            var scene = context.ParseResult.GetValueForOption(sceneOption);
            foreach (var s in scene)
                Console.WriteLine($"scene: '{s}'");
            string path = context.ParseResult.GetValueForArgument(pathArgument);
            SceneDefinition.DrawScene(scene, path);
        });
        return renderCommand;
    }
}

internal class SceneDefinition
{
    public static void DrawScene(List<string> scenesPaths, string outputPath)
    {
        SceneJson scj = new SceneJson();

        Camera cam;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var sc = SceneJson.FromJson(scenesPaths, out cam);

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"Scene loading took {elapsedMs}ms");

        int threads = Environment.ProcessorCount;
        if (threads < 1)
            threads = 1;

        //sc.RenderScene(cam).SaveAs(outputPath);
        sc.RenderSceneParallel(cam,threads).SaveAs(outputPath);
        return;
    }
}
