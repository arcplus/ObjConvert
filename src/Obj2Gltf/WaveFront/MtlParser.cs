using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Arctron.Obj2Gltf.WaveFront
{
    public class MtlParser : IDisposable
    {
        private readonly string _mtlFile;

        private readonly StreamReader _reader;

        private readonly string _parentFolder;

        private Encoding _encoding;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mtlFile">material file</param>
        public MtlParser(string mtlFile):this(mtlFile, InitEncoding(mtlFile))
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mtlFile">material file</param>
        /// <param name="encoding"></param>
        public MtlParser(string mtlFile, Encoding encoding)
        {
            _mtlFile = mtlFile;
            _parentFolder = Path.GetDirectoryName(mtlFile);
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            _encoding = encoding;
            _reader = new StreamReader(mtlFile, _encoding);
        }

        internal static Encoding InitEncoding(string mtlFile)
        {
            Encoding encoding = null;
            using (var sr = new StreamReader(mtlFile, true))
            {
                sr.Read();
                encoding = sr.CurrentEncoding;
                sr.Close();
            }
            return encoding;
        }

        internal static Encoding InitEncoding(Stream stream)
        {
            Encoding encoding = null;
            using (var sr = new StreamReader(stream, true))
            {
                sr.ReadLine();
                encoding = sr.CurrentEncoding;
                sr.Close();
            }
            return encoding;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public MtlParser(Stream stream) : this(stream, InitEncoding(stream)) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        public MtlParser(Stream stream, Encoding encoding)
        {
            _reader = new StreamReader(stream, encoding);
        }

        private static Reflectivity GetReflectivity(string val)
        {
            if (String.IsNullOrEmpty(val)) return null;
            var strs = val.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (strs.Length == 3)
            {
                var r = double.Parse(strs[0]);
                var g = double.Parse(strs[1]);
                var b = double.Parse(strs[2]);

                return new Reflectivity(new Color(r, g, b));
            }

            //TODO:
            return null;
        }

        private Transparency GetTransparency(string str)
        {
            double val;
            var ok = double.TryParse(str, out val);
            if (ok)
            {
                return new Transparency { Factor = val };
            }
            return null;
        }

        private Dissolve GetDissolve(string str)
        {
            var strs = str.Split(new char[]{' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
            double val;
            var ok = double.TryParse(strs[strs.Length-1], out val);
            if (!ok) return null;
            if (val == 0)
            {
                val = 1.0f;
            }
            var d =new Dissolve{Factor = val};
            if (strs[0] == "-halo")
            {
                d.Halo = true;
            }
            return d;
        }
        /// <summary>
        /// get all mats in mtl file
        /// </summary>
        /// <returns></returns>
        public ICollection<Material> GetMats()
        {
            var mats = new List<Material>();
            var matStrs = new List<List<string>>();
            while(!_reader.EndOfStream)
            {
                var line = _reader.ReadLine().Trim();
                if (line.StartsWith("newmtl"))
                {
                    matStrs.Add(new List<string> { line });
                }
                else if (matStrs.Count > 0)
                {
                    matStrs[matStrs.Count - 1].Add(line);
                }
            }
            foreach(var matS in matStrs)
            {
                var m = new Material();
                foreach(var line in matS)
                {
                    if (line.StartsWith("newmtl"))
                    {
                        var matName = line.Substring("newmtl".Length).Trim();
                        m.Name = matName;
                    }
                    else if (line.StartsWith("Ka"))
                    {
                        var ka = line.Substring("Ka".Length).Trim();
                        var r = GetReflectivity(ka);
                        if (r != null)
                        {
                            m.Ambient = r;
                        }
                    }
                    else if (line.StartsWith("Kd"))
                    {
                        var kd = line.Substring("Kd".Length).Trim();
                        var r = GetReflectivity(kd);
                        if (r != null)
                        {
                            m.Diffuse = r;
                        }
                    }
                    else if (line.StartsWith("Ks"))
                    {
                        var ks = line.Substring("Ks".Length).Trim();
                        var r = GetReflectivity(ks);
                        if (r != null)
                        {
                            m.Specular = r;
                        }
                    }
                    else if (line.StartsWith("Ke"))
                    {
                        var ks = line.Substring("Ke".Length).Trim();
                        var r = GetReflectivity(ks);
                        if (r != null)
                        {
                            m.Emissive = r;
                        }
                    }
                    else if (line.StartsWith("d"))
                    {
                        var d = line.Substring("d".Length).Trim();
                        m.Dissolve = GetDissolve(d);
                    }
                    else if (line.StartsWith("Tr"))
                    {
                        var tr = line.Substring("Tr".Length).Trim();
                        m.Transparency = GetTransparency(tr);
                    }
                    else if (line.StartsWith("Ns"))
                    {
                        var ns = line.Substring("Ns".Length).Trim();
                        if (ns.Contains("."))
                        {
                            var d = float.Parse(ns);
                            m.SpecularExponent = (int)Math.Round(d);
                        }
                        else
                        {
                            m.SpecularExponent = int.Parse(ns);
                        }                           
                    }
                    else if (line.StartsWith("map_Ka"))
                    {
                        var ma = line.Substring("map_Ka".Length).Trim();
                        if (File.Exists(Path.Combine(_parentFolder, ma)))
                        {
                            m.AmbientTextureFile = ma;
                        }
                    }
                    else if (line.StartsWith("map_Kd"))
                    {
                        var md = line.Substring("map_Kd".Length).Trim();
                        if (File.Exists(Path.Combine(_parentFolder, md)))
                        {
                            m.DiffuseTextureFile = md;
                        }
                    }
                }
                mats.Add(m);
            }
            return mats;
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();                
            }
        }
        
    }
}
