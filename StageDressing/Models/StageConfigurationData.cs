using System;
using System.Collections.Generic;

namespace StageDressing.Models
{
    [Serializable]
    public class StageConfigurationData
    {
        public List<SceneInfo> Scenes { get; set; }
    }
}
