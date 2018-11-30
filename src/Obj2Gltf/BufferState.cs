using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Obj2Gltf
{
    /// <summary>
    /// cached all buffers
    /// </summary>
    public class BufferState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="withBatchTable">whether include batchtables for 3d tiles</param>
        public BufferState(bool withBatchTable = false)
        {
            if (withBatchTable)
            {
                BatchIdAccessors = new List<int>();
                BatchIdBuffers = new List<byte[]>();
                BatchTableJson = new BatchTable();
            }
        }
        /// <summary>
        /// Vertex Coordinates Buffers
        /// </summary>
        public List<byte[]> PositionBuffers { get; } = new List<byte[]>();
        /// <summary>
        /// Vertex Normals Buffers
        /// </summary>
        public List<byte[]> NormalBuffers { get; } = new List<byte[]>();
        /// <summary>
        /// Vertex Texture Coordinates Buffers
        /// </summary>
        public List<byte[]> UvBuffers { get; } = new List<byte[]>();
        /// <summary>
        /// Triangle Indices Buffers
        /// </summary>
        public List<byte[]> IndexBuffers { get; } = new List<byte[]>();
        /// <summary>
        /// Vertex Coordinates Indices
        /// </summary>
        public List<int> PositionAccessors { get; } = new List<int>();
        /// <summary>
        /// Vertex Normals Indices
        /// </summary>
        public List<int> NormalAccessors { get; } = new List<int>();
        /// <summary>
        /// Vertex Texture Coordinates Indices
        /// </summary>
        public List<int> UvAccessors { get; } = new List<int>();
        /// <summary>
        /// Triangle Indices
        /// </summary>
        public List<int> IndexAccessors { get; } = new List<int>();
        /// <summary>
        /// if with batchTable, the current batch id
        /// </summary>
        public int CurrentBatchId { get; set; }
        /// <summary>
        /// if with batchTable, batch ids buffers
        /// </summary>
        public List<byte[]> BatchIdBuffers { get; set; }
        /// <summary>
        /// if with batchTable, batch ids indices
        /// </summary>
        public List<int> BatchIdAccessors { get; set; }
        /// <summary>
        /// batched table
        /// </summary>
        public BatchTable BatchTableJson { get; set; }
    }
    /// <summary>
    /// batched table
    /// </summary>
    public class BatchTable
    {
        /// <summary>
        /// Batch Ids in ushort
        /// </summary>
        [JsonProperty("batchId")]
        public List<ushort> BatchIds { get; set; } = new List<ushort>();
        /// <summary>
        /// Batch Names
        /// </summary>
        [JsonProperty("name")]
        public List<string> Names { get; set; } = new List<string>();
        /// <summary>
        /// The maximum boundary for each batch
        /// </summary>
        [JsonProperty("maxPoint")]
        public List<double[]> MaxPoint { get; set; } = new List<double[]>();
        /// <summary>
        /// The minimum boundary for each batch
        /// </summary>
        [JsonProperty("minPoint")]
        public List<double[]> MinPoint { get; set; } = new List<double[]>();
    }
}
