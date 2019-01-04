using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Arctron.Obj2Gltf.Geom
{
    // from cesium
    internal class OrientedBoundingBox
    {
        public Matrix3 HalfAxis { get; set; }

        public Vec3 Center { get; set; }


        public static OrientedBoundingBox FromPoints(IList<Vec3> positions)
        {
            var result = new OrientedBoundingBox();

            var length = positions.Count;

            var meanPoint = positions[0];
            for(var i = 1; i< length;i++)
            {
                meanPoint = Vec3.Add(meanPoint, positions[i]);
            }
            var invLength = 1.0 / length;

            meanPoint = meanPoint.MultiplyBy(invLength);

            var exx = 0.0;
            var exy = 0.0;
            var exz = 0.0;
            var eyy = 0.0;
            var eyz = 0.0;
            var ezz = 0.0;

            for(var i = 0;i<length;i++)
            {
                var p = positions[i].Substract(meanPoint);
                exx += p.X * p.X;
                exy += p.X * p.Y;
                exz += p.X * p.Z;
                eyy += p.Y * p.Y;
                eyz += p.Y * p.Z;
                ezz += p.Z * p.Z;
            }

            exx *= invLength;
            exy *= invLength;
            exz *= invLength;
            eyy *= invLength;
            eyz *= invLength;
            ezz *= invLength;

            var covarianceMatrix = new Matrix3(exx, exy, exz, exy, eyy, eyz, exz, eyz, ezz);

            var eigenDecomposition = covarianceMatrix.ComputeEigenDecomposition();
            var diagMatrix = eigenDecomposition.Item1;
            var unitaryMatrix = eigenDecomposition.Item2;
            var rotation = unitaryMatrix.Clone();

            var v1 = rotation.GetColumn(0);
            var v2 = rotation.GetColumn(1);
            var v3 = rotation.GetColumn(2);

            var u1 = double.MinValue; //-Number.MAX_VALUE;
            var u2 = double.MinValue; //-Number.MAX_VALUE;
            var u3 = double.MinValue; //-Number.MAX_VALUE;
            var l1 = double.MaxValue; //Number.MAX_VALUE;
            var l2 = double.MaxValue; //Number.MAX_VALUE;
            var l3 = double.MaxValue; //Number.MAX_VALUE;

            for(var i = 0;i <length;i++)
            {
                var p = positions[i];
                u1 = new[] { Vec3.Dot(v1, p), u1 }.Max();
                u2 = new[] { Vec3.Dot(v2, p), u2 }.Max();
                u3 = new[] { Vec3.Dot(v3, p), u3 }.Max();

                l1 = new[] { Vec3.Dot(v1, p), l1 }.Min();
                l2 = new[] { Vec3.Dot(v2, p), l2 }.Min();
                l3 = new[] { Vec3.Dot(v3, p), l3 }.Min();
            }

            v1 = v1.MultiplyBy(0.5 * (l1 + u1));
            v2 = v2.MultiplyBy(0.5 * (l2 + u2));
            v3 = v3.MultiplyBy(0.5 * (l3 + u3));

            var center = Vec3.Add(v1, v2);
            center = Vec3.Add(center, v3);

            var scale = new Vec3(u1 - l1, u2 - l2, u3 - l3);
            scale = scale.MultiplyBy(0.5);

            rotation = rotation.MultiplyByScale(scale);

            return new OrientedBoundingBox
            {
                Center = center,
                HalfAxis = rotation
            };
        }
    }
}
