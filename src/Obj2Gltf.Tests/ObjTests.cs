using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Arctron.Obj2Gltf.WaveFront;

namespace Arctron.Obj2Gltf.Tests
{
    public class ObjTests
    {
        static string objFile = @"..\..\..\..\testassets\Office\model.obj";
        [Fact]
        public void Test_LoadObj()
        {
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
