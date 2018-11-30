using Arctron.Obj23dTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctron.ObjConvert.FrameworkTests
{
    class Program
    {
        
        static void Main(string[] args)
        {
            //Obj2GltfTests.TestConvert();
            Test3dTiles();
        }

        static void Test3dTiles()
        {
            var gisPosition = new GisPosition(121.449, 31.1989, 40.0);
            //var objZipFile = @"test.objr";
            //Obj23dTilesTests.WriteTilesetWithZip(objZipFile, "test", gisPosition);

            var mobjZipFile = @"testm.mobjr";
            var tasks = new Task<string>[2];
            tasks[0] = Task.Run(() =>
            Obj23dTilesTests.WriteMTilesetsWithZip(mobjZipFile, "testm", gisPosition));
            var mobjZipFile2 = @"testm2.mobjr";
            tasks[1] = Task.Run(() =>
            Obj23dTilesTests.MergeMTilesetsWithZip(mobjZipFile2, "testm2", gisPosition));
            Task.WaitAll(tasks);
            
        }
    }
}
