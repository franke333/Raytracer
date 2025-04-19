using OpenTK.Mathematics;

namespace rt004.Rendering;

public abstract class Light
{

    public float intensity;
    public Vector3 color;

    protected const double EPSILON = 1e-6;

    public abstract Vector3 LightContribution(RayHitInfo rayHitInfo);

    public abstract bool InShadow(Vector3d worldPoint, Scene scene);
}


public class PointLight : Light
{
    public Vector3d position;
    public double c0, c1, c2;

    private bool _useDistanceAttenuation = false;

    public PointLight(Vector3d position, float intensity, Vector3 color, double c0 = 1, double c1 = 0, double c2 = 0)
    {
        this.position = position;
        this.intensity = intensity;
        this.color = color;
        this.c0 = c0;
        this.c1 = c1;
        this.c2 = c2;
        if (c1 != 0 || c2 != 0)
            _useDistanceAttenuation = true;
    }

    public override bool InShadow(Vector3d worldPoint, Scene scene)
    {
        
        var rhi = scene.RayCast(Ray.FromTo(worldPoint, this.position));
        return rhi.HasHit && rhi.t*rhi.t < (position-worldPoint).LengthSquared;
    }

    public override Vector3 LightContribution(RayHitInfo rayHitInfo)
    {
        Material mat = rayHitInfo.solid.material;

        //TODO check if Multiply for color mixing is okay
        Vector3d lightVector = (position - rayHitInfo.Point);
        Vector3d lightDir = lightVector.Normalized();

        double lv = Vector3d.Dot(lightDir, rayHitInfo.normal);
        Vector3 diff = intensity * mat.k_d * Vector3.Multiply(color, mat.color) * (float)Math.Max(0, lv);

        double cosB = Vector3d.Dot(lightDir, rayHitInfo.Reflectence);
        Vector3 spec = Vector3.Zero;
        if (cosB > 0)
            spec = intensity * mat.k_s * color * (float)Math.Pow(cosB, mat.h);
        float d = _useDistanceAttenuation ? 1f/(float)(c0 + c1 * lightVector.Length + c2 * lightVector.LengthSquared) : 1f; 
        return d * (diff + spec);
    }
}

public class AmbientLight : Light
{
    public AmbientLight(float intensity, Vector3 color)
    {
        this.intensity = intensity;
        this.color = color;
    }

    public override bool InShadow(Vector3d worldPoint, Scene scene) => false;

    public override Vector3 LightContribution(RayHitInfo rayHitInfo)
    {
        Material mat = rayHitInfo.solid.material;
        return intensity * mat.k_a * Vector3.Multiply(color, mat.color);
    }
}

public class DirectionalLight : Light
{
    public Vector3 direction;
    public DirectionalLight(Vector3 direction, float intensity, Vector3 color)
    {
        this.direction = direction;
        this.intensity = intensity;
        this.color = color;
    }

    public override bool InShadow(Vector3d worldPoint, Scene scene)
    {
        Ray r = Ray.FromTo(worldPoint, worldPoint-direction);
        return scene.RayCast(r).HasHit;
    }

    public override Vector3 LightContribution(RayHitInfo rayHitInfo)
    {
        Material mat = rayHitInfo.solid.material;
        Vector3d lightVector = -direction.Normalized();

        double lv = Vector3d.Dot(lightVector, rayHitInfo.normal);
        Vector3 diff = intensity * mat.k_d * Vector3.Multiply(color, mat.color) * (float)Math.Max(0, lv);

        double cosB = Vector3d.Dot(-lightVector, rayHitInfo.Reflectence);
        Vector3 spec = Vector3.Zero;
        if (cosB > 0)
            spec = intensity * mat.k_s * color * (float)Math.Pow(cosB, mat.h);
        return diff + spec;
    }
}