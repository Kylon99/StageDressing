using Newtonsoft.Json;
using System;
using UnityEngine;

namespace StageDressing.Models
{
    /// <summary>
    /// A convenience struct for storing a Vector3 position and a Quaternion rotation.  This class
    /// should be Serializeable into and from JSON as well.
    /// </summary>
    [JsonConverter(typeof(InstanceTransformConverter))]
    public class InstanceTransform
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float Scale { get; set; }
    }

    /// <summary>
    /// The Newtonsoft JSON Converter class for InstanceTransform
    /// </summary>
    public class InstanceTransformConverter : JsonConverter<InstanceTransform>
    {
        public override InstanceTransform ReadJson(JsonReader reader, Type objectType, InstanceTransform value, bool hasvalue, JsonSerializer serializer)
        {
            StageDressing.Logger.Info($"path: {reader.Path} value: {reader.Value}");

            float[] values = serializer.Deserialize<float[]>(reader);
            return new InstanceTransform
            {
                Position = new Vector3(values[0], values[1], values[2]),
                Rotation = new Quaternion(values[3], values[4], values[5], values[6]),
                Scale = values[7]
            };
        }

        public override void WriteJson(JsonWriter writer, InstanceTransform value, JsonSerializer serializer)
        {
            float[] values = new float[8];

            values[0] = value.Position.x;
            values[1] = value.Position.y;
            values[2] = value.Position.z;
            values[3] = value.Rotation.x;
            values[4] = value.Rotation.y;
            values[5] = value.Rotation.z;
            values[6] = value.Rotation.w;
            values[7] = value.Scale;

            serializer.Serialize(writer, values);
        }
    }
}
