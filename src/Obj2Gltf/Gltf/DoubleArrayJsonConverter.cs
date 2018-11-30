using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    /// <summary>
    /// Convert to int[] when all values are equals integers
    /// </summary>
    public class DoubleArrayJsonConverter : JsonConverter<double[]>
    {
        public override double[] ReadJson(JsonReader reader, Type objectType, 
            double[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var values = new List<double>();
            double? val;
            while((val = reader.ReadAsDouble()) != null)
            {
                values.Add(val.Value);
            }
            if (values.Count > 0)
            {
                return values.ToArray();
            }
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, 
            double[] value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var json = "[" + String.Join(",", 
                    value.Select(c => Math.Round(c) - c == 0 ? (int)c : c)) + "]";
                writer.WriteRawValue(json);
            }
            
        }
    }
}
