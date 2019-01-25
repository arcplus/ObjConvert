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
            //Test3dTile();
            //Test3dTiles();
            TestSplitAndMerge();
        }
        static void TestSplitAndMerge()
        {
            var gisPosition = new GisPosition();
            var mobjZipFile2 = @"testsplitmerge.objr";
            Obj23dTilesTests.SplitObjAndMergeMTilesetsWithZip(mobjZipFile2, "testsplitmerge", gisPosition, 2);
        }

        static void Test3dTile()
        {
            var gisPosition = new GisPosition();
            var mobjZipFile2 = @"testm3.mobjr";
            Obj23dTilesTests.MergeMTilesetsWithZip(mobjZipFile2, "testm4", gisPosition, false);
        }

        static void Test3dTiles()
        {
            var gisPosition = new GisPosition();
            var objZipFile = @"test.objr";
            Obj23dTilesTests.WriteTilesetWithZip(objZipFile, "test", gisPosition);

            var mobjZipFile = @"testm.mobjr";
            var tasks = new Task<string>[2];
            tasks[0] = Task.Run(() =>
            Obj23dTilesTests.WriteMTilesetsWithZip(mobjZipFile, "testm", gisPosition));
            var mobjZipFile2 = @"testm2.mobjr";
            tasks[1] = Task.Run(() =>
            Obj23dTilesTests.MergeMTilesetsWithZip(mobjZipFile2, "testm2", gisPosition, true));
            Task.WaitAll(tasks);
            
        }
    }
}
