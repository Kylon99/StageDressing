using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace StageDressing.Models
{
    /// <summary>
    /// The manager for all in game stage assets
    /// </summary>
    public class StageManager : PersistentSingleton<StageManager>
    {
        private readonly string stageAssetsPath = Path.Combine(Application.dataPath, @"..\CustomStageAssets");
        private readonly string configurationFile = Path.Combine(Application.dataPath, @"..\UserData\StageConfiguration.json");

        public StageConfigurationData Configuration { get; private set; }

        /// <summary>
        /// Writes the current configuration to disk
        /// </summary>
        public void SaveConfiguration()
        {
            string fileName = Path.Combine(this.stageAssetsPath, this.configurationFile);
            string json = JsonConvert.SerializeObject(this.Configuration, Formatting.Indented, new PoseConverter());
            File.WriteAllText(fileName, json);
        }

        /// <summary>
        /// Clears out disabled GameObjects, Prefabs and Instsances and Loads only the unloaded Prefabs
        /// </summary>
        public void LoadPrefabs()
        {
            this.DestroyAllInstances();

            // Destroy unused prefabs
            IEnumerable<PrefabInfo> disabledPrefabs = Configuration.Scenes.SelectMany(s => s.Prefabs).Where(go => go.Enabled == false);
            disabledPrefabs.ToList().ForEach(go =>
            {
                GameObject.Destroy(go.Prefab);
                go.Prefab = null;
            });

            // Group by AssetBundles to prepare for loading
            IEnumerable<PrefabInfo> allPrefabs = this.Configuration.Scenes
                .Where(s => s.ShowInMenu || s.ShowInGame)
                .SelectMany(s => s.Prefabs)
                .Where(go => go.Enabled && go.Prefab == null);

            IEnumerable<IGrouping<string, PrefabInfo>> assetBundleGroups = allPrefabs.GroupBy(go => go.File);
            foreach (var assetBundleGroup in assetBundleGroups)
            {
                var assetBundle = AssetBundle.LoadFromFile(Path.Combine(stageAssetsPath, assetBundleGroup.Key));
                if (assetBundle == null)
                {
                    // AssetBundle could not be loaded; mark all game objects as error and skip
                    assetBundleGroup.ToList().ForEach(go => go.Error = true);
                    continue;
                }

                // Load all game objects from this bundle
                foreach (PrefabInfo prefab in assetBundleGroup)
                {
                    if (prefab.Prefab == null)
                    {
                        prefab.Prefab = assetBundle.LoadAsset<GameObject>(prefab.Name);
                        prefab.Prefab.name += "Prefab";
                        prefab.Error = prefab.Prefab == null;
                    }
                }

                assetBundle.Unload(false);
            }
        }

        /// <summary>
        /// Clean out unused instances for the Menu Scene and instantiate the visible and enabled instances 
        /// </summary>
        public void RebuildMenuInstances()
        {
            this.DestroyAllInstances();

            IEnumerable<PrefabInfo> enabledPrefabs = this.Configuration.Scenes.Where(s => s.ShowInMenu).SelectMany(s => s.Prefabs).Where(p => p.Enabled);
            enabledPrefabs.ToList().ForEach(prefab =>
            {
                prefab.Instances?.ForEach(instance =>
                {
                    instance.Instance = GameObject.Instantiate(prefab.Prefab);
                    instance.Instance.name += "_MenuInstance";
                    instance.Instance.transform.position = instance.Pose.position;
                    instance.Instance.transform.rotation = instance.Pose.rotation;
                    instance.Instance.transform.localScale = new Vector3(instance.Scale, instance.Scale, instance.Scale);
                    if (instance.GrabbableInMenu)
                    {
                        instance.ExpandedBounds = GrabbableBehavior.CalculateExpandedBounds(instance.Instance);
                    }
                    StageDressing.Logger.Info($"Rebuild Menu Instances - New Instance {instance.Instance.name}");
                });
            });
        }

        /// <summary>
        /// Destroys all non null instances
        /// </summary>
        public void DestroyAllInstances()
        {
            var allInstances = this.Configuration.Scenes.SelectMany(s => s.Prefabs).SelectMany(p => p.Instances);
            foreach (var instance in allInstances.Where(i => i.Instance != null))
            {
                StageDressing.Logger.Info($"Destroying instance: {instance.Instance.name}");
                GameObject.Destroy(instance.Instance);
                instance.Instance = null;
            }
        }

        /// <summary>
        /// Removes all Menu Scene disabled GameObject instances in even enabled ones in hidden scenes
        /// </summary>
        public void DestroyMenuInstances()
        {
            IEnumerable<PrefabInfo> allHiddenPrefabs = this.Configuration.Scenes.Where(s => !s.ShowInMenu).SelectMany(s => s.Prefabs).Where(g => g.Instances != null && g.Instances.Any());
            IEnumerable<PrefabInfo> allDisabledPrefabs = this.Configuration.Scenes.SelectMany(s => s.Prefabs).Where(g => !g.Enabled && g.Instances != null && g.Instances.Any());
            IEnumerable<PrefabInfo> allToBeDestroyed = allHiddenPrefabs.Concat(allDisabledPrefabs);
            allToBeDestroyed.ToList().ForEach(p =>
            {
                p.Instances.ForEach(i =>
                {
                    GameObject.Destroy(i.Instance);
                    i = null;
                });
            });
        }

        /// <summary>
        /// Instantiate only the visible and enabled instances for the Game Scene 
        /// </summary>
        public void CreateGameInstances()
        {
            this.DestroyAllInstances();

            // Get all enabled menu game objects
            IEnumerable<PrefabInfo> enabledGamePrefabs = this.Configuration.Scenes.Where(s => s.ShowInGame).SelectMany(s => s.Prefabs).Where(go => go.Enabled);
            enabledGamePrefabs.ToList().ForEach(p =>
            {
                p.Instances.ForEach(instance =>
                {
                    instance.Instance = GameObject.Instantiate(p.Prefab);
                    instance.Instance.name += "_GameInstance";
                    instance.Instance.transform.position = instance.Pose.position;
                    instance.Instance.transform.rotation = instance.Pose.rotation;
                    instance.Instance.transform.localScale = new Vector3(instance.Scale, instance.Scale, instance.Scale);
                    if (instance.GrabbableInGame)
                    {
                        instance.ExpandedBounds = GrabbableBehavior.CalculateExpandedBounds(instance.Instance);
                    }
                    StageDressing.Logger.Info($"Create Game Instances - New Instance {instance.Instance.name}");
                });
            });
        }

        /// <summary>
        /// Removes all Game Scene GameObject instances
        /// </summary>
        public void DestroyGameInstances()
        {
            IEnumerable<PrefabInfo> allGamePrefabs = this.Configuration.Scenes.SelectMany(s => s.Prefabs).Where(g => g.Instances != null && g.Instances.Any());
            allGamePrefabs.ToList().ForEach(p =>
            {
                p.Instances.ForEach(i =>
                {
                    GameObject.Destroy(i.Instance);
                    i = null;
                });
            });
        }



        private void Awake()
        {
            this.LoadConfiguration();
        }

        /// <summary>
        /// Loads the saved configuration from disk and also checks if the AssetBundle files
        /// exist.
        /// </summary>
        private void LoadConfiguration()
        {
            if (!File.Exists(configurationFile))
            {
                // Create a new empty configuration if it doesn't exist
                StageConfigurationData newConfiguration = new StageConfigurationData();
                string json = JsonConvert.SerializeObject(newConfiguration, Formatting.Indented, new PoseConverter());
                File.WriteAllText(configurationFile, json);

                this.Configuration = newConfiguration;
                return;
            }

            string configuration = File.ReadAllText(configurationFile);
            this.Configuration = JsonConvert.DeserializeObject<StageConfigurationData>(configuration, new PoseConverter());
        }

    }

}
