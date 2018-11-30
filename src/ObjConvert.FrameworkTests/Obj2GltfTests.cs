
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Arctron.Obj2Gltf;

namespace Arctron.ObjConvert.FrameworkTests
{
    public class Obj2GltfTests
    {
        internal static readonly string TestObjFile = @"..\..\..\testassets\Office\model.obj";

        public static void TestConvert()
        {
            var objFile = TestObjFile;
            var converter = new Converter(objFile, false);
            var outputFile = "model.gltf";
            converter.Run();
            converter.WriteFile(outputFile);
        }
    }
}
