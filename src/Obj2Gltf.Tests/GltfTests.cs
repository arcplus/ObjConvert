using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Arctron.Obj2Gltf.Tests
{
    public class GltfTests
    {

        [Fact]
        public void Test_Load_Gltf()
        {
            var file = @"..\..\..\..\testassets\Office\model.gltf";
            Assert.True(System.IO.File.Exists(file), "gltf file does not exist!");
            var model = Gltf.GltfModel.LoadFromJsonFile(file);
            Assert.True(model != null);
        }
    }
}
