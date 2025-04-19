using OpenTK.Mathematics;
using rt004.Utility;
using System.ComponentModel;

namespace rt004.Rendering;

public class Scene
{
    public List<ISolid> solids = new List<ISolid>();

    public List<Light> lights = new List<Light>();

    public Vector3 backgroundColor = Vector3.Zero;

    // ------ for parallel rendering

    Camera _cam;
    struct RenderTaskBlock
    {
        public int x, y, size;
        public RenderTaskBlock(int x, int y, int size)
        {
            this.x = x;
            this.y = y;
            this.size = size;
        }
    }

    Canvas _canvas;

    Queue<RenderTaskBlock> _tasks = new Queue<RenderTaskBlock>();
    object _tasksLock = new object();
    
    // ----------

    public RayHitInfo RayCast(Ray ray)
    {
        RayHitInfo? closestRayHitInfo = null;
        foreach (var solid in solids)
        {
            var hitinfo = solid.Intersect(ray);
            if (hitinfo.HasHit && (closestRayHitInfo == null || hitinfo.t < closestRayHitInfo.t))
                closestRayHitInfo = hitinfo;
        }
        return closestRayHitInfo ?? new RayHitInfo() {ray = ray};
    }

    public Canvas RenderSceneParallel(Camera cam, int workers)
    {
        _cam = cam;
        _canvas = new(cam.Width, cam.Height,3);

        //measure time
        var watch = System.Diagnostics.Stopwatch.StartNew();

        //create tasks
        int blockSize = 32;
        _tasks = new Queue<RenderTaskBlock>();
        for (int x = 0; x < cam.Width; x += blockSize)
            for (int y = 0; y < cam.Height; y += blockSize)
                _tasks.Enqueue(new RenderTaskBlock(x, y, blockSize));
        Task[] workerArr = new Task[workers];
        
        for (int i = 0; i < workers; i++)
        {
            int workerId = i;
            workerArr[i] = Task.Run(() => RunWorker(workerId));
        }
        Task.WaitAll(workerArr);

        if (cam.renderMode == Camera.CameraRenderMode.Depth)
            ProcessDepth();

        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"Rendering took {elapsedMs}ms using {workers} workers for maxDepth {cam.maxDepth}");

        return _canvas;
    }

    private void ProcessDepth()
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int x = 0; x < _cam.Width; x++)
            for (int y = 0; y < _cam.Height; y++)
            {
                float depth = _canvas.canvas[x, y, 0];
                if(depth < 0) //background
                    continue;
                min = Math.Min(min, depth);
                max = Math.Max(max, depth);
            }
        Console.WriteLine($"Depth min: {min} max: {max}");
        
        for (int x = 0; x < _cam.Width; x++)
            for (int y = 0; y < _cam.Height; y++)
            {
                float depth = _canvas.canvas[x, y, 0];
                if (depth < 0) //background
                    _canvas.SetPixel(x, y, Vector3.Zero);
                _canvas.SetPixel(x, y, Vector3.One * (1 - (depth - min) / (max - min))); //lerp
            }
    }

    private void RunWorker(int workerId)
    {
        RenderTaskBlock task;
        while (true)
        {
            lock(_tasksLock)
            {
                if (_tasks.Count == 0)
                {
                    //Console.WriteLine($"W{workerId}: Queue empty. ending work");
                    return; //returning will unlock the lock
                }
                task = _tasks.Dequeue();
            }
            //Console.WriteLine($"W{workerId}: Working on task {task.x},{task.y}");
            for (int x = task.x; x < task.x + task.size; x++)
                for (int y = task.y; y < task.y + task.size; y++)
                {
                    if(x>= _cam.Width || y >= _cam.Height)
                        continue;
                    List<Vector3> microPixels = new();
                    foreach(var r in _cam.EnumerateThroughMacroPixel(x,y))
                        switch(_cam.renderMode)
                        {
                            case Camera.CameraRenderMode.Color:
                                microPixels.Add(Shade(r, _cam.maxDepth));
                                break;
                            case Camera.CameraRenderMode.Normal:
                                microPixels.Add(ShaderNormals(r));
                                break;
                            case Camera.CameraRenderMode.Depth:
                                microPixels.Add(ShaderDepth(r));
                                break;
                        }
                    float[] color = new float[3];
                    foreach(var c in microPixels)
                    {
                        color[0] += c.X;
                        color[1] += c.Y;
                        color[2] += c.Z;
                    }
                    for (int i = 0; i < 3; i++)
                        color[i] /= microPixels.Count;
                    _canvas.SetPixel(x, y, color);
                }
        }
    }

    public Vector3 ShaderNormals(Ray ray)
    {
        var hitInfo = RayCast(ray);
        if (!hitInfo.HasHit)
            return Vector3.Zero;
        return ((Vector3)hitInfo.normal) / 2 + Vector3.One / 2;
    }

    public Vector3 ShaderDepth(Ray ray)
    {
        var hitInfo = RayCast(ray);
        if (!hitInfo.HasHit)
            return - Vector3.One;
        return Vector3.One * (float)hitInfo.t;
    }

    public Vector3 Shade(Ray ray,int depth,double potential = 1, Stack<Material> materialsOnPath = null)
    {
        // Actually if anyone is reading this... Can refractive ray be reflected when hitting the solid from inside? I think it shouldnt, so thats what i am going to do
        if (materialsOnPath == null)
        {
            materialsOnPath = new Stack<Material>();
            materialsOnPath.Push(null); //push air in
        }
        var hitInfo = RayCast(ray);
        if(!hitInfo.HasHit)
            return backgroundColor;
        Vector3 color = hitInfo.solid!.material.brdf.CalculateColor(hitInfo, this);

        if (depth <= 0)
            return color;
        if (potential <= 1e-4)
            return color;

        // Isnt this overexposure?
        //color *= (1 - hitInfo.solid!.material.k_s);


        var reflectaneMultiplier = hitInfo.solid!.material.k_s * hitInfo.solid!.material.alpha;
        var refractanceMultiplier = hitInfo.solid!.material.k_s * (1 - hitInfo.solid!.material.alpha);

        bool isHittingInsideSurface = Vector3d.Dot(hitInfo.normal, ray.p1) > 0;
        if (isHittingInsideSurface)
        {
            materialsOnPath.Pop(); // we left this material
            if(materialsOnPath.Count == 0)
                materialsOnPath.Push(null); //push air in (this is hack: there is probably a hole in the mesh)
            Material newMaterial = materialsOnPath.Peek(); //into this material
            var refractranceVector = hitInfo.Refractance(newMaterial);
            if (!double.IsNaN(refractranceVector.X))
                color += refractanceMultiplier * Shade(new Ray(hitInfo.Point, refractranceVector), depth - 1, potential * refractanceMultiplier, materialsOnPath);
            return color;
        }

        //else we are hitting outside surface. do both reflectance and refractance

        color += reflectaneMultiplier * Shade(new Ray(hitInfo.Point, hitInfo.Reflectence), depth - 1, potential * reflectaneMultiplier, materialsOnPath);
        if (refractanceMultiplier > float.Epsilon)
        {
            var refractranceVector = hitInfo.Refractance(materialsOnPath.Peek());
            if (!double.IsNaN(refractranceVector.X))
            {
                materialsOnPath.Push(hitInfo.solid!.material);
                color += refractanceMultiplier * Shade(new Ray(hitInfo.Point, refractranceVector), depth - 1, potential * refractanceMultiplier, materialsOnPath);
            }
        }
        return color;
    }
}

