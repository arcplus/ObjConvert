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

            var converter = new Converter(TestObjFile, new GltfOptions());
            var outputFile = name+".gltf";
            converter.Run();
            converter.WriteFile(outputFile);
            Assert.True(File.Exists(outputFile));
        }

        [Fact]
        public void TestConvertGltf2()
        {
            var name = "model";
            CheckObjFiles();

            var objParser = new WaveFront.ObjParser(TestObjFile);
            var objModel = objParser.GetModel();

            var converter = new Converter(objModel, Path.GetDirectoryName(TestObjFile), new GltfOptions { Name = "model" });
            var outputFile = name + ".gltf";
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
            var converter = new Converter(objFile, new GltfOptions { Binary = true });
            var outputFile = $"{name}.glb";
            converter.Run();
            converter.WriteFile(outputFile);
            Assert.True(System.IO.File.Exists(outputFile));
        }
    }
}
