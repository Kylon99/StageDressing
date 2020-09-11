using System;
using System.Collections.Generic;

namespace StageDressing.Models
{
    [Serializable]
    public class SceneInfo
    {
        public string Name { get; set; }
        public bool ShowInMenu { get; set; }
        public bool ShowInGame { get; set; }
        public bool FollowRoomAdjust { get; set; }
        public List<PrefabInfo> Prefabs { get; set; }
    }
}
