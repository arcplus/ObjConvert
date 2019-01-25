using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Arctron.Obj2Gltf.Geom;

namespace Arctron.Obj2Gltf.Tests
{
    public class PolygonTests
    {
        [Fact]
        public void Test_Intersect()
        {
            var pnts = new List<Vec2>
            {
                new Vec2(0.0, 0.0), new Vec2(1.0, 0.0), new Vec2(1.0,1.0), new Vec2(0.5,0.5), new Vec2(0.0,1.0)
            };
            var tol = 1e-10;
            Assert.Equal(PolygonPointRes.Vetex, PolygonUtil.CrossTest(new Vec2(0.0, 0.0), pnts, tol));
            Assert.Equal(PolygonPointRes.Edge, PolygonUtil.CrossTest(new Vec2(0.1, 0.0), pnts, tol));
            Assert.Equal(PolygonPointRes.Outside, PolygonUtil.CrossTest(new Vec2(0.5, 0.6), pnts, tol));
            Assert.Equal(PolygonPointRes.Outside, PolygonUtil.CrossTest(new Vec2(0.5, 0.500001), pnts, tol));
            Assert.Equal(PolygonPointRes.Inside, PolygonUtil.CrossTest(new Vec2(0.5, 0.499999), pnts, tol));
            Assert.Equal(PolygonPointRes.Outside, PolygonUtil.CrossTest(new Vec2(1.5, 0.5), pnts, tol));
        }

        [Fact]
        public void Test_BoundingBoxSplit()
        {
            var box = new BoundingBox
            {
                X = new MinMax { Min = 0, Max = 10 },
                Y = new MinMax { Min = 0, Max = 10 },
                Z = new MinMax { Min = 0, Max = 10 }
            };

            var boxes = box.Split(2);
            Assert.Equal(8, boxes.Count);
        }
    }
}
