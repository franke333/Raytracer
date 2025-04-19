using OpenTK.Mathematics;
using rt004.Rendering;

namespace rt004;

internal class Plane : ISolid
{
    private Vector3d normal = Vector3d.UnitY; // [0,1,0]
    // public double D; // 0

    public Plane(Transform transform, Material material) : base(transform, material)
    {
    }

    public Plane():base(new Transform(), new Material(0, 0, 0, 0,0,0, Vector3.Zero))
    {
    }

    public override RayHitInfo Calculate(RayHitInfo ray, byte flag)
    {
        return ray;
    }

    public override ISolid CreateInstanceOf(Transform transform, Material material) => new Plane(transform, material);

    protected override RayHitInfo IntersectLocal(Ray ray, byte flag = 31)
    {
        double np = Vector3d.Dot(normal, ray.p1);
        if (np == 0)
            return new RayHitInfo { ray = ray };
        double sign = np > 0 ? 1 : -1;
        double t = -Vector3d.Dot(ray.P0, normal) / np;
        if (t <= ISolid.EPSILON)
            return new RayHitInfo { ray = ray };
        
        //TODO Calculate when calculate does something
        return new RayHitInfo { ray = ray, t = t, solid = this, normal = normal };
    }

    public override List<RayHitInfo> IntersectAll(Ray ray, byte flag = (byte)IntersectFlag.AllSort)
    {
        var r = Intersect(ray, flag);
        return r.HasHit ? new List<RayHitInfo>() { r } : new List<RayHitInfo>();
    }
}
