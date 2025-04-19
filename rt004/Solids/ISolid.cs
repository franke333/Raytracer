using OpenTK.Mathematics;
using rt004.Rendering;

namespace rt004;

public enum IntersectFlag : byte {
    //TODO we will add computational flags here
    Normal = 1,
    UV = 2,

    //RayHitInfo will containt Data from previous calculations that can be reused
    UseData = 32,
    Sort = 64,

    All = 31,
    AllSort = 95,
}

public abstract class ISolid
{
    public const double EPSILON = 1e-6;

    public ISolid(Transform transform, Material material)
    {
        this.transform = transform;
        this.material = material;
    }
    // returns closest intersect
    public RayHitInfo Intersect(Ray ray, byte flag = (byte)IntersectFlag.All)
    {
        Ray rayLocal = ray.ApplyTransform(transform.Inverse);
        RayHitInfo hitInfo = IntersectLocal(rayLocal, flag);
        return hitInfo.HasHit ? hitInfo.ApplyTransform(transform) : hitInfo;
    }

    protected abstract RayHitInfo IntersectLocal(Ray ray, byte flag = (byte)IntersectFlag.All);

    public abstract RayHitInfo Calculate(RayHitInfo ray, byte flag);

    //returns all intersections
    //TODO make protected and create a public method transform the rays into local/world space
    public abstract List<RayHitInfo> IntersectAll(Ray ray, byte flag = (byte)IntersectFlag.AllSort);

    public Transform transform { get; set; }

    public Material material { get; set; }

    public abstract ISolid CreateInstanceOf(Transform transform, Material material);

}
