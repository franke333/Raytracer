using OpenTK.Mathematics;
using rt004.Rendering;

namespace rt004;

internal class SolidSphere : ISolid
{
    public SolidSphere(Transform transform, Material material) : base(transform, material)
    {
    }

    //public Vector3d center; // [0,0,0]
    //public double radius; // 1

    public SolidSphere() : base(new Transform(), new Material(0, 0, 0, 0,0,0, Vector3.Zero))
    {
    }

    public override RayHitInfo Calculate(RayHitInfo ray, byte flag)
    {
        //TODO normal is easy, so it is calculated everytime (-1 flag check)
        //But for UV, the calculation will be done here ONLY IF it asked by the flag
        return ray;
    }

    public override ISolid CreateInstanceOf(Transform transform, Material material) => new SolidSphere(transform, material);

    protected override RayHitInfo IntersectLocal(Ray ray, byte flag = (byte)IntersectFlag.All)
    {
        double a = Vector3d.Dot(ray.p1, ray.p1);
        double b = 2 * Vector3d.Dot(ray.p1, ray.P0);
        double c = Vector3d.Dot(ray.P0, ray.P0) - 1;
        double discriminant = b * b - 4 * a * c;
        if (discriminant <= 0)
            return new RayHitInfo { ray = ray };
        double t1 = (-b - Math.Sqrt(discriminant)) / (2 * a);
        if (t1 <= ISolid.EPSILON)
        {
            double t2 = (-b + Math.Sqrt(discriminant)) / (2 * a);
            if (t2 <= ISolid.EPSILON)
                return new RayHitInfo { ray = ray };
            t1 = t2;
        }
        Vector3d normal = ray.At(t1);
        var hitInfo = new RayHitInfo { ray = ray, t = t1, solid = this, normal = normal };
        //return Calculate(hitInfo, flag);
        return hitInfo;
    }

    public override List<RayHitInfo> IntersectAll(Ray ray, byte flag = (byte)IntersectFlag.AllSort)
    {
        double a = Vector3d.Dot(ray.p1, ray.p1);
        double b = 2 * Vector3d.Dot(ray.p1, ray.P0);
        double c = Vector3d.Dot(ray.P0, ray.P0) - 1;
        double discriminant = b * b - 4 * a * c;
        var list = new List<RayHitInfo>();
        if (discriminant <= 0)
            return list;
        discriminant = Math.Sqrt(discriminant);
        foreach (var sign in new int[] { -1, 1 })
        {
            double t = (-b - Math.Sqrt(discriminant)) / (2 * a);
            if (t > ISolid.EPSILON)
                list.Add(new RayHitInfo()
                {
                    ray = ray,
                    t = t,
                    normal = ray.At(t),
                });
        }
        list.Select(rhi => Calculate(rhi, flag)).ToList();
        return list;
    }
}
