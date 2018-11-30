using System;
using System.Collections.Generic;
using System.Text;
using Arctron.Obj2Gltf;

namespace Arctron.Obj23dTiles
{
    public class Options
    {
        public List<byte> FeatureTableJson { get; set; } = new List<byte>();

        public List<byte> FeatureTableBinary { get; set; } = new List<byte>();

        public List<byte> BatchTableJson { get; set; } = new List<byte>();

        public List<byte> BatchTableBinary { get; set; } = new List<byte>();
    }
}
