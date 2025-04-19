using OpenTK.Mathematics;

namespace rt004.Rendering;

public class Transform
{
    public Matrix4d transformationMatrix { get; private set; }

    private Matrix4d? _inverseTransMatrix = null;
    private Transform? _inverse = null;

    private Vector3d _position;
    private Vector3d _rotation;
    private Vector3d _scale;



    private void UpdateMatrix()
    {
        transformationMatrix = Matrix4d.Scale(scale) * Matrix4d.CreateRotationX(rotation.X) * Matrix4d.CreateRotationY(rotation.Y) * Matrix4d.CreateRotationZ(rotation.Z) * Matrix4d.CreateTranslation(position);
        //invalidate the inverse matrix
        _inverseTransMatrix = null;
        _inverse = null;
    }

    //calculate the inverse matrix only if needed
    public Matrix4d InverseMatrix
    {
        get
        {
            if (_inverseTransMatrix == null)
                _inverseTransMatrix = transformationMatrix.Inverted();
            return _inverseTransMatrix.Value;
        }
    }

    // C# is actually cool
    public Transform Inverse => _inverse ??= new Transform(InverseMatrix) { _inverseTransMatrix = transformationMatrix };

    public Transform()
    {
        _position = Vector3d.Zero;
        _rotation = Vector3d.Zero;
        _scale = Vector3d.One;
        UpdateMatrix();
    }

    public Transform(Matrix4d transform)
    {
        transformationMatrix = transform;
        _scale = transform.ExtractScale();
        _rotation = transform.ExtractRotation().ToEulerAngles();
        _position = transform.ExtractTranslation();
    }

    public Transform(Vector3d position, Vector3d rotation, Vector3d scale)
    {
        _position = position;
        _rotation = rotation;
        _scale = scale;
        UpdateMatrix();
    }

    public Vector3d TransformVector(Vector3d v) => (new Vector4d(v, 0) * transformationMatrix).Xyz;
    public Vector3d TransformPoint(Vector3d v) => (new Vector4d(v, 1) * transformationMatrix).Xyz;

    public Vector3d position
    {
        get => _position;
        set
        {
            _position = value;
            UpdateMatrix();
        }
    }

    public Vector3d rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            UpdateMatrix();
        }
    }

    public Vector3d scale
    {
        get => _scale;
        set
        {
            _scale = value;
            UpdateMatrix();
        }
    }
}
