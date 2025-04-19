using OpenTK.Mathematics;
using System.Globalization;

namespace rt004.Utility
{
    class TriangleMeshOBJData
    {
        public List<Vector3d> vertices = new List<Vector3d>();
        public List<Vector3i> triangles = new List<Vector3i>();
    }
    internal class ImportMesh
    {
        public static TriangleMeshOBJData ImportOBJ(string path)
        {
            //lets import .obj format
            // # are lines with comments
            // v are vertices
            // f are faces  (lets assume only triangles, but obj allows polygons and so on)

            StreamReader sr = new StreamReader(path);
            TriangleMeshOBJData data = new TriangleMeshOBJData();

            while(!sr.EndOfStream)
            {
                string? line = sr.ReadLine();
                if (line == null)
                    break;
                if (line.StartsWith("#"))
                    continue;
                string[] parts = line.Split(' ');
                if (parts[0] == "v")
                {
                    double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                    double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    double z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                    data.vertices.Add(new Vector3d(x, y, z));
                }
                else if (parts[0] == "f")
                {
                    int v0 = int.Parse(parts[1]) -1;
                    int v1 = int.Parse(parts[2]) -1;
                    int v2 = int.Parse(parts[3]) -1;
                    data.triangles.Add(new Vector3i(v0, v1, v2));
                }
            }
            sr.Close();
            return data;
        }
    }
}
