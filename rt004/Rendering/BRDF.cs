

using OpenTK.Mathematics;

namespace rt004.Rendering;

public interface IBRDF
{
    Vector3 CalculateColor(RayHitInfo rayHitInfo, Scene scene);
}

public class PhongBRDF : IBRDF
{
    private static PhongBRDF _instance = null;
    public static PhongBRDF Instance
    {
        get
        {
            if( _instance == null )
                _instance = new PhongBRDF();
            return _instance;
        }
    }

    public Vector3 CalculateColor(RayHitInfo rayHitInfo, Scene scene)
    {
        Vector3 color = Vector3.Zero;
        foreach (Light l in scene.lights)
        {
            if (!l.InShadow(rayHitInfo.Point, scene))
                color += l.LightContribution(rayHitInfo);
        }
        return color;
    }
}
