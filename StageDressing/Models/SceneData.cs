using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace StageDressing.Models
{
    [Serializable]
    public class SceneData
    {
        public string Name { get; set; }
        public string File { get; set; }
        public bool ShowInMenu { get; set; }
        public bool ShowInGame { get; set; }
        public bool FollowRoomAdjust { get; set; }
        public List<PrefabData> Prefabs { get; set; }

        /// <summary>
        /// Whether there was an error loading this scene
        /// </summary>
        [JsonIgnore]
        public bool Error { get; set; }
    }
}
