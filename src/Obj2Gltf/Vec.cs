using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Arctron.Obj2Gltf
{
    /// <summary>
    /// 2-d point or vector
    /// </summary>
    public struct Vec2
    {
        public Vec2(double u, double v)
        {
            U = u;
            V = v;
        }

        public double U;

        public double V;

        public override string ToString()
        {
            return $"{U}, {V}";
        }

        public byte[] ToFloatBytes()
        {            
            return BitConverter.GetBytes((float)U).Concat(BitConverter.GetBytes((float)V)).ToArray();
        }
    }
    /// <summary>
    /// 3-d point or verctor
    /// </summary>
    public struct Vec3
    {
        public Vec3(double xyz) : this(xyz, xyz, xyz) { }

        public Vec3(double x, double y) : this(x, y, 0.0) { }

        public Vec3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X;

        public double Y;

        public double Z;

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public double GetLength()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public Vec3 Normalize()
        {
            var len = GetLength();
            return new Vec3(X / len, Y / len, Z / len);
        }

        public Vec3 MultiplyBy(double val)
        {
            return new Vec3(X * val, Y * val, Z * val);
        }

        public Vec3 DividedBy(double val)
        {
            return new Vec3(X / val, Y / val, Z / val);
        }

        public byte[] ToFloatBytes()
        {
            return BitConverter.GetBytes((float)X).Concat(BitConverter.GetBytes((float)Y)).Concat(BitConverter.GetBytes((float)Z)).ToArray();
        }

        public static Vec3 Multiply(Vec3 left, Vec3 right)
        {
            return new Vec3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        public static Vec3 Add(Vec3 v1, Vec3 v2)
        {
            return new Vec3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static Vec3 Cross(Vec3 left, Vec3 right)
        {
            var leftX = left.X;
            var leftY = left.Y;
            var leftZ = left.Z;
            var rightX = right.X;
            var rightY = right.Y;
            var rightZ = right.Z;

            var x = leftY * rightZ - leftZ * rightY;
            var y = leftZ * rightX - leftX * rightZ;
            var z = leftX * rightY - leftY * rightX;

            return new Vec3(x, y, z);
        }

        public static double Dot(Vec3 v1, Vec3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }
    }
    /// <summary>
    /// a bounding box with min and max points
    /// </summary>
    public class MinMax
    {
        public MinMax()
        {
            Min = double.MaxValue;
            Max = double.MinValue;
        }
        public double Min { get; set; }

        public double Max { get; set; }

        public bool IsValid()
        {
            return Min <= Max;
        }
    }
}
