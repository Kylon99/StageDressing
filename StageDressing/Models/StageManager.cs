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
        /// Loads the saved configuration from disk
        /// exist.
        /// </summary>
        public void LoadConfiguration()
        {
            if (!File.Exists(configurationFile))
            {
                // Create a new empty configuration if it doesn't exist
                StageConfigurationData newConfiguration = new StageConfigurationData();
                string json = JsonConvert.SerializeObject(newConfiguration, Formatting.Indented);
                File.WriteAllText(configurationFile, json);

                this.Configuration = newConfiguration;
                return;
            }

            string configuration = File.ReadAllText(configurationFile);
            this.Configuration = JsonConvert.DeserializeObject<StageConfigurationData>(configuration);
        }

        /// <summary>
        /// Writes the current configuration to disk
        /// </summary>
        public void SaveConfiguration()
        {
            string json = JsonConvert.SerializeObject(this.Configuration, Formatting.Indented);
            File.WriteAllText(configurationFile, json);
        }

        /// <summary>
        /// Creates a new definition of a scene but does not load it yet
        /// </summary>
        /// <param name="assetBundleFile"></param>
        public void CreateNewScene(string assetBundleFile)
        {
            var assetBundle = AssetBundle.LoadFromFile(Path.Combine(stageAssetsPath, assetBundleFile));
            if (assetBundle == null)
            {
                StageDressing.Logger.Error($"Could not load asset bundle {assetBundleFile}");
                return;
            }

            SceneData scene = new SceneData()
            {
                Name = assetBundleFile,
                File = assetBundleFile,
                ShowInMenu = true,
                ShowInGame = false,
                FollowRoomAdjust = true,
                Prefabs = assetBundle.GetAllAssetNames().Select(a => new PrefabData
                {
                    Name = a,
                    Enabled = true,
                    Instances = new List<InstanceData>()
                }).ToList()
            };
        }

        /// <summary>
        /// Clears out disabled GameObjects, Prefabs and Instsances and Loads only the unloaded Prefabs
        /// </summary>
        public void LoadPrefabs()
        {
            this.DestroyAllInstances();

            // Destroy unused prefabs
            IEnumerable<PrefabData> disabledPrefabs = Configuration.Scenes.SelectMany(s => s.Prefabs).Where(go => go.Enabled == false);
            disabledPrefabs.ToList().ForEach(go =>
            {
                GameObject.Destroy(go.Prefab);
                go.Prefab = null;
            });

            // Load all prefabs in scenes that are shown and enabled
            foreach (var scene in this.Configuration.Scenes.Where(s => s.ShowInMenu || s.ShowInGame))
            {
                var assetBundle = AssetBundle.LoadFromFile(Path.Combine(stageAssetsPath, scene.File));
                if (assetBundle == null)
                {
                    // AssetBundle could not be loaded; mark all game objects as error and skip
                    scene.Error = true;
                    scene.Prefabs.ForEach(p => p.Error = true);
                    continue;
                }

                // Load all game objects from this bundle
                foreach (var prefab in scene.Prefabs)
                {
                    if (!prefab.Enabled) continue;
                    if (prefab.Prefab != null) GameObject.Destroy(prefab.Prefab);

                    prefab.Prefab = assetBundle.LoadAsset<GameObject>(prefab.Name);
                    prefab.Prefab.name += "Prefab";
                    prefab.Error = prefab.Prefab == null;
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

            IEnumerable<PrefabData> enabledPrefabs = this.Configuration.Scenes.Where(s => s.ShowInMenu).SelectMany(s => s.Prefabs).Where(p => p.Enabled);
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
            IEnumerable<PrefabData> allHiddenPrefabs = this.Configuration.Scenes.Where(s => !s.ShowInMenu).SelectMany(s => s.Prefabs).Where(g => g.Instances != null && g.Instances.Any());
            IEnumerable<PrefabData> allDisabledPrefabs = this.Configuration.Scenes.SelectMany(s => s.Prefabs).Where(g => !g.Enabled && g.Instances != null && g.Instances.Any());
            IEnumerable<PrefabData> allToBeDestroyed = allHiddenPrefabs.Concat(allDisabledPrefabs);
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
            IEnumerable<PrefabData> enabledGamePrefabs = this.Configuration.Scenes.Where(s => s.ShowInGame).SelectMany(s => s.Prefabs).Where(go => go.Enabled);
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
            IEnumerable<PrefabData> allGamePrefabs = this.Configuration.Scenes.SelectMany(s => s.Prefabs).Where(g => g.Instances != null && g.Instances.Any());
            allGamePrefabs.ToList().ForEach(p =>
            {
                p.Instances.ForEach(i =>
                {
                    GameObject.Destroy(i.Instance);
                    i = null;
                });
            });
        }

        /// <summary>
        /// Get a list of all the asset bundle file names in the StageDressing Asset Directory
        /// </summary>
        /// <returns>A list of file names</returns>
        public List<string> GetAllAssetBundles()
        {
            var filePathNames = Directory.GetFiles(this.stageAssetsPath).ToList();
            return filePathNames.Select(fp => Path.GetFileName(fp)).ToList();
        }

        /// <summary>
        /// Gets the names of all the assets in the AssetBundle.
        /// </summary>
        /// <param name="fileName">The file name of the AssetBundle</param>
        /// <returns>A list of names of assets.  A null is returned if the asset bundle failed to load</returns>
        public List<string> GetAllPrefabNames(string fileName)
        {
            var assetBundle = AssetBundle.LoadFromFile(Path.Combine(stageAssetsPath, fileName));
            if (assetBundle == null) return null;

            return assetBundle.GetAllAssetNames().ToList();
        }
    }

}
