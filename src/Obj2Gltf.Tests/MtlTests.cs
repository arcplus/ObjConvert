using System;
using Xunit;
using Arctron.Obj2Gltf.WaveFront;

namespace Arctron.Obj2Gltf.Tests
{
    public class MtlTests
    {
        [Fact]
        public void LoadMtl_Test()
        {
            var mtlFile = @"..\..\..\..\testassets\Office\model.mtl";
            Assert.True(System.IO.File.Exists(mtlFile), "mtl file does not exist!");
            using (var mtlParser = new MtlParser(mtlFile))
            {
                var mats = mtlParser.GetMats();
                Assert.True(mats.Count > 0);
            }            
        }
    }
}
