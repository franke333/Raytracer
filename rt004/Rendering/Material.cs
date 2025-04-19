using OpenTK.Mathematics;

namespace rt004.Rendering;

public class Material
{
    public float k_d, k_s, k_a, h, alpha, n;
    public Vector3 color;
    public IBRDF brdf;

    public Material(float k_d, float k_s, float k_a, float h, float alpha, float n, Vector3 color, IBRDF brdf = null)
    {
        this.k_d = k_d;
        this.k_s = k_s;
        this.k_a = k_a;
        this.h = h;
        this.alpha = alpha;
        this.n = n;
        this.color = color;
        //Default phong
        this.brdf = brdf ?? PhongBRDF.Instance;
    }
}
