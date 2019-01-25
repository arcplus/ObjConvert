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

        public double[] ToArray()
        {
            return new[] { U, V };
        }

        public double GetDistance(Vec2 p)
        {
            return Math.Sqrt((U - p.U) * (U - p.U) + (V - p.V) * (V - p.V));
        }

        public double GetLength()
        {
            return Math.Sqrt(U * U + V * V);
        }

        public Vec2 Normalize()
        {
            var len = GetLength();
            return new Vec2(U / len, V / len);
        }

        public double Dot(Vec2 v)
        {
            return U * v.U + V * v.V;
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

        public Vec3 Substract(Vec3 p)
        {
            return new Vec3(X - p.X, Y - p.Y, Z - p.Z);
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

        public double[] ToArray()
        {
            return new[] { X, Y, Z };
        }

    }

    public class BoundingBox
    {
        public MinMax X { get; set; } = new MinMax();

        public MinMax Y { get; set; } = new MinMax();

        public MinMax Z { get; set; } = new MinMax();

        public bool IsIn(Vec3 p)
        {
            return p.X >= X.Min && p.X <= X.Max &&
                p.Y >= Y.Min && p.Y <= Y.Max &&
                p.Z >= Z.Min && p.Z <= Z.Max;
        }

        public override string ToString()
        {
            return $"X: {X}; Y: {Y}; Z: {Z}";
        }

        public List<BoundingBox> Split(int level)
        {
            if (level <= 1) return new List<BoundingBox> { this };
            var boxes = new List<BoundingBox>();
            var diffX = (X.Max - X.Min) / level;
            var diffY = (Y.Max - Y.Min) / level;
            var diffZ = (Z.Max - Z.Min) / level;
            for(var x = 0;x<level;x++)
            {
                var xj = x + 1;
                var maxX = X.Max;
                if (xj < level)
                {
                    maxX = X.Min + xj * diffX;
                }
                for(var y =0;y<level;y++)
                {
                    var yj = y + 1;
                    var maxY = Y.Max;
                    if (yj < level)
                    {
                        maxY = Y.Min + yj * diffY;
                    }

                    for(var z = 0;z<level;z++)
                    {
                        var zj = z + 1;
                        var maxZ = Z.Max;
                        if (zj < level)
                        {
                            maxZ = Z.Min + zj * diffZ;
                        }

                        boxes.Add(new BoundingBox
                        {
                            X = new MinMax { Min = X.Min + x * diffX, Max = maxX },
                            Y = new MinMax { Min = Y.Min + y * diffY, Max = maxY },
                            Z = new MinMax { Min = Z.Min + z * diffZ, Max = maxZ }
                        });
                    }
                }
            }
            return boxes;
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

        public override string ToString()
        {
            return $"(Min: {Min}, Max: {Max})";
        }
    }
}
