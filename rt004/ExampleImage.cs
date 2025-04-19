using System.CommandLine;
using System.Globalization;
using Util;

namespace rt004;

class ExampleCommand : ICommand
{
    //TODO make common arguments/options easy to reuse (probably using some black magic, try reading https://learn.microsoft.com/en-us/dotnet/standard/commandline/model-binding)
    // -> (sub)commands can be in file with the appropriate piece of program -> clean
    // -> or use params

    public Command GetCommand()
    {
        var exampleCommand = new Command("example", "Creates an image example");
        var widthArgument = new Argument<int>
            (
            name: "width",
            description: "width of final image in pixels",
            getDefaultValue: () => 800
            );
        var heightArgument = new Argument<int>
            (
            name: "height",
            description: "height of the final image in pixels",
            getDefaultValue: () => 600
            );
        var pathArgument = new Argument<string>
            (
            name: "fileName",
            description: "name/path of the output image",
            getDefaultValue: () => "default.pfm"
            );

        //this would be the only argument in example project
        var freqArgument = new Argument<float>
            (
            name: "frequency",
            description: "frequency of the hue change",
            //getDefaultValue: () => 3.33f
            parse: result =>
            {
                //default
                if(!result.Tokens.Any())
                {
                    Console.WriteLine("Using default");
                    return 33.3f;
                }
                //take the last instance of this
                var thingToParse = result.Tokens[result.Tokens.Count-1].Value;
                if (float.TryParse(thingToParse, CultureInfo.InvariantCulture,out var parsedResult))
                {
                    return parsedResult;
                }
                else
                {
                    result.ErrorMessage = "Could not parse " + thingToParse + " in frequency argument";
                    return 0; // Ignored.
                }
            }
            );

        exampleCommand.Add(widthArgument);
        exampleCommand.Add(heightArgument);
        exampleCommand.Add(pathArgument);
        exampleCommand.Add(freqArgument);

        exampleCommand.SetHandler((width, height, path, freq) =>
        {
            ExampleImage.DrawExample(width, height, path, freq);
        }, widthArgument, heightArgument, pathArgument, freqArgument);
        return exampleCommand;
    }
}

internal class ExampleImage
{
    // Custom HSV to RGB conversion where SV are fixed to 1
    // somehow OpenTK totally ignores H???? it just checks which of the 6 sectors the hue is in but thats the end of Hue usage (so it can create only 6 colors)
    // Color3<Hsv>().ToRgb();
    public static float[] ConvertHuetoSaturatedRGB(float hue)
    {
        int hi = Convert.ToInt32(Math.Floor(hue*6)) % 6;
        float f = hue*6 - (float)Math.Floor(hue*6);
        switch (hi)
        {
            case 0:
                return new float[] { 1, f, 0 };
            case 1:
                return new float[] { 1 - f, 1, 0 };
            case 2:
                return new float[] { 0, 1, f };
            case 3:
                return new float[] { 0, 1 - f, 1 };
            case 4:
                return new float[] { f, 0, 1 };
            default:
                return new float[] { 1, 0, 1 - f };
        }
    }
public static void DrawExample(int width, int height, string path, float frequency)
    {

        // HDR image.
        FloatImage fi = new(width, height, 3);

        for(int x=0; x<width; x++)
            for(int  y=0; y<height; y++)
            {
                float dist = 0;
                int ax = Math.Abs(x - width / 2);
                int ay = Math.Abs(y - height / 2);
                int quart = ((x > width / 2) ? 1 : 0) + ((y > height / 2) ? 1 : 0);
                
                switch (quart)
                {
                    case 0:
                        dist = MathF.Sqrt(ax * ax + ay * ay);
                        break;
                    case 1:
                        dist = ax + ay;
                        break;
                    default:
                        dist = MathF.Max(ax, ay);
                        break;
                }
                float hue = dist*frequency / width;
                var rgb = ConvertHuetoSaturatedRGB(hue);
                fi.PutPixel(x, y, rgb);
            }

        fi.SavePFM(path); 

        Console.WriteLine($"HDR image '{path}' is finished.");
    }
}

