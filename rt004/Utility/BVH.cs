using rt004.Solids;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rt004.Utility
{
    internal class BVH
    {
        class BVHNode
        {

            public int startIndex, triangleCount;
            public AABB aabb;
            public BVHNode left, right;


            public BVHNode(Triangle[] triangles, int startIndex, int triangleCount, int maxDepth, int minTriangles = 1)
            {

                aabb = new AABB(triangles, startIndex, triangleCount);

                this.startIndex = startIndex;
                this.triangleCount = triangleCount;

                if (triangleCount < 2 * minTriangles || maxDepth == 0)
                    return;


                //sort current triangles along axis
                int longestAxis = aabb.MaxAxis();
                var currentTriangles = triangles.Skip(startIndex).Take(triangleCount).ToArray();
                Array.Sort(currentTriangles, (t1, t2) => t1.center[longestAxis].CompareTo(t2.center[longestAxis]));
                for (int i = 0; i < triangleCount; i++)
                    triangles[startIndex + i] = currentTriangles[i];

                // if only one split is possible
                if(triangleCount == 2*minTriangles)
                {
                    left = new BVHNode(triangles, startIndex, minTriangles, 0, minTriangles);
                    right = new BVHNode(triangles, startIndex + minTriangles, minTriangles, 0, minTriangles);
                    return;
                }


                //try numerous splits and choose the best one
                int candidates = Math.Min(32, triangleCount - 1);
                float step = triangleCount / (float)(candidates + 1);

                double[] costs = new double[candidates - 2];
                Parallel.For(1, candidates - 1, i =>
                {
                    int index = i % candidates;
                    int leftCount = (int)(index * step);

                    AABB leftAABB = new AABB(triangles, startIndex, leftCount);
                    AABB rightAABB = new AABB(triangles, startIndex + leftCount, triangleCount - leftCount);
                    if(leftAABB.Cost() == 0 || rightAABB.Cost() == 0)
                    {
                        costs[i - 1] = double.MaxValue;
                    }
                    else
                        costs[i - 1] = leftAABB.Cost() * leftCount + rightAABB.Cost() * (triangleCount - leftCount);
                });

                //find lowest cost index
                int bestIndex = 0;
                double bestCost = double.MaxValue;
                for (int i = 0; i < candidates - 2; i++)
                    if (costs[i] < bestCost)
                    {
                        bestCost = costs[i];
                        bestIndex = i;
                    }

                //split by it
                bestIndex++;
                int mid = (int)(bestIndex*step);

                left = new BVHNode(triangles, startIndex, mid, maxDepth - 1, minTriangles);
                right = new BVHNode(triangles, startIndex + mid, triangleCount - mid, maxDepth - 1, minTriangles);


            }

        }

        Triangle[] triangles;
        BVHNode root;

        public BVH(Triangle[] triangles, int maxDepth, int minTriangles = 1)
        {
            this.triangles = triangles;
            root = new BVHNode(triangles, 0, triangles.Length, maxDepth, minTriangles);
        }

        public RayHitInfo Intersect(Ray ray)
        {
            RayHitInfo hitInfo = new RayHitInfo { ray = ray, t = double.MaxValue };
            Stack<BVHNode> stack = new Stack<BVHNode>();
            stack.Push(root);
            double t = double.MaxValue;
            while (stack.Count > 0)
            {
                BVHNode node = stack.Pop();
                if (!node.aabb.Intersect(ray,out t))
                    continue;
                if(t > hitInfo.t)
                    continue;
                if (node.left == null)
                {
                    foreach (var triangle in triangles.Skip(node.startIndex).Take(node.triangleCount))
                        if (triangle.Intersect(ray, out RayHitInfo hitInfoTemp))
                            if (hitInfoTemp.t < hitInfo.t)
                                hitInfo = hitInfoTemp;
                }
                else
                {
                    stack.Push(node.left);
                    stack.Push(node.right);
                }
            }
            return hitInfo;
        }
    }
}
