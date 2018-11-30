using System;
using System.Collections.Generic;
using System.Text;

namespace Arctron.Obj23dTiles
{
    /// <summary>
    /// b3dm file format model
    /// </summary>
    public class B3dm
    {
        internal const int Version = 1;

        internal const int HeaderByteLength = 28;

        private readonly List<byte> _glb;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="glb">binary gltf model</param>
        public B3dm(List<byte> glb)
        {
            _glb = glb;
        }
        /// <summary>
        /// convert to b3dm binary data
        /// </summary>
        /// <param name="options">converting options</param>
        /// <returns></returns>
        public byte[] Convert(Options options)
        {
            if (options == null) options = new Options();

            var featureTableJson = options.FeatureTableJson;
            var featureTableBinary = options.FeatureTableBinary;
            var batchTableJson = options.BatchTableJson;
            var batchTableBinary = options.BatchTableBinary;

            var featureTableJsonByteLength = featureTableJson.Count;
            var featureTableBinaryByteLength = featureTableBinary.Count;
            var batchTableJsonByteLength = batchTableJson.Count;
            var batchTableBinaryByteLength = batchTableBinary.Count;
            var gltfByteLength = _glb.Count;
            var byteLength = HeaderByteLength + featureTableJsonByteLength 
                + featureTableBinaryByteLength + batchTableJsonByteLength 
                + batchTableBinaryByteLength + gltfByteLength;

            var all = new List<byte>();
            // Header
            all.Add(System.Convert.ToByte('b'));
            all.Add(System.Convert.ToByte('3'));
            all.Add(System.Convert.ToByte('d'));
            all.Add(System.Convert.ToByte('m'));
            all.AddRange(BitConverter.GetBytes((uint)Version));
            all.AddRange(BitConverter.GetBytes((uint)byteLength));
            all.AddRange(BitConverter.GetBytes((uint)featureTableJsonByteLength));
            all.AddRange(BitConverter.GetBytes((uint)featureTableBinaryByteLength));
            all.AddRange(BitConverter.GetBytes((uint)batchTableJsonByteLength));
            all.AddRange(BitConverter.GetBytes((uint)batchTableBinaryByteLength));

            all.AddRange(featureTableJson);
            all.AddRange(featureTableBinary);
            all.AddRange(batchTableJson);
            all.AddRange(batchTableBinary);
            all.AddRange(_glb);

            return all.ToArray();
        }
    }
}
