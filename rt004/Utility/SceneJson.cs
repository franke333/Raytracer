using OpenTK.Mathematics;
using rt004.Rendering;
using rt004.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static rt004.Camera;
using static rt004.Utility.SceneGraph;

namespace rt004.Utility
{
    // This code is used to parse a json string into a scene and camera
    internal class SceneJson
    {
        public class TransformJson
        {
            public string Translation { get; set; }
            public string Scale { get; set; }
            public string Rotation { get; set; }

            public Transform ToTransform()
            {
                return new Transform(Translation.ParseToVector3d(), Rotation.ParseToVector3d(), Scale.ParseToVector3d());
            }
        }

        public class MaterialJson
        {
            public string Color { get; set; }
            public float A { get; set; }
            public float D { get; set; }
            public float S { get; set; }

            public float Alpha { get; set; }
            public float N { get; set; }
            public int H { get; set; }

            public Material ToMaterial()
            {
                return new Material(D, S, A, H, Alpha,N, Color.ParseToVector3());
            }
        }

        public class NodeJson
        {
            public string Solid { get; set; }
            public List<string> Children { get; set; }
            public List<Dictionary<string,string>> Transforms { get; set; }
            public string Material { get; set; }

            // path to mesh file
            public string Path { get; set; }

            private Node node;

            private static Dictionary<string,ISolid> premadeSolids = new Dictionary<string,ISolid>
            {
                { "Sphere", new SolidSphere() },
                { "Plane", new Plane() },
            };

            public Node ToNode()
            {
                if(Solid != null)
                {
                    if (premadeSolids.TryGetValue(Solid, out var solid))
                    {
                        node = new Leaf(solid);
                    }
                    else if(Solid == "Mesh")
                    {
                        if (Path == null)
                            throw new ArgumentException("Path not found for Mesh");
                        node = new Leaf(new TriangleMesh(Path));
                    }
                    else
                        throw new ArgumentException($"Solid {Solid} not found");
                }
                else
                    node = new Node();
                return node;
            }

            public void Populate(string name, Dictionary<string,Node> nodes, Dictionary<string,Material> material)
            {
                if (Material != null)
                {
                    if(material.ContainsKey(Material))
                        node.SetMaterial(material[Material]);
                    else
                        throw new ArgumentException($"Material {Material} in node {name} not found");
                }
                if(Children == null && Transforms == null)
                    return;
                if(Children == null || Transforms == null || Children.Count != Transforms.Count)
                    throw new ArgumentException($"Node {name}: Children and Transforms must be the same length");
                for(int i = 0; i < Children.Count; i++)
                {
                    var position = Transforms[i].ContainsKey("Translation") ? Transforms[i]["Translation"].ParseToVector3d() : Vector3d.Zero;
                    var rotation = Transforms[i].ContainsKey("Rotation") ? Transforms[i]["Rotation"].ParseToVector3d() : Vector3d.Zero;
                    // degree to radian
                    rotation *= Math.PI/180;
                    var scale = Transforms[i].ContainsKey("Scale") ? Transforms[i]["Scale"].ParseToVector3d() : Vector3d.One;
                    Transform transform = new Transform(position, rotation, scale);
                    if (!nodes.ContainsKey(Children[i]))
                        throw new ArgumentException($"Node {Children[i]} in node {name} not found");
                    node.AddChild(nodes[Children[i]], transform.transformationMatrix);
                }
            }
        }

        public class LightJson
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Position { get; set; }
            public string Color { get; set; }
            public float Intensity { get; set; }

            public Light ToLight()
            {
                if(Type == "Point")
                    return new PointLight(Position.ParseToVector3d(), Intensity, Color.ParseToVector3());
                else if(Type == "Directional")
                    return new DirectionalLight(Position.ParseToVector3(), Intensity, Color.ParseToVector3());
                else if(Type == "Ambient")
                    return new AmbientLight(Intensity, Color.ParseToVector3());
                else
                    throw new ArgumentException($"Light type {Type} not found");
            }
        }

        public class CameraJson
        {
            public string Position { get; set; }
            public string LookAt { get; set; }
            public string Up { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public float Fov { get; set; }
            public int MaxDepth { get; set; }

            public int Spp { get; set; }

            public string Mode { get; set; }

            private void SetDefaults()
            {
                if (Position == null)
                    Position = "0 0 -1";
                if (LookAt == null)
                    LookAt = "0 0 0";
                if (Up == null)
                    Up = "0 1 0";
                if (Width == 0)
                    Width = 400;
                if (Height == 0)
                    Height = 400;
                if (Fov == 0)
                    Fov = 90;
                if (MaxDepth == 0)
                    MaxDepth = 0;
                if (Spp == 0)
                    Spp = 1;
                if (!Enum.TryParse(Mode, out CameraRenderMode _))
                    Mode = "Color";
            }

            public Camera ToCamera()
            {
                SetDefaults();
                return new Camera(Width, Height, Position.ParseToVector3d(), LookAt.ParseToVector3d(), Up.ParseToVector3d(), Fov, MaxDepth, Spp, Enum.Parse<CameraRenderMode>(Mode));
            }
        }

        public class SceneGraphJson
        {
            public Dictionary<string,NodeJson> Nodes { get; set; }
            public Dictionary<string, MaterialJson> Materials { get; set; }
            public List<LightJson> Lights { get; set; }

            public string BackgroundColor { get; set; }

            public CameraJson Camera { get; set; }

            public bool IsNonEmpty()
            {
                return Nodes != null || Materials != null || Lights != null || Camera != null;
            }
        }

        /// <summary>
        /// Creates a scene and camera from a json string
        /// </summary>
        /// <param name="jsonString">scene in json format</param>
        /// <param name="cam">camera extracted from json</param>
        public static Scene FromJson(List<string> jsonScenePaths, out Camera cam)

        {
            SceneGraphJson sceneGraphJson = null;

            // load all scenes, where each new scene overriedes old data if there is some
            foreach (var path in jsonScenePaths)
            {
                string json = System.IO.File.ReadAllText(path);
                SceneGraphJson overrideScene = JsonSerializer.Deserialize<SceneGraphJson>(json);
                if(overrideScene == null || !overrideScene.IsNonEmpty()){
                    Console.WriteLine($"{path} doesnt containt valid json scene");
                    continue;
                }
                if (sceneGraphJson != null)
                {
                    overrideScene.Lights ??= sceneGraphJson.Lights;

                    if(overrideScene.Materials != null && sceneGraphJson.Materials != null)
                    {
                        foreach (var (key, value) in overrideScene.Materials)
                        {
                            sceneGraphJson.Materials[key] = value;
                        }
                        overrideScene.Materials = sceneGraphJson.Materials;
                    }
                    else
                        overrideScene.Materials ??= sceneGraphJson.Materials;
                    if(overrideScene.Nodes != null && sceneGraphJson.Nodes != null)
                    {
                        foreach (var (key, value) in overrideScene.Nodes)
                        {
                            sceneGraphJson.Nodes[key] = value;
                        }
                        overrideScene.Nodes = sceneGraphJson.Nodes;
                    }
                    else
                        overrideScene.Nodes ??= sceneGraphJson.Nodes;
                    overrideScene.BackgroundColor ??= sceneGraphJson.BackgroundColor;
                    overrideScene.Camera ??= sceneGraphJson.Camera;
                }
                sceneGraphJson = overrideScene;
            }


            if (sceneGraphJson == null)
                throw new ArgumentException("Invalid JSON");
            SceneGraph sceneGraph = new SceneGraph();
            Scene scene = new Scene();

            scene.lights = new List<Light>();

            if(sceneGraphJson.Lights != null)
                foreach (var light in sceneGraphJson.Lights)
                    scene.lights.Add(light.ToLight());

            Dictionary<string, Node> nodes = new Dictionary<string, Node>();
            Dictionary<string, Material> materials = new Dictionary<string, Material>();

            if(sceneGraphJson.Materials != null)
                foreach (var material in sceneGraphJson.Materials)
                    materials.Add(material.Key, material.Value.ToMaterial());

            if(sceneGraphJson.Nodes != null)
                foreach (var node in sceneGraphJson.Nodes)
                    nodes.Add(node.Key, node.Value.ToNode());

            foreach(var (name,nodeJson) in sceneGraphJson.Nodes)
                nodeJson.Populate(name, nodes, materials);


            if (!nodes.ContainsKey("root"))
                throw new ArgumentException("Root node not found");

            if (sceneGraphJson.BackgroundColor != null)
                scene.backgroundColor = sceneGraphJson.BackgroundColor.ParseToVector3();

            sceneGraph.root = nodes["root"];

            scene.solids = sceneGraph.CreateSolidsList();

            // try use scene camera, then try use scene default camera, then use default camera
            cam = sceneGraphJson.Camera?.ToCamera() ?? new CameraJson().ToCamera();

            return scene;
        }
    }
}
