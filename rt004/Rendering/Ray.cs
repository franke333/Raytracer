using OpenTK.Mathematics;
using rt004.Rendering;

namespace rt004;

//parametric representation of ray: P0 + t*p1
public struct Ray
{
    //point of origin
    public Vector3d P0;

    //direction unit vector
    public Vector3d p1;

    public bool IsValid() => p1 != Vector3d.Zero;

    public Vector3d At(double t) => P0 + t * p1;

    public Ray(Vector3d P0, Vector3d p1)
    {
        this.P0 = P0;
        this.p1 = p1.Normalized();
    }

    public Ray ApplyTransform(Transform transform) => new Ray(transform.TransformPoint(P0), transform.TransformVector(p1));


    static public Ray FromTo(Vector3d from,Vector3d to) => new Ray(from, to-from);

    

    static public Ray Invalid() => new Ray(Vector3d.Zero, Vector3d.Zero);
}

public class RayHitInfo
{
    public Ray ray;
    public double t;
    public ISolid? solid;

    //data might be used in other calculations
    public object? data;

    private Vector3d _normal;

    public Vector3d normal
    {
        get => _normal;
        set => _normal = value.Normalized();
    }
    public Vector3d Point => ray.At(t);
    public bool HasHit => solid != null;

    public Vector3d Reflectence => (ray.p1 - 2 * _normal * Vector3d.Dot(_normal, ray.p1)).Normalized();

    public Vector3d Refractance(Material otherMaterial)
    {
        float otherN = otherMaterial?.n ?? 1; //otherMaterial ?? air
        float n = solid!.material.n;

        var D = ray.p1;
        var N = normal;

        // hitting solid from inside
        if (Vector3d.Dot(normal, ray.p1) > 0)
        {
            var temp = n;
            n = otherN;
            otherN = temp;
            N = -N;
        }


        var sinA = Vector3d.Cross(D, N).Length;
        var B = (D - N * Vector3d.Dot(D, N)) / sinA;
        var sinB = sinA * otherN / n;
        var cosB = Math.Sqrt(1 - sinB * sinB);
        return sinB * B - cosB * N;

    }

    public RayHitInfo ApplyTransform(Transform transform)
    {
        return new RayHitInfo()
        {
            ray = ray.ApplyTransform(transform),
            t = t * transform.TransformVector(ray.p1).Length, // yeah. the normalized rays are biting me back now...
            solid = solid,
            data = data,
            normal = transform.TransformVector(_normal),
        };
    }
}