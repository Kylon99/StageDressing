using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StageDressing.Models
{
    [Serializable]
    public class PrefabData
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public List<InstanceData> Instances { get; set; }

        /// <summary>
        /// The loaded Prefab.  Null when it has failed to load.
        /// </summary>
        [JsonIgnore]
        public GameObject Prefab { get; set; }

        /// <summary>
        /// Whether there was an error loading this prefab
        /// </summary>
        [JsonIgnore]
        public bool Error { get; set; }
    }
}
