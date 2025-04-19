using rt004.Rendering;
using OpenTK.Mathematics;
using rt004.Utility;
using System.Diagnostics;

namespace rt004.Solids
{
    internal class TriangleMesh : ISolid
    {
        Triangle[] triangles;
        string path;

        BVH bvh;

        public TriangleMesh(Transform transform, Material material, string path) : base(transform, material)
        {
            TriangleMeshOBJData data = ImportMesh.ImportOBJ(path);
            triangles = new Triangle[data.triangles.Count];
            for (int i = 0; i < data.triangles.Count; i++)
            {
                Vector3i t = data.triangles[i];
                //TODO check ccw/cw
                triangles[i] = new Triangle(data.vertices[t.X], data.vertices[t.Y], data.vertices[t.Z]);
            }
            this.path = path;
            bvh = new BVH(triangles, 40, 4);
        }

        private TriangleMesh(Transform transform, Material material, TriangleMesh other) : base(transform, material)
        {
            triangles = other.triangles;
            path = other.path;
            bvh = other.bvh;
        }

        public TriangleMesh(string path) : this(new Transform(), new Material(0, 0, 0, 0, 0, 0, Vector3.Zero), path)
        {
        }

        public override RayHitInfo Calculate(RayHitInfo ray, byte flag)
        {
            //unused as of now
            return ray;
        }

        public override ISolid CreateInstanceOf(Transform transform, Material material)
        {
            return new TriangleMesh(transform, material, this);
        }

        public override List<RayHitInfo> IntersectAll(Ray ray, byte flag = 95)
        {
            throw new NotImplementedException();
        }

        protected override RayHitInfo IntersectLocal(Ray ray, byte flag = 31)
        {
            var hitInfo = bvh.Intersect(ray);
            if (hitInfo.t != double.MaxValue)
            {
                hitInfo.solid = this;
            }
            return hitInfo;
        }
    }


    internal struct Triangle
    {
        //Defined by 1 point and 2 edges
        public Vector3d p0, e1, e2;

        public Vector3d p1 { get => p0 + e1; }
        public Vector3d p2 { get => p0 + e2; }

        public Vector3d normal { get => Vector3d.Cross(e1, e2); }

        public Vector3d center { get => p0 + (e1 + e2) / 3; }

        //CCW
        public Triangle(Vector3d p0, Vector3d p1, Vector3d p2)
        {
            this.p0 = p0;
            e1 = p1 - p0;
            e2 = p2 - p0;
        }

        public bool Intersect(Ray ray, out RayHitInfo rayHitInfo) {             
            rayHitInfo = new RayHitInfo { ray = ray };
            Vector3d p = Vector3d.Cross(ray.p1, e2);
            double det = Vector3d.Dot(e1, p);
            if (det > -ISolid.EPSILON && det < ISolid.EPSILON)
                        return false;
            double invDet = 1 / det;
            Vector3d t = ray.P0 - p0;
            double u = Vector3d.Dot(t, p) * invDet;
            if (u < 0 || u > 1)
                        return false;
            Vector3d q = Vector3d.Cross(t, e1);
            double v = Vector3d.Dot(ray.p1, q) * invDet;
            if (v < 0 || u + v > 1)
                        return false;
            double t0 = Vector3d.Dot(e2, q) * invDet;
            if (t0 < ISolid.EPSILON)
                        return false;
            rayHitInfo = new RayHitInfo { ray = ray, t = t0, normal = normal };
            return true;
        }
    }
}
