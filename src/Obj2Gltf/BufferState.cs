using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Obj2Gltf
{
    public class BufferState
    {
        public BufferState(bool withBatchTable = false)
        {
            if (withBatchTable)
            {
                BatchIdAccessors = new List<int>();
                BatchIdBuffers = new List<byte[]>();
                BatchTableJson = new BatchTable();
            }
        }
        public List<byte[]> PositionBuffers { get; } = new List<byte[]>();

        public List<byte[]> NormalBuffers { get; } = new List<byte[]>();

        public List<byte[]> UvBuffers { get; } = new List<byte[]>();

        public List<byte[]> IndexBuffers { get; } = new List<byte[]>();

        public List<int> PositionAccessors { get; } = new List<int>();

        public List<int> NormalAccessors { get; } = new List<int>();

        public List<int> UvAccessors { get; } = new List<int>();

        public List<int> IndexAccessors { get; } = new List<int>();


        public int CurrentBatchId { get; set; }

        public List<byte[]> BatchIdBuffers { get; set; }

        public List<int> BatchIdAccessors { get; set; }

        public BatchTable BatchTableJson { get; set; }
    }

    public class BatchTable
    {
        [JsonProperty("batchId")]
        public List<ushort> BatchIds { get; set; } = new List<ushort>();
        [JsonProperty("name")]
        public List<string> Names { get; set; } = new List<string>();
        [JsonProperty("maxPoint")]
        public List<double[]> MaxPoint { get; set; } = new List<double[]>();
        [JsonProperty("minPoint")]
        public List<double[]> MinPoint { get; set; } = new List<double[]>();
    }
}
