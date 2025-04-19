using OpenTK.Mathematics;
using Util;

namespace rt004.Utility;

public class Canvas
{
    public int width, height, channels;
    public float[,,] canvas;

    public Canvas(int width, int height, int channels)
    {
        this.width = width;
        this.height = height;
        this.channels = channels;
        canvas = new float[width, height, channels];
    }

    public void SetPixel(int x, int y, Vector3 color)
    {
        for (int c = 0; c < channels; c++)
            canvas[x, y, c] = color[c];
    }

    public void SetPixel(int x, int y, float[] pixel)
    {
        for (int c = 0; c < channels; c++)
            canvas[x, y, c] = pixel[c];
    }

    public void SaveAs(string path)
    {
        FloatImage fi = new(width, height, channels);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float[] pixel = new float[channels];
                for (int c = 0; c < channels; c++)
                    pixel[c] = canvas[x, y, c];
                fi.PutPixel(x, y, pixel);
            }
        fi.SavePFM(path);
        Console.WriteLine($"HDR image '{path}' is finished.");
    }
}
