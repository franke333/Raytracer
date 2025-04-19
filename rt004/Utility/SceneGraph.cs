using OpenTK.Mathematics;
using rt004.Rendering;
using System.Text.Json;

namespace rt004.Utility
{
    class SceneGraph
    {
        public Node root;

        public class Attributes
        {
            public Material? material;
            public Texture? texture;

            /// <summary>
            /// Update the attributes with the other attributes having priority
            /// </summary>
            /// <param name="other">priority attributes</param>
            /// <returns>new attribute list</returns>
            public Attributes UpdateAtrributes(Attributes other)
            {
                return new Attributes
                {
                    material = other.material ?? material,
                    texture = other.texture ?? texture
                };
            }
        }


        public class Node
        {
            public virtual bool IsSolidLeaf => false;
            public Attributes? attributes;

            public List<Node> children;
            public List<Matrix4d> childTransforms;

            public Node()
            {
                children = new List<Node>();
                childTransforms = new List<Matrix4d>();
            }

            public void AddChild(Node child, Matrix4d transform)
            {
                children.Add(child);
                childTransforms.Add(transform);
            }

            public void SetMaterial(Material material)
            {
                if (attributes == null)
                    attributes = new Attributes();
                attributes.material = material;
            }

            public void SetTexture(Texture texture)
            {
                if (attributes == null)
                    attributes = new Attributes();
                attributes.texture = texture;
            }
        }

        public class Leaf : Node
        {
            public ISolid solid;
            public Leaf(ISolid solid) => this.solid = solid;
            public override bool IsSolidLeaf => true;
        }

        /// <summary>
        /// Traverse the graph scene and create a list of all solids. Creating a new instance of each solid
        /// </summary>
        /// <returns>List of all solids contained in scene</returns>
        public List<ISolid> CreateSolidsList()
        {
            return ProcessNode(Matrix4d.Identity, new Attributes(), root);
        }

        private List<ISolid> ProcessNode(Matrix4d transformMatrix, Attributes attributes, Node node)
        {
            if (node.attributes != null)
                attributes = attributes.UpdateAtrributes(node.attributes!);
            if (node.IsSolidLeaf)
            {
                var leaf = (Leaf)node;
                var solid = leaf.solid.CreateInstanceOf(new Transform(transformMatrix), attributes.material!);
                return new List<ISolid> { solid };
            }
            else
            {
                List<ISolid> solids = new();
                for (int i = 0; i < node.children.Count; i++)
                {
                    var child = node.children[i];
                    var childTransform = node.childTransforms[i];
                    var newTransformation = childTransform * transformMatrix;
                    var childSolids = ProcessNode(newTransformation, attributes, child);
                    solids.AddRange(childSolids);
                }
                return solids;
            }
        }


    }

}
