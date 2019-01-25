using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Arctron.Obj2Gltf.WaveFront;

namespace Arctron.Obj2Gltf.Tests
{
    public class ObjTests
    {
        [Fact]
        public void Test_LoadObj()
        {
            var objFile = @"..\..\..\..\testassets\Office\model.obj";
            Assert.True(System.IO.File.Exists(objFile), "obj file does not exist!");
            using (var parser = new ObjParser(objFile))
            {
                var model = parser.GetModel();
                Assert.True(model.Vertices.Count > 0);
            }
        }

        [Fact]
        public void Test_Split()
        {
            var objFile = @"..\..\..\..\testassets\Office\model.obj";
            Assert.True(System.IO.File.Exists(objFile), "obj file does not exist!");
            using (var parser = new ObjParser(objFile))
            {
                var model = parser.GetModel();
                var models = model.Split(2);
                Assert.True(models.Count > 0);
            }
        }
    }
}
