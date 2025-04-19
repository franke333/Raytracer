using OpenTK.Mathematics;
namespace rt004.Solids
{
    //axis aligned bounding box
    internal class AABB
    {
        public Vector3d Min { get; private set; }
        public Vector3d Max { get; private set; }

        public AABB(Vector3d min, Vector3d max)
        {
            Min = min;
            Max = max;
        }

        public AABB(Triangle[] triangles, int offset, bool[] flags, bool currentFlag)
        {
            double minX = float.MaxValue;
            double minY = float.MaxValue;
            double minZ = float.MaxValue;
            double maxX = float.MinValue;
            double maxY = float.MinValue;
            double maxZ = float.MinValue;

            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i] != currentFlag)
                    continue;
                Triangle triangle = triangles[offset+i];

                minX = Math.Min(minX, triangle.p0.X);
                minX = Math.Min(minX, triangle.p1.X);
                minX = Math.Min(minX, triangle.p2.X);

                minY = Math.Min(minY, triangle.p0.Y);
                minY = Math.Min(minY, triangle.p1.Y);
                minY = Math.Min(minY, triangle.p2.Y);

                minZ = Math.Min(minZ, triangle.p0.Z);
                minZ = Math.Min(minZ, triangle.p1.Z);
                minZ = Math.Min(minZ, triangle.p2.Z);

                maxX = Math.Max(maxX, triangle.p0.X);
                maxX = Math.Max(maxX, triangle.p1.X);
                maxX = Math.Max(maxX, triangle.p2.X);

                maxY = Math.Max(maxY, triangle.p0.Y);
                maxY = Math.Max(maxY, triangle.p1.Y);
                maxY = Math.Max(maxY, triangle.p2.Y);

                maxZ = Math.Max(maxZ, triangle.p0.Z);
                maxZ = Math.Max(maxZ, triangle.p1.Z);
                maxZ = Math.Max(maxZ, triangle.p2.Z);
            }
            Min = new Vector3d(minX, minY, minZ);
            Max = new Vector3d(maxX, maxY, maxZ);
        }


        public AABB(Triangle[] triangles, int startIndex, int triangleCount)
        {
            double minX = float.MaxValue;
            double minY = float.MaxValue;
            double minZ = float.MaxValue;
            double maxX = float.MinValue;
            double maxY = float.MinValue;
            double maxZ = float.MinValue;

            for (int i = 0; i < triangleCount; i++)
            {
                Triangle triangle = triangles[startIndex + i];

                minX = Math.Min(minX, triangle.p0.X);
                minX = Math.Min(minX, triangle.p1.X);
                minX = Math.Min(minX, triangle.p2.X);

                minY = Math.Min(minY, triangle.p0.Y);
                minY = Math.Min(minY, triangle.p1.Y);
                minY = Math.Min(minY, triangle.p2.Y);

                minZ = Math.Min(minZ, triangle.p0.Z);
                minZ = Math.Min(minZ, triangle.p1.Z);
                minZ = Math.Min(minZ, triangle.p2.Z);

                maxX = Math.Max(maxX, triangle.p0.X);
                maxX = Math.Max(maxX, triangle.p1.X);
                maxX = Math.Max(maxX, triangle.p2.X);

                maxY = Math.Max(maxY, triangle.p0.Y);
                maxY = Math.Max(maxY, triangle.p1.Y);
                maxY = Math.Max(maxY, triangle.p2.Y);

                maxZ = Math.Max(maxZ, triangle.p0.Z);
                maxZ = Math.Max(maxZ, triangle.p1.Z);
                maxZ = Math.Max(maxZ, triangle.p2.Z);
            }

            Min = new Vector3d(minX, minY, minZ);
            Max = new Vector3d(maxX, maxY, maxZ);
        }

        public bool Intersect(Ray ray, out double t)
        {
            double t1 = (Min.X - ray.P0.X) / ray.p1.X;
            double t2 = (Max.X - ray.P0.X) / ray.p1.X;
            double t3 = (Min.Y - ray.P0.Y) / ray.p1.Y;
            double t4 = (Max.Y - ray.P0.Y) / ray.p1.Y;
            double t5 = (Min.Z - ray.P0.Z) / ray.p1.Z;
            double t6 = (Max.Z - ray.P0.Z) / ray.p1.Z;

            double tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            double tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));
            t = tmin;
            if(tmax < 0)
                return false;
            return tmin < tmax;
        }

        public int MaxAxis()
        {
            Vector3d diag = Max - Min;
            if (diag.X > diag.Y && diag.X > diag.Z)
                return 0;
            if (diag.Y > diag.Z)
                return 1;
            return 2;
        }

        public double Cost()
        {
            Vector3d diag = Max - Min;
            //surface area
            if(diag.X == 0 || diag.Y == 0 || diag.Z == 0)
                return 0;
            double sa = 2 * (diag.X * diag.Y + diag.X * diag.Z + diag.Y * diag.Z);
            return sa;
        }
    }
}
