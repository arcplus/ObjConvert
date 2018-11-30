using System;
using System.Collections.Generic;
using System.Text;

namespace Arctron.Obj2Gltf.WaveFront
{
    //http://paulbourke.net/dataformats/mtl/
    public class Material
    {
        public string Name { get; set; }

        public Reflectivity Ambient { get; set; } // Ka

        public Reflectivity Diffuse { get; set; } // Kd

        public string DiffuseTextureFile { get; set; } // map_Kd

        public string AmbientTextureFile { get; set; } // map_Ka

        public Reflectivity Specular { get; set; }
        /// <summary>
        /// transmission filter: Any light passing through the object 
        /// is filtered by the transmission filter
        /// </summary>
        public Reflectivity Filter { get; set; }
        /// <summary>
        /// illum illum_# 0 ~ 10
        /// </summary>
        public int? Illumination { get; set; }

        public Dissolve Dissolve { get; set; }
        /// <summary>
        /// Ns exponent 0 ~ 1000
        /// </summary>
        public int SpecularExponent { get; set; }
        /// <summary>
        /// sharpness value 0 ~ 1000, The default is 60
        /// </summary>
        public int? Sharpness { get; set; } = 60;
        /// <summary>
        /// 0.001 ~ 10
        /// </summary>
        public double? OpticalDensity { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"newmtl {Name}" + Environment.NewLine);
            if (Ambient != null)
            {
                sb.Append($"Ka {Diffuse}" + Environment.NewLine);
            }
            if (Diffuse != null)
            {
                sb.Append($"Kd {Diffuse}" + Environment.NewLine);
            }
            if (Specular != null)
            {
                sb.Append($"Ks {Specular}" + Environment.NewLine);
            }
            if (Dissolve != null && Dissolve.Factor < 1)
            {
                sb.Append(Dissolve + Environment.NewLine);
            }
            if (SpecularExponent > 0)
            {
                sb.Append($"Ns {SpecularExponent}" + Environment.NewLine);
            }
            if (!String.IsNullOrEmpty(AmbientTextureFile))
            {
                sb.Append($"map_Ka {DiffuseTextureFile}" + Environment.NewLine);
            }
            if (!String.IsNullOrEmpty(DiffuseTextureFile))
            {
                sb.Append($"map_Kd {DiffuseTextureFile}" + Environment.NewLine);
            }
            return sb.ToString();
        }
    }

    public class Reflectivity
    {
        public Color Color { get; }

        public Spectral Spectral { get; }

        public XYZ XYZ { get; }

        public ReflectivityType AmbientType { get; }

        public Reflectivity(Color color)
        {
            AmbientType = ReflectivityType.Color;
            Color = color;
        }

        public Reflectivity(Spectral spectral)
        {
            AmbientType = ReflectivityType.Spectral;
            Spectral = spectral;
        }

        public Reflectivity(XYZ xyz)
        {
            AmbientType = ReflectivityType.XYZ;
            XYZ = xyz;
        }

        public override string ToString()
        {
            switch (AmbientType)
            {
                case ReflectivityType.Color:
                    return Color.ToString();
                default: // TODO:
                    return String.Empty;
            }
        }
    }

    public enum ReflectivityType
    {
        Color,
        Spectral,
        XYZ
    }

    public class Color
    {
        public Color(double v)
        {
            Red = v;
            Green = v;
            Blue = v;
        }

        public Color(double r, double g, double b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
        public double Red { get; set; }

        public double Green { get; set; }

        public double Blue { get; set; }

        public override string ToString()
        {
            return $"{Red:0.0000} {Green:0.0000} {Blue:0.0000}";
        }

        public double[] ToArray(double? alpha=null)
        {
            if (alpha == null) return new double[] { Red, Green, Blue };
            return new double[] { Red, Green, Blue, alpha.Value };
        }

    }

    public class Spectral
    {
        public string Filename { get; set; }

        public int Factor { get; set; }
    }

    public class XYZ
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }
    }
    /// <summary>
    /// 1- transparency
    /// </summary>
    public class Dissolve
    {
        /// <summary>
        /// A factor of 1.0 is fully opaque.
        /// </summary>
        public double Factor { get; set; }

        public bool Halo { get; set; }

        public override string ToString()
        {
            if (!Halo)
            {
                return $"d {Factor}";
            }
            return $"d -halo {Factor}";
        }
    }
}
