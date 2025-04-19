using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rt004;

public static class stringExtensions
{
    public static Vector3d ParseToVector3d(this string str)
    {
        string[] strings = str.Split(' ');
        if (strings.Length != 3)
            throw new ArgumentException("Invalid vector format. Needs to be three doubles seperated by spaces");
        return new Vector3d(
                double.Parse(strings[0], CultureInfo.InvariantCulture),
                double.Parse(strings[1], CultureInfo.InvariantCulture),
                double.Parse(strings[2], CultureInfo.InvariantCulture)
            );
    }

    public static Vector3 ParseToVector3(this string str)
    {
        string[] strings = str.Split(' ');
        if (strings.Length != 3)
            throw new ArgumentException("Invalid vector format. Needs to be three doubles seperated by spaces");
        return new Vector3(
            float.Parse(strings[0], CultureInfo.InvariantCulture),
            float.Parse(strings[1], CultureInfo.InvariantCulture),
            float.Parse(strings[2], CultureInfo.InvariantCulture)
        );
    }
}
