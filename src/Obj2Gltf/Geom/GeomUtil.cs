using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Arctron.Obj2Gltf.Geom
{
    internal class PlanarAxis
    {
        public Vec3 Center { get; set; }

        public Vec3 Axis1 { get; set; }

        public Vec3 Axis2 { get; set; }
    }
    internal static class GeomUtil
    {
        public static PlanarAxis ComputeProjectTo2DArguments(IList<Vec3> positions)
        {
            var box = OrientedBoundingBox.FromPoints(positions);

            var halfAxis = box.HalfAxis;
            var xAxis = halfAxis.GetColumn(0);
            var scratchXAxis = xAxis;
            var yAxis = halfAxis.GetColumn(1);
            var scratchYAxis = yAxis;
            var zAxis = halfAxis.GetColumn(2);
            var scratchZAxis = zAxis;

            var xMag = xAxis.GetLength();
            var yMag = yAxis.GetLength();
            var zMag = zAxis.GetLength();
            var min = new[] { xMag, yMag, zMag }.Min();

            // If all the points are on a line return undefined because we can't draw a polygon
            if ((xMag == 0 && (yMag == 0 || zMag == 0)) || (yMag == 0 && zMag == 0))
            {
                return null;
            }

            var planeAxis1 = new Vec3();
            var planeAxis2 = new Vec3();

            if (min == yMag || min == zMag)
            {
                planeAxis1 = xAxis;
            }
            if (min == xMag)
            {
                planeAxis1 = yAxis;
            }
            else if (min == zMag)
            {
                planeAxis2 = yAxis;
            }
            if (min == xMag || min == yMag)
            {
                planeAxis2 = zAxis;
            }

            return new PlanarAxis
            {
                Center = box.Center,
                Axis1 = planeAxis1,
                Axis2 = planeAxis2
            };
        }


        private static Vec2 Project2D(Vec3 p, Vec3 center, Vec3 axis1, Vec3 axis2)
        {
            var v = p.Substract(center);
            var x = Vec3.Dot(axis1, v);
            var y = Vec3.Dot(axis2, v);

            return new Vec2(x, y);
        }

        public static IList<Vec2> CreateProjectPointsTo2DFunction(PlanarAxis axis, IList<Vec3> positions)
        {
            var pnts = new Vec2[positions.Count];
            for(var i = 0;i< pnts.Length;i++)
            {
                pnts[i] = Project2D(positions[i], axis.Center, axis.Axis1, axis.Axis2);
            }
            return pnts;
        }
    }
}
