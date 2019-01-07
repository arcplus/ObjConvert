using System;
using System.Collections.Generic;
using System.Text;

namespace Arctron.Obj2Gltf.Geom
{
    public class BoundingBox2
    {
        public Vec2 Min { get; set; }

        public Vec2 Max { get; set; }

        public bool IsValid()
        {
            return Min.U < Max.U && Min.V < Max.V;
        }

        public bool IsIn(Vec2 p)
        {
            return p.U > Min.U && p.U < Max.U && p.V > Min.V && p.V < Max.V;
        }

        public static BoundingBox2 New()
        {
            return new BoundingBox2
            {
                Min = new Vec2(double.MaxValue, double.MaxValue),
                Max = new Vec2(double.MinValue, double.MinValue)
            };

        }
    }

    public enum PolygonPointRes
    {
        Outside,
        Inside,
        Vetex,
        Edge
    }

    public class PolygonUtil
    {
        // 
        private static bool IsIntersect(Vec2 ln1Start, Vec2 ln1End, Vec2 ln2Start, Vec2 ln2End)
        {
            //https://ideone.com/PnPJgb
            var A = ln1Start;
            var B = ln1End;
            var C = ln2Start;
            var D = ln2End;
            Vec2 CmP = new Vec2(C.U - A.U, C.V - A.V);
            Vec2 r = new Vec2(B.U - A.U, B.V - A.V);
            Vec2 s = new Vec2(D.U - C.U, D.V - C.V);

            var CmPxr = CmP.U * r.V - CmP.V * r.U;
            var CmPxs = CmP.U * s.V - CmP.V * s.U;
            var rxs = r.U * s.V - r.V * s.U;

            if (CmPxr == 0f)
            {
                // Lines are collinear, and so intersect if they have any overlap

                return ((C.U - A.U < 0f) != (C.U - B.U < 0f))
                    || ((C.V - A.V < 0f) != (C.V - B.V < 0f));
            }

            if (rxs == 0f)
                return false; // Lines are parallel.

            var rxsr = 1f / rxs;
            var t = CmPxs * rxsr;
            var u = CmPxr * rxsr;

            return (t >= 0) && (t <= 1) && (u >= 0) && (u <= 1);



            //// https://en.wikipedia.org/wiki/Line%E2%80%93line_intersection
            //double x1 = ln1Start.U, y1 = ln1Start.V;
            //double x2 = ln1End.U, y2 = ln1End.V;
            //double x3 = ln2Start.U, y3 = ln2Start.V;
            //double x4 = ln2End.U, y4 = ln2End.V;

            //var t1 = (x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4);
            //var t2 = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            //var t = t1 / t2;

            //if (t < 0.0 || t > 1.0) return false;

            ////if (t >= 0.0 && t <= 1.0) return true;

            ////return false;

            //var u1 = (x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3);
            //var u2 = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            //var u = u1 / u2;

            //return u >= 0.0 && u <= 1.0;
        }

        public static PolygonPointRes CrossTest(Vec2 p, IList<Vec2> polygon, double tol)
        {
            
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            var angleVs = new List<double>();
            var vecX = new Vec2(1.0, 0);
            foreach(var v in polygon)
            {
                var d = p.GetDistance(v);                
                if (d < tol) return PolygonPointRes.Vetex;
                if (minX > v.U) minX = v.U;
                if (minY > v.V) minY = v.V;
                if (maxX < v.U) maxX = v.U;
                if (maxY < v.V) maxY = v.V;
                var vector = new Vec2(v.U - p.U, v.V - p.V).Normalize();
                var an = Math.Acos(vecX.Dot(vector));
                if (vector.V < 0)
                {
                    an = 2* Math.PI - an;
                }
                angleVs.Add(an);
            }
            for(var i = 0;i<polygon.Count;i++)
            {
                var j = i + 1;
                if (j == polygon.Count)
                {
                    j = 0;
                }
                var v1 = polygon[i];
                var v2 = polygon[j];
                var d0 = v1.GetDistance(v2);
                if (Math.Abs(p.GetDistance(v1) + p.GetDistance(v2) - d0) < tol)
                {
                    return PolygonPointRes.Edge;
                }
            }
            var box = new BoundingBox2 { Min = new Vec2(minX, minY), Max = new Vec2(maxX, maxY) };
            if (!box.IsIn(p)) return PolygonPointRes.Outside;

            angleVs.Sort();

            var startIndex = 0;
            var diff = angleVs[1]-angleVs[0];
            for(var i = 1;i<angleVs.Count;i++)
            {
                var j = i + 1;
                if (j == angleVs.Count) j = 0;
                var anJ = angleVs[j];
                if (j == 0)
                {
                    anJ += Math.PI * 2;
                }
                var diff1 = angleVs[j] - angleVs[i];
                if (diff1 > diff)
                {
                    diff = diff1;
                    startIndex = i;
                }
            }
            var angle = angleVs[startIndex] + diff / 2.0;
            var len = box.Max.GetDistance(box.Min);
            var p2 = new Vec2(len * Math.Cos(angle), len * Math.Sin(angle));

            var intersectCount = 0;
            for(var i = 0;i<polygon.Count;i++)
            {
                var j = i + 1;
                if (j == polygon.Count) j = 0;
                var v1 = polygon[i];
                var v2 = polygon[j];
                var pnt = IsIntersect(p, p2, v1, v2);
                IsIntersect(p, p2, v1, v2);
                if (pnt)
                {
                    
                    intersectCount++;
                }
            }

            if (intersectCount % 2 == 1)
            {
                return PolygonPointRes.Inside;
            }
            return PolygonPointRes.Outside;
        }

        private static double GetRayLength(Vec2 p, IList<Vec2> polygon)
        {
            throw new NotImplementedException();
        }
    }
}
