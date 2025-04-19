using OpenTK.Mathematics;
using System.ComponentModel;

namespace rt004;

public class Camera
{
    public enum CameraRenderMode
    {
        Color,Normal,Depth
    }
    protected int canvasPixelsWidth, canvasPixelsHeight;
    public int Width => canvasPixelsWidth;
    public int Height => canvasPixelsHeight;

    protected double FOV;
    protected double canvasWidth, canvasHeight;
    //LeftCenter
    Vector3d LC;
    //LeftUpperCorner
    Vector3d LU;

    Vector3d camPosition;
    Vector3d camUp, camRight,camView; //camera space basis
    Vector3d worldUp;

    public int maxDepth = 3;

    // sampler per pixel
    public int spp;

    protected double pixelSize;

    public CameraRenderMode renderMode = CameraRenderMode.Color;

    /// <summary>
    /// Creates a camera object
    /// </summary>
    /// <param name="canvasPixelsWidth">number of pixels on x axis</param>
    /// <param name="canvasPixelsHeight">number of pixels on y axis</param>
    /// <param name="position">origin of camera</param>
    /// <param name="lookAtPoint">point that the camera will look at</param>
    /// <param name="FOV">Field of view in degrees</param>
    /// <param name="maxDepth">maximum depth of ray recursion</param>
    /// <param name="spp">samples per pixel</param>
    public Camera(int canvasPixelsWidth, int canvasPixelsHeight,
        Vector3d position, Vector3d lookAtPoint, Vector3d worldUp, float FOV = 90, int maxDepth = 3, int spp = 1, CameraRenderMode mode = CameraRenderMode.Color)
    {
        //normalize
        worldUp.Normalize();

        //ray recursion
        this.maxDepth = maxDepth;
        //samples per pixel
        this.spp = spp;

        //mode
        renderMode = mode;

        //canvas size
        this.canvasPixelsWidth = canvasPixelsWidth;
        this.canvasPixelsHeight = canvasPixelsHeight;
        camPosition = position;


        //convert from degrees to radians
        FOV = Math.Clamp(FOV, 0.1f, 170f);
        this.FOV = FOV * Math.PI / 180;

        // calculate camera space basis
        camView = (lookAtPoint - position).Normalized();

        camRight = Vector3d.Cross(worldUp, camView);
        camUp = Vector3d.Cross(camView, camRight);

        //Console.WriteLine($"View : {camView}");
        //Console.WriteLine($"Up : {camUp}");
        //Console.WriteLine($"Right : {camRight}");



        Vector3d centerOfCanvas = camPosition + camView;


        //1 -> length of the adjacent side
        this.canvasWidth = 1 * Math.Tan(this.FOV / 2) * 2;
        this.canvasHeight = canvasWidth * canvasPixelsHeight / canvasPixelsWidth;

        this.LC = centerOfCanvas - camRight * canvasWidth/2;
        this.LU = LC + camUp * canvasHeight / 2;

        this.pixelSize = canvasWidth / canvasPixelsWidth;
    }

    public Ray GenerateRay(int x, int y, int sample = 0)
    {
        if(x < 0 || x >= canvasPixelsWidth || y < 0 || y >= canvasPixelsHeight)
            return Ray.Invalid();
        if(spp == 1 )
            return Ray.FromTo(camPosition, LU + (y + 0.5) * -camUp * pixelSize + (x + 0.5) * camRight * pixelSize);

        //jittering
        double microPixelSize = 1.0 / spp;
        double dx = ((sample % spp) + Random.Shared.NextDouble()) * microPixelSize;
        double dy = ((sample / spp) + Random.Shared.NextDouble()) * microPixelSize;
        return Ray.FromTo(camPosition, LU + (y + dy) * -camUp * pixelSize + (x + dx) * camRight * pixelSize);

    }

    public IEnumerable<Ray> EnumerateThroughMacroPixel(int x,int y)
    {
        for (int sample = 0; sample < spp*spp; sample++)
            yield return GenerateRay(x,y,sample);
    }

    public IEnumerator<Ray> GetEnumerator()
    {
        for (int y = 0; y < canvasPixelsHeight; y++)
            for (int x = 0; x < canvasPixelsWidth; x++)
                yield return GenerateRay(x, y);
    }
}
