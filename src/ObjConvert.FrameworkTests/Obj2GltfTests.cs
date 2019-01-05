
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
        internal static string TestObjFile = @"..\..\..\testassets\Office\model.obj";

        public static void TestConvert()
        {
            var objFile = TestObjFile;
            var opts = new GltfOptions();
            var converter = new Converter(objFile, opts);
            var outputFile = "model.gltf";
            if (opts.Binary)
            {
                outputFile = "model.glb";
            }
            converter.Run();
            converter.WriteFile(outputFile);
        }
    }
}
