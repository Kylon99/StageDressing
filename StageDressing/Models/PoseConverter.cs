using Newtonsoft.Json;
using System;
using UnityEngine;

namespace StageDressing.Models
{
    /// <summary>
    /// This class serializes and deserializes the UnityEngine.Pose class
    /// to and from JSON
    /// </summary>
    public class PoseConverter : JsonConverter<Pose>
    {
        public override Pose ReadJson(JsonReader reader, Type objectType, Pose value, bool hasvalue, JsonSerializer serializer)
        {
            float[] values = serializer.Deserialize<float[]>(reader);
            return new Pose
            {
                position = new Vector3(values[0], values[1], values[2]),
                rotation = new Quaternion(values[3], values[4], values[5], values[6]),
            };
        }

        public override void WriteJson(JsonWriter writer, Pose value, JsonSerializer serializer)
        {
            float[] values = new float[7];

            values[0] = value.position.x;
            values[1] = value.position.y;
            values[2] = value.position.z;
            values[3] = value.rotation.x;
            values[4] = value.rotation.y;
            values[5] = value.rotation.z;
            values[6] = value.rotation.w;

            serializer.Serialize(writer, values);
        }
    }
}
