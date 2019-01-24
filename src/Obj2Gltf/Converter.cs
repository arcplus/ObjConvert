using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Arctron.Obj2Gltf.WaveFront;
using Arctron.Gltf;
using System.IO;
using Newtonsoft.Json;

namespace Arctron.Obj2Gltf
{

    public class GltfOptions
    {
        /// <summary>
        /// Model Name
        /// </summary>
        public string Name { get; set; } = "Untitled";
        /// <summary>
        /// glb?
        /// </summary>
        public bool Binary { get; set; }
        /// <summary>
        /// whether to generate batchids
        /// </summary>
        public bool WithBatchTable { get; set; }
        /// <summary>
        /// obj and mtl files' text encoding
        /// </summary>
        public Encoding ObjEncoding { get; set; } = Encoding.UTF8;
    }
    /// <summary>
    /// obj2gltf converter
    /// </summary>
    public class Converter
    {
        private readonly ObjParser _objParser;
        private readonly string _objFolder;

        private readonly GltfOptions _options;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFile">obj file path</param>
        /// <param name="binary">whether generate glb</param>
        /// <param name="withBatchTable"> whether generate batch table</param>
        [Obsolete("Please use the constructor with GltfOptions")]
        public Converter(string objFile, bool binary, bool withBatchTable = false)
            :this(objFile, new GltfOptions
            {
                Binary = binary,
                WithBatchTable = withBatchTable,
                ObjEncoding = MtlParser.InitEncoding(objFile)
            })
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFile">obj file path</param>
        /// <param name="options"></param>
        public Converter(string objFile, GltfOptions options)
        {
            _objParser = new ObjParser(objFile, options.ObjEncoding);
            _objFolder = Path.GetDirectoryName(objFile);
            var name = Path.GetFileNameWithoutExtension(objFile);
            _options = options ?? new GltfOptions();
            if (String.IsNullOrEmpty(_options.Name))
            {
                _options.Name = name;
            }
            _buffers = new BufferState(_options.WithBatchTable);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objModel">Parsed ObjModel</param>
        /// <param name="objFolder">where the obj file resides</param>
        /// <param name="options"></param>
        public Converter(ObjModel objModel, string objFolder, GltfOptions options)
        {
            _objModel = objModel;
            _objFolder = objFolder;
            _options = options ?? new GltfOptions();
            _buffers = new BufferState(_options.WithBatchTable);            
        }

        private ObjModel _objModel;

        private GltfModel _model;
        private List<byte> _glb;
        private readonly BufferState _buffers;
        /// <summary>
        /// write converted data to file
        /// </summary>
        /// <param name="outputFile"></param>
        public void WriteFile(string outputFile)
        {
            if (_model == null)
            {
                Run();
            }
            if (!_options.Binary)
            {
                var json = ToJson(_model);
                File.WriteAllText(outputFile, json);
            }
            else
            {
                File.WriteAllBytes(outputFile, _glb.ToArray());
            }
        }
        /// <summary>
        /// get converted model
        /// </summary>
        /// <returns></returns>
        public GltfModel GetModel()
        {
            if (_model == null) Run();
            return _model;
        }
        /// <summary>
        /// get converted binary model
        /// </summary>
        /// <returns></returns>
        public List<byte> GetGlb()
        {
            if (_model == null) Run();
            return _glb;
        }
        /// <summary>
        /// get batch table if batch table enabled
        /// </summary>
        /// <returns></returns>
        public BatchTable GetBatchTable()
        {
            if (_model == null) Run();
            return _buffers.BatchTableJson;
        }
        /// <summary>
        /// run converter
        /// </summary>
        public void Run()
        {
            if (_model == null)
            {
                _model = new GltfModel();
                //TODO:
                if (_objModel == null)
                {
                    using (_objParser)
                    {
                        _objModel = _objParser.GetModel();
                    }
                }
                _model.Scenes.Add(new Scene());
                var u32IndicesEnabled = RequiresUint32Indices(_objModel);
                var meshes = _objModel.Geometries;
                var meshesLength = meshes.Count;
                for (var i = 0; i < meshesLength; i++)
                {
                    var mesh = meshes[i];
                    var meshIndex = AddMesh(_objModel, mesh, u32IndicesEnabled);
                    AddNode(mesh.Id, meshIndex, null);
                }

                if (_model.Images.Count > 0)
                {
                    _model.Samplers.Add(new Sampler
                    {
                        MagFilter = MagFilter.Linear,
                        MinFilter = MinFilter.NearestMipmapLinear,
                        WrapS = WrappingMode.Repeat,
                        WrapT = WrappingMode.Repeat
                    });
                }

                var allBuffers = AddBuffers(_options.Name);
                _model.Buffers.Add(new Gltf.Buffer
                {
                    Name = _options.Name,
                    ByteLength = allBuffers.Count
                });
                var boundary = 4;
                FillImageBuffers(allBuffers, boundary);


                if (!_options.Binary)
                {
                    _model.Buffers[0].Uri = "data:application/octet-stream;base64," + Convert.ToBase64String(allBuffers.ToArray());
                }
                else
                {
                    _glb = GltfToGlb(allBuffers);
                }
                _model.Clean();

            }
            
            
        }

        private static string ToJson(object model)
        {
            return JsonConvert.SerializeObject(model,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    });
        }

        private bool CheckWindingCorrect(Vec3 a, Vec3 b, Vec3 c, Vec3 normal)
        {
            var ba = new Vec3(b.X - a.X, b.Y - a.Y, b.Z - a.Z);
            var ca = new Vec3(c.X - a.X, c.Y - a.Y, c.Z - a.Z);
            var cross = Vec3.Cross(ba, ca);

            return Vec3.Dot(normal, cross) > 0;

        }

        private bool RequiresUint32Indices(ObjModel objModel)
        {
            return objModel.Vertices.Count > 65534;            
        }

        #region Buffers

        private List<byte> GltfToGlb(List<byte> binaryBuffer)
        {
            var buffer = _model.Buffers[0];
            if (!String.IsNullOrEmpty(buffer.Uri))
            {
                binaryBuffer = new List<byte>();
            }
            var jsonBuffer = GetJsonBufferPadded(_model);
            // Allocate buffer (Global header) + (JSON chunk header) + (JSON chunk) + (Binary chunk header) + (Binary chunk)
            var glbLength = 12 + 8 + jsonBuffer.Length + 8 + binaryBuffer.Count;

            var glb = new List<byte>(glbLength);

            // Write binary glTF header (magic, version, length)
            var byteOffset = 0;
            glb.AddRange(BitConverter.GetBytes((uint)0x46546C67));
            byteOffset += 4;
            glb.AddRange(BitConverter.GetBytes((uint)2));
            byteOffset += 4;
            glb.AddRange(BitConverter.GetBytes((uint)glbLength));
            byteOffset += 4;

            // Write JSON Chunk header (length, type)
            glb.AddRange(BitConverter.GetBytes((uint)jsonBuffer.Length));
            byteOffset += 4;
            glb.AddRange(BitConverter.GetBytes((uint)0x4E4F534A)); // Json
            byteOffset += 4;
            // Write JSON Chunk
            glb.AddRange(jsonBuffer);
            byteOffset += jsonBuffer.Length;

            // Write Binary Chunk header (length, type)
            glb.AddRange(BitConverter.GetBytes((uint)binaryBuffer.Count));
            byteOffset += 4;
            glb.AddRange(BitConverter.GetBytes((uint)0x004E4942)); // BIN
            byteOffset += 4;
            // Write Binary Chunk
            glb.AddRange(binaryBuffer);

            return glb;

        }
        /// <summary>
        /// padding json buffer
        /// </summary>
        /// <param name="model"></param>
        /// <param name="boundary"></param>
        /// <param name="offset">The byte offset on which the buffer starts.</param>
        /// <returns></returns>
        public static byte[] GetJsonBufferPadded(object model, int boundary = 4, int offset = 0)
        {
            var json = ToJson(model);
            var bs = Encoding.UTF8.GetBytes(json);
            var remainder = (offset + bs.Length) % boundary;
            var padding = (remainder == 0) ? 0 : boundary - remainder;
            for(var i = 0;i< padding; i++)
            {
                json += " ";
            }
            return Encoding.UTF8.GetBytes(json);
        }

        private int AddIndexArray(int[] indices, bool u32IndicesEnabled, string name)
        {
            var cType = u32IndicesEnabled ? ComponentType.U32 : ComponentType.U16;

            var count = indices.Length;
            var minMax = new MinMax();
            UpdateMinMax(indices.Select(c=>(double)c).ToArray(), minMax);

            var accessor = new Accessor
            {
                Type = AccessorType.SCALAR,
                ComponentType = cType,
                Count = count,
                Min = new[] { Math.Round(minMax.Min) },
                Max = new[] { Math.Round(minMax.Max) },
                Name = name
            };

            var index = _model.Accessors.Count;
            _model.Accessors.Add(accessor);
            return index;
        }

        private static byte[] ToU32Buffer(int[] arr)
        {
            var bytes = new List<byte>();
            foreach(var i in arr)
            {
                bytes.AddRange(BitConverter.GetBytes((uint)i));
            }
            return bytes.ToArray();
        }

        private static byte[] ToU16Buffer(int[] arr)
        {
            var bytes = new List<byte>();
            foreach (var i in arr)
            {
                bytes.AddRange(BitConverter.GetBytes((ushort)i));
            }
            return bytes.ToArray();
        }
        /// <summary>
        /// padding buffers with boundary
        /// </summary>
        /// <param name="buffers"></param>
        /// <param name="boundary"></param>
        public static void PaddingBuffers(List<byte> buffers, int boundary = 4)
        {
            var length = buffers.Count;
            var remainder = length % boundary;
            if (remainder != 0)
            {
                var padding = boundary - remainder;
                for (var i = 0; i < padding; i++)
                {
                    buffers.Add(0);
                }
            }
        }
        
        private List<byte> AddBuffers(string name)
        {
            BufferState bufferState = _buffers;
            AddBufferView(bufferState.PositionBuffers, bufferState.PositionAccessors.ToArray(), 12, 0x8892);
            AddBufferView(bufferState.NormalBuffers, bufferState.NormalAccessors.ToArray(), 12, 0x8892);
            AddBufferView(bufferState.UvBuffers, bufferState.UvAccessors.ToArray(), 8, 0x8892); // ARRAY_BUFFER
            AddBufferView(bufferState.IndexBuffers, bufferState.IndexAccessors.ToArray(), null, 0x8893); // ELEMENT_ARRAY_BUFFER
            if (_options.WithBatchTable)
            {
                AddBufferView(bufferState.BatchIdBuffers, bufferState.BatchIdAccessors.ToArray(), 0, 0x8892);
            }

            var buffers = new List<byte>();
            foreach(var b in bufferState.PositionBuffers)
            {
                buffers.AddRange(b);
            }
            foreach(var b in bufferState.NormalBuffers)
            {
                buffers.AddRange(b);
            }
            foreach(var b in bufferState.UvBuffers)
            {
                buffers.AddRange(b);
            }
            foreach(var b in bufferState.IndexBuffers)
            {
                buffers.AddRange(b);
            }
            if (_options.WithBatchTable)
            {
                foreach(var b in bufferState.BatchIdBuffers)
                {
                    buffers.AddRange(b);
                }                
            }
            PaddingBuffers(buffers);
            return buffers;
        }

        private void AddBufferView(List<byte[]> buffers, 
            int[] accessors, int? byteStride, int? target)
        {
            if (buffers.Count == 0) return;
            
            BufferView previousBufferView = null;
            if (_model.BufferViews.Count > 0)
            {
                previousBufferView = _model.BufferViews[_model.BufferViews.Count - 1];
            }
            var byteOffset = previousBufferView != null ? 
                previousBufferView.ByteOffset + previousBufferView.ByteLength : 0;
            var byteLength = 0;
            var bufferViewIndex = _model.BufferViews.Count;
            for (var i =0; i < buffers.Count; i++)
            {
                var accessor = _model.Accessors[accessors[i]];
                accessor.BufferView = bufferViewIndex;
                accessor.ByteOffset = byteLength;
                byteLength += buffers[i].Length;
            }
            var bf = new BufferView
            {
                Name = "bufferView_" + bufferViewIndex,
                Buffer = 0,
                ByteLength = byteLength,
                ByteOffset = byteOffset,
                ByteStride = byteStride,
                Target = target
            };
            _model.BufferViews.Add(bf);
        }

        private void FillImageBuffers(List<byte> buffers, int boundary)
        {
            var bufferViewIndex = _model.BufferViews.Count;
            var byteOffset = buffers.Count;
            foreach (var img in _model.Images)
            {
                var imageFile = Path.Combine(_objFolder, img.Name);
                var textureSource = File.ReadAllBytes(imageFile);
                var textureByteLength = textureSource.Length;
                img.BufferView = _model.BufferViews.Count;
                _model.BufferViews.Add(new BufferView
                {
                    Buffer = 0,
                    ByteOffset = byteOffset,
                    ByteLength = textureByteLength
                });
                byteOffset += textureByteLength;
                buffers.AddRange(textureSource);
            }
            // Padding Buffers
            PaddingBuffers(buffers);
            _model.Buffers[0].ByteLength = buffers.Count;
        }

        #endregion Buffers

        #region Materials

        /// <summary>
        /// Translate the blinn-phong model to the pbr metallic-roughness model
        /// Roughness factor is a combination of specular intensity and shininess
        /// Metallic factor is 0.0
        /// Textures are not converted for now
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static double Luminance(Color color)
        {
            return color.Red * 0.2125 + color.Green * 0.7154 + color.Blue * 0.0721;
        }

        private int AddTexture(string textureFilename)
        {
            var image = new Image
            {
                Name = textureFilename,
                BufferView = 0
            };
            var ext = Path.GetExtension(textureFilename).ToUpper();
            switch(ext)
            {
                case ".PNG":
                    image.MimeType = "image/png";
                    break;
                case ".JPEG":
                case ".JPG":
                    image.MimeType = "image/jpeg";
                    break;
                case ".GIF":
                    image.MimeType = "image/gif";
                    break;
            }
            var imageIndex = _model.Images.Count;
            _model.Images.Add(image);

            var textureIndex = _model.Textures.Count;
            var t = new Gltf.Texture
            {
                Name = textureFilename,
                Source = imageIndex,
                Sampler = 0
            };
            _model.Textures.Add(t);
            return textureIndex;
        }

        private Gltf.Material GetDefault(string name = "default", AlphaMode mode = AlphaMode.OPAQUE)
        {
            return new Gltf.Material
            {
                AlphaMode = mode,
                Name = name,
                //EmissiveFactor = new double[] { 1, 1, 1 },
                PbrMetallicRoughness = new PbrMetallicRoughness
                {
                    BaseColorFactor = new double[] { 0.5, 0.5, 0.5, 1 },
                    MetallicFactor = 1.0,
                    RoughnessFactor = 0.0
                }
            };
        }

        private static double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mat"></param>
        /// <returns>roughnessFactor</returns>
        private static double ConvertTraditional2MetallicRoughness(WaveFront.Material mat)
        {
            // Transform from 0-1000 range to 0-1 range. Then invert.
            //var roughnessFactor = mat.SpecularExponent; // options.metallicRoughness ? 1.0 : 0.0;
            //roughnessFactor = roughnessFactor / 1000.0;
            var roughnessFactor = 1.0 - mat.SpecularExponent / 1000.0;
            roughnessFactor = Clamp(roughnessFactor, 0.0, 1.0);

            if (mat.Specular == null || mat.Specular.Color == null)
            {
                mat.Specular = new Reflectivity(new Color());
                return roughnessFactor;
            }
            // Translate the blinn-phong model to the pbr metallic-roughness model
            // Roughness factor is a combination of specular intensity and shininess
            // Metallic factor is 0.0
            // Textures are not converted for now
            var specularIntensity = Luminance(mat.Specular.Color);
            

            // Low specular intensity values should produce a rough material even if shininess is high.
            if (specularIntensity < 0.1)
            {
                roughnessFactor *= (1.0 - specularIntensity);
            }

            var metallicFactor = 0.0;
            mat.Specular = new Reflectivity(new Color(metallicFactor));
            return roughnessFactor;
        }

        private int AddMaterial(WaveFront.Material mat)
        {
            Gltf.Material gMat = null;
            if (mat == null)
            {
                gMat = GetDefault();
            }
            else
            {
                var roughnessFactor = ConvertTraditional2MetallicRoughness(mat);

                gMat = new Gltf.Material
                {
                    Name = mat.Name,
                    AlphaMode = AlphaMode.OPAQUE
                };
                var hasTexture = !String.IsNullOrEmpty(mat.DiffuseTextureFile);
                var alpha = mat.GetAlpha();
                var metallicFactor = 0.0;
                if (mat.Specular != null && mat.Specular.Color != null)
                {
                    metallicFactor = mat.Specular.Color.Red;
                }
                gMat.PbrMetallicRoughness = new PbrMetallicRoughness
                {
                    RoughnessFactor = roughnessFactor,
                    MetallicFactor = metallicFactor
                };
                if (mat.Diffuse != null)
                {
                    gMat.PbrMetallicRoughness.BaseColorFactor = mat.Diffuse.Color.ToArray(alpha);
                }
                else if (mat.Ambient != null)
                {
                    gMat.PbrMetallicRoughness.BaseColorFactor = mat.Ambient.Color.ToArray(alpha);
                }
                else
                {
                    gMat.PbrMetallicRoughness.BaseColorFactor = new double[] { 0.7, 0.7, 0.7, alpha };
                }
                

                if (hasTexture)
                {
                    int index = -1;
                    for (var i = 0; i < _model.Textures.Count; i++)
                    {
                        if (mat.DiffuseTextureFile == _model.Textures[i].Name)
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index == -1)
                    {
                        index = AddTexture(mat.DiffuseTextureFile);
                    }
                    gMat.PbrMetallicRoughness.BaseColorTexture = new Info
                    {
                        Index = index
                    };
                }

                if (mat.Emissive != null && mat.Emissive.Color != null)
                {
                    gMat.EmissiveFactor = mat.Emissive.Color.ToArray();
                }

                if (alpha < 1.0)
                {
                    gMat.AlphaMode = AlphaMode.BLEND;
                    gMat.DoubleSided = true;
                }
            }
            
            var matIndex = _model.Materials.Count;
            _model.Materials.Add(gMat);
            return matIndex;

        }

        private int GetMaterial(ObjModel objModel, string matName)
        {
            if (String.IsNullOrEmpty(matName))
            {
                matName = "default";
            }
            for(var i = 0;i< _model.Materials.Count;i++)
            {
                if (_model.Materials[i].Name == matName)
                {
                    return i;
                }
            }
            var mat = objModel.Materials.FirstOrDefault(c => c.Name == matName);
            var gMatIndex = AddMaterial(mat);
            return gMatIndex;
        }

        #endregion Materials

        #region Meshes

        private int AddMesh(ObjModel objModel, Geometry mesh, bool uint32Indices)
        {
            var ps = AddVertexAttributes(objModel, mesh, uint32Indices);

            var m = new Mesh
            {
                Name = mesh.Id,
                Primitives = ps
            };
            var meshIndex = _model.Meshes.Count;
            _model.Meshes.Add(m);
            return meshIndex;

        }

        /// <summary>
        /// update bounding box with double array
        /// </summary>
        /// <param name="vs"></param>
        /// <param name="minMax"></param>

        public static void UpdateMinMax(double[] vs, MinMax minMax)
        {
            var min = vs.Min();
            var max = vs.Max();
            if (minMax.Min > min)
            {
                minMax.Min = min;
            }
            if (minMax.Max < max)
            {
                minMax.Max = max;
            }
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objModel"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private List<Primitive> AddVertexAttributes(ObjModel objModel, Geometry mesh,
            bool uint32Indices)
        {
            var facesGroup = mesh.Faces.GroupBy(c => c.MatName);
            var faces = new List<Face>();
            foreach(var fg in facesGroup)
            {
                var matName = fg.Key;
                var f = new Face { MatName = matName };
                foreach(var ff in fg)
                {
                    f.Triangles.AddRange(ff.Triangles);
                }
                if (f.Triangles.Count > 0)
                {
                    faces.Add(f);
                }                
            }

            var hasPositions = faces.Count > 0;
            var hasUvs = faces.Any(c => c.Triangles.Any(d => d.V1.T > 0));
            var hasNormals = faces.Any(c => c.Triangles.Any(d => d.V1.N > 0));

            var vertices = objModel.Vertices;
            var normals = objModel.Normals;
            var uvs = objModel.Uvs;

            // Vertex attributes are shared by all primitives in the mesh
            var name0 = mesh.Id;

            var ps = new List<Primitive>(faces.Count * 2);
            var index = 0;
            foreach (var f in faces)
            {
                var faceName = name0;
                if (index > 0)
                {
                    faceName = name0 + "_" + index;
                }
                MinMax vmmX = new MinMax(), vmmY = new MinMax(), vmmZ = new MinMax();
                MinMax nmmX = new MinMax(), nmmY = new MinMax(), nmmZ = new MinMax();
                MinMax tmmX = new MinMax(), tmmY = new MinMax();
                var vList = 0;
                var nList = 0;
                var tList = 0;
                var vs = new List<byte>(); // vertexBuffers
                var ns = new List<byte>(); // normalBuffers
                var ts = new List<byte>(); // textureBuffers

                // every primitive need their own vertex indices(v,t,n)
                Dictionary<string, int> FaceVertexCache = new Dictionary<string, int>();
                int FaceVertexCount = 0;

                //List<int[]> indiceList = new List<int[]>(faces.Count * 2);
                //var matIndexList = new List<int>(faces.Count * 2);

                // f is a primitive
                var iList = new List<int>(f.Triangles.Count*3*2); // primitive indices
                foreach(var t in f.Triangles)
                {
                    var v1Index = t.V1.V - 1;
                    var v2Index = t.V2.V - 1;
                    var v3Index = t.V3.V - 1;
                    var v1 = vertices[v1Index];                    
                    var v2 = vertices[v2Index];                    
                    var v3 = vertices[v3Index];                    
                    UpdateMinMax(new[] { v1.X, v2.X, v3.X }, vmmX);
                    UpdateMinMax(new[] { v1.Y, v2.Y, v3.Y }, vmmY);
                    UpdateMinMax(new[] { v1.Z, v2.Z, v3.Z }, vmmZ);

                    Vec3 n1 = new Vec3(), n2 = new Vec3(), n3 = new Vec3();
                    if (t.V1.N > 0) // hasNormals
                    {
                        var n1Index = t.V1.N - 1;
                        var n2Index = t.V2.N - 1;
                        var n3Index = t.V3.N - 1;
                        n1 = normals[n1Index];
                        n2 = normals[n2Index];
                        n3 = normals[n3Index];
                        UpdateMinMax(new[] { n1.X, n2.X, n3.X }, nmmX);
                        UpdateMinMax(new[] { n1.Y, n2.Y, n3.Y }, nmmY);
                        UpdateMinMax(new[] { n1.Z, n2.Z, n3.Z }, nmmZ);
                    }
                    Vec2 t1 = new Vec2(), t2 = new Vec2(), t3 = new Vec2();
                    if (t.V1.T > 0) // hasUvs
                    {
                        var t1Index = t.V1.T - 1;
                        var t2Index = t.V2.T - 1;
                        var t3Index = t.V3.T - 1;
                        t1 = uvs[t1Index];
                        t2 = uvs[t2Index];
                        t3 = uvs[t3Index];
                        UpdateMinMax(new[] { t1.U, t2.U, t3.U }, tmmX);
                        UpdateMinMax(new[] { 1 - t1.V, 1 - t2.V, 1 - t3.V }, tmmY);
                    }
                    

                    var v1Str = t.V1.ToString();
                    if (!FaceVertexCache.ContainsKey(v1Str))
                    {
                        FaceVertexCache.Add(v1Str, FaceVertexCount++);

                        vList++; vs.AddRange(v1.ToFloatBytes());
                        if (t.V1.N > 0) // hasNormals
                        {
                            nList++; ns.AddRange(n1.ToFloatBytes());
                        }
                        if (t.V1.T > 0) // hasUvs
                        {
                            tList++; ts.AddRange(new Vec2(t1.U, 1 - t1.V).ToFloatBytes());
                        }
                        
                    }

                    var v2Str = t.V2.ToString();
                    if (!FaceVertexCache.ContainsKey(v2Str))
                    {
                        FaceVertexCache.Add(v2Str, FaceVertexCount++);

                        vList++; vs.AddRange(v2.ToFloatBytes());
                        if (t.V2.N > 0) // hasNormals
                        {
                            nList++; ns.AddRange(n2.ToFloatBytes());
                        }
                        if (t.V2.T > 0) // hasUvs
                        {
                            tList++; ts.AddRange(new Vec2(t2.U, 1 - t2.V).ToFloatBytes());
                        }
                        
                    }

                    var v3Str = t.V3.ToString();
                    if (!FaceVertexCache.ContainsKey(v3Str))
                    {
                        FaceVertexCache.Add(v3Str, FaceVertexCount++);

                        vList++; vs.AddRange(v3.ToFloatBytes());
                        if (t.V3.N > 0) // hasNormals
                        {
                            nList++; ns.AddRange(n3.ToFloatBytes());
                        }
                        if (t.V3.T > 0) // hasUvs
                        {
                            tList++; ts.AddRange(new Vec2(t3.U, 1 - t3.V).ToFloatBytes());
                        }
                        
                    }

                    // Vertex Indices
                    var correctWinding = CheckWindingCorrect(v1, v2, v3, n1);
                    if (correctWinding)
                    {
                        iList.AddRange(new[] {
                            FaceVertexCache[v1Str],
                            FaceVertexCache[v2Str],
                            FaceVertexCache[v3Str]
                        });
                    }
                    else
                    {
                        iList.AddRange(new[] {
                            FaceVertexCache[v1Str],
                            FaceVertexCache[v3Str],
                            FaceVertexCache[v2Str]
                        });
                    }
                    
                }

                var materialIndex = GetMaterial(objModel, f.MatName);
                //matIndexList.Add(materialIndex);


                var atts = new Dictionary<string, int>();

                var accessorIndex = _model.Accessors.Count;
                var accessorVertex = new Accessor
                {
                    Min = new double[] { vmmX.Min, vmmY.Min, vmmZ.Min },
                    Max = new double[] { vmmX.Max, vmmY.Max, vmmZ.Max },
                    Type = AccessorType.VEC3,
                    Count = vList,
                    ComponentType = ComponentType.F32,
                    Name = faceName + "_positions"
                };
                _model.Accessors.Add(accessorVertex);
                atts.Add("POSITION", accessorIndex);
                _buffers.PositionBuffers.Add(vs.ToArray());
                _buffers.PositionAccessors.Add(accessorIndex);

                if (_options.WithBatchTable)
                {
                    _buffers.BatchTableJson.MaxPoint.Add(accessorVertex.Max);
                    _buffers.BatchTableJson.MinPoint.Add(accessorVertex.Min);
                }

                if (nList > 0) //hasNormals)
                {
                    accessorIndex = _model.Accessors.Count;
                    var accessorNormal = new Accessor
                    {
                        Min = new double[] { nmmX.Min, nmmY.Min, nmmZ.Min },
                        Max = new double[] { nmmX.Max, nmmY.Max, nmmZ.Max },
                        Type = AccessorType.VEC3,
                        Count = nList,
                        ComponentType = ComponentType.F32,
                        Name = faceName + "_normals"
                    };
                    _model.Accessors.Add(accessorNormal);
                    atts.Add("NORMAL", accessorIndex);
                    _buffers.NormalBuffers.Add(ns.ToArray());
                    _buffers.NormalAccessors.Add(accessorIndex);
                }

                if (tList > 0) //hasUvs)
                {
                    accessorIndex = _model.Accessors.Count;
                    var accessorUv = new Accessor
                    {
                        Min = new double[] { tmmX.Min, tmmY.Min },
                        Max = new double[] { tmmX.Max, tmmY.Max },
                        Type = AccessorType.VEC2,
                        Count = tList,
                        ComponentType = ComponentType.F32,
                        Name = faceName + "_texcoords"
                    };
                    _model.Accessors.Add(accessorUv);
                    atts.Add("TEXCOORD_0", accessorIndex);
                    _buffers.UvBuffers.Add(ts.ToArray());
                    _buffers.UvAccessors.Add(accessorIndex);
                }
                else
                {
                    var gMat = _model.Materials[materialIndex];
                    if (gMat.PbrMetallicRoughness.BaseColorTexture != null)
                    {
                        gMat.PbrMetallicRoughness.BaseColorTexture = null;
                    }
                }


                if (_options.WithBatchTable)
                {
                    var batchIdCount = vList;
                    accessorIndex = AddBatchIdAttribute(
                        _buffers.CurrentBatchId, batchIdCount, faceName + "_batchId");
                    atts.Add("_BATCHID", accessorIndex);
                    var batchIds = new List<byte>();
                    for (var i = 0; i < batchIdCount; i++)
                    {
                        batchIds.AddRange(BitConverter.GetBytes((ushort)_buffers.CurrentBatchId));
                    }
                    _buffers.BatchIdBuffers.Add(batchIds.ToArray());
                    _buffers.BatchIdAccessors.Add(accessorIndex);
                    _buffers.BatchTableJson.BatchIds.Add((ushort)_buffers.CurrentBatchId);
                    _buffers.BatchTableJson.Names.Add(faceName);
                    _buffers.CurrentBatchId++;
                }


                var indices = iList.ToArray();
                var indexAccessorIndex = AddIndexArray(indices, uint32Indices, faceName + "_indices");
                var indexBuffer = uint32Indices ? ToU32Buffer(indices) : ToU16Buffer(indices);
                _buffers.IndexBuffers.Add(indexBuffer);
                _buffers.IndexAccessors.Add(indexAccessorIndex);

                var p = new Primitive
                {
                    Attributes = atts,
                    Indices = indexAccessorIndex,
                    Material = materialIndex,//matIndexList[i],
                    Mode = Mode.Triangles
                };
                ps.Add(p);


                index++;
            }

            


            return ps;
        }

        private int AddBatchIdAttribute(int batchId, int count, string name)
        {
            //var ctype = u32IndicesEnabled ? ComponentType.U32 : ComponentType.U16;
            var ctype = ComponentType.U16;
            var accessor = new Accessor
            {
                Name = name,
                ComponentType = ctype,
                Count = count,
                Min = new double[] { batchId },
                Max = new double[] { batchId },
                Type = AccessorType.SCALAR
            };
            var accessorIndex = _model.Accessors.Count;
            _model.Accessors.Add(accessor);
            return accessorIndex;
        }

        private int AddNode(string name, int? meshIndex, int? parentIndex=null)
        {
            var node = new Node { Name = name, Mesh = meshIndex };
            var nodeIndex = _model.Nodes.Count;
            _model.Nodes.Add(node);
            //if (parentIndex != null)
            //{
            //    var pNode = _model.Nodes[parentIndex.Value];
            //    //TODO:
            //}
            //else
            //{
                
            //}
            _model.Scenes[_model.Scene].Nodes.Add(nodeIndex);

            return nodeIndex;
        }

        #endregion Meshes
    }
}
