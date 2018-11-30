using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.IO;

namespace Arctron.Obj2Gltf.Tests
{
    public class ConverterTests
    {
        private static string TestObjFile = @"..\..\..\..\testassets\Office\model.obj";

        static void CheckObjFiles()
        {
            Assert.True(File.Exists(TestObjFile), "Obj File does not exist!");
        }
        [Fact]
        public void TestConvertGltf()
        {
            var name = "model";

            CheckObjFiles();            

            var converter = new Converter(TestObjFile, false);
            var outputFile = name+".gltf";
            converter.Run();
            converter.WriteFile(outputFile);
            Assert.True(File.Exists(outputFile));
        }

        [Fact]
        public void TestConvertGlb()
        {
            var name = "model";

            CheckObjFiles();
            var objFile = TestObjFile;
            var converter = new Converter(objFile, true);
            var outputFile = $"{name}.glb";
            converter.Run();
            converter.WriteFile(outputFile);
            Assert.True(System.IO.File.Exists(outputFile));
        }
    }
}
