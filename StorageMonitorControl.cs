using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Storage Monitor Control", "WhiteThunder", "1.1.1")]
    [Description("Allows storage monitors to be deployed to more container types.")]
    internal class StorageMonitorControl : CovalencePlugin
    {
        #region Fields

        private const string PermissionAll = "storagemonitorcontrol.owner.all";
        private const string PermissionEntityFormat = "storagemonitorcontrol.owner.{0}";

        private const string StorageMonitorBoneName = "storagemonitor";

        private WaitWhile WaitWhileSaving = new WaitWhile(() => SaveRestore.IsSaving);

        private HashSet<StorageMonitor> _quarryMonitors = new HashSet<StorageMonitor>();

        private Coroutine _saveRoutine;
        private Configuration _config;

        #endregion

        #region Hooks

        private void Init()
        {
            _config.GeneratePermissionNames();

            permission.RegisterPermission(PermissionAll, this);
            foreach (var entry in _config.Containers)
            {
                permission.RegisterPermission(entry.Value.PermissionName, this);
            }

            Unsubscribe(nameof(OnEntitySpawned));
        }

        private void OnServerInitialized()
        {
            foreach (var entity in BaseNetworkable.serverEntities)
            {
                var container = entity as StorageContainer;
                if (container != null)
                {
                    OnEntitySpawned(container);
                    continue;
                }

                var storageMonitor = entity as StorageMonitor;
                if (storageMonitor != null)
                {
                    var parentQuarry = storageMonitor.GetParentEntity() as MiningQuarry;
                    if (parentQuarry != null)
                        ReparentToClosestQuarryStorage(storageMonitor, parentQuarry);

                    OnEntitySpawned(storageMonitor);
                    continue;
                }
            }

            Subscribe(nameof(OnEntitySpawned));
        }

        private void Unload()
        {
            if (_saveRoutine != null)
            {
                ServerMgr.Instance.StopCoroutine(_saveRoutine);
            }

            ReparentMonitorsToQuarry();
        }

        private void OnServerSave()
        {
            _saveRoutine = ServerMgr.Instance.StartCoroutine(ReparentWhileSaving());
        }

        private void OnEntitySpawned(StorageContainer container)
        {
            if (container == null || NativelySupportsStorageMonitor(container))
                return;

            container.isMonitorable = ShouldEnableMonitoring(container);
        }

        private void OnEntitySpawned(StorageMonitor storageMonitor)
        {
            var parentContainer = storageMonitor.GetParentEntity() as StorageContainer;
            if (parentContainer == null || NativelySupportsStorageMonitor(parentContainer))
                return;

            var containerConfig = GetContainerConfig(parentContainer);
            if (containerConfig == null || !containerConfig.Enabled)
                return;

            if (IsQuarryStorage(parentContainer))
            {
                _quarryMonitors.Add(storageMonitor);
            }

            var transform = storageMonitor.transform;

            if (transform.localPosition != containerConfig.Position
                || transform.localRotation.eulerAngles != containerConfig.Rotation.eulerAngles)
            {
                transform.localPosition = containerConfig.Position;
                transform.localRotation = containerConfig.Rotation;
                storageMonitor.InvalidateNetworkCache();
                storageMonitor.SendNetworkUpdate_Position();
            }
        }

        private void OnEntityKill(StorageMonitor storageMonitor)
        {
            _quarryMonitors.Remove(storageMonitor);
        }

        #endregion

        #region Helper Methods

        private static bool NativelySupportsStorageMonitor(BaseEntity entity)
        {
            return entity.model != null
                && entity.FindBone(StorageMonitorBoneName) != entity.model.rootBone;
        }

        private static bool IsQuarryStorage(StorageContainer storageContainer, out MiningQuarry quarry)
        {
            // Ignore if the parent has saving enabled, in case a plugin added other containers to quarries.
            if (storageContainer.enableSaving)
            {
                quarry = null;
                return false;
            }

            quarry = storageContainer.GetParentEntity() as MiningQuarry;
            return quarry != null;
        }

        private static bool IsQuarryStorage(StorageContainer storageContainer)
        {
            MiningQuarry quarry;
            return IsQuarryStorage(storageContainer, out quarry);
        }

        private bool ShouldEnableMonitoring(StorageContainer container)
        {
            var containerConfig = GetContainerConfig(container);
            if (containerConfig == null || !containerConfig.Enabled)
                return false;

            if (!ContainerOwnerHasPermission(container, containerConfig))
                return false;

            return true;
        }

        private bool ContainerOwnerHasPermission(BaseEntity entity, ContainerConfig containerConfig)
        {
            if (!containerConfig.RequirePermission)
                return true;

            if (entity.OwnerID == 0)
                return false;

            var ownerIdString = entity.OwnerID.ToString();

            return permission.UserHasPermission(ownerIdString, PermissionAll)
                || permission.UserHasPermission(ownerIdString, containerConfig.PermissionName);
        }

        private void ReparentMonitorsToQuarry()
        {
            foreach (var storageMonitor in _quarryMonitors)
            {
                var parentContainer = storageMonitor.GetParentEntity() as StorageContainer;
                if (parentContainer == null)
                    continue;

                MiningQuarry quarry;
                if (IsQuarryStorage(parentContainer, out quarry))
                {
                    storageMonitor.SetParent(quarry, worldPositionStays: true);
                }
            }
        }

        private void ReparentToClosestQuarryStorage(StorageMonitor storageMonitor, MiningQuarry quarry)
        {
            var transform = storageMonitor.transform;

            var fuelStorage = quarry.fuelStoragePrefab.instance as StorageContainer;
            var fuelStorageSqrDistance = fuelStorage != null
                ? (fuelStorage.transform.position - transform.position).sqrMagnitude
                : float.PositiveInfinity;

            var outputStorage = quarry.hopperPrefab.instance as StorageContainer;
            var outputStorageSqrDistance = outputStorage != null
                ? (outputStorage.transform.position - transform.position).sqrMagnitude
                : float.PositiveInfinity;

            StorageContainer newStorageParent = null;
            if (fuelStorageSqrDistance < outputStorageSqrDistance)
            {
                newStorageParent = fuelStorage;
            }
            else if (outputStorageSqrDistance < fuelStorageSqrDistance)
            {
                newStorageParent = outputStorage;
            }

            if (newStorageParent != null)
            {
                storageMonitor.SetParent(newStorageParent, worldPositionStays: true);
                newStorageParent.SetSlot(BaseEntity.Slot.StorageMonitor, storageMonitor);

                // Remove delegate before adding just in case it's already present (there isn't a way to check).
                newStorageParent.inventory.onItemAddedRemoved -= storageMonitor.OnContainerChanged;
                newStorageParent.inventory.onItemAddedRemoved += storageMonitor.OnContainerChanged;
            }
        }

        private void ReparentMonitorsToQuarryContainers()
        {
            foreach (var storageMonitor in _quarryMonitors)
            {
                var quarry = storageMonitor.GetParentEntity() as MiningQuarry;
                if (quarry == null)
                    continue;

                ReparentToClosestQuarryStorage(storageMonitor, quarry);
            }
        }

        private IEnumerator ReparentWhileSaving()
        {
            TrackStart();
            ReparentMonitorsToQuarry();
            TrackEnd();

            yield return WaitWhileSaving;

            TrackStart();
            ReparentMonitorsToQuarryContainers();
            TrackEnd();
        }

        #endregion

        #region Configuration

        private ContainerConfig GetContainerConfig(StorageContainer container)
        {
            ContainerConfig containerConfig;
            return _config.Containers.TryGetValue(container.ShortPrefabName, out containerConfig)
                ? containerConfig
                : null;
        }

        private class Configuration : BaseConfiguration
        {
            public void GeneratePermissionNames()
            {
                foreach (var entry in Containers)
                {
                    // Make the permission name less redundant
                    entry.Value.PermissionName = string.Format(PermissionEntityFormat, entry.Key)
                        .Replace(".deployed", string.Empty)
                        .Replace("_deployed", string.Empty)
                        .Replace(".entity", string.Empty);
                }
            }

            [JsonProperty("Containers")]
            public Dictionary<string, ContainerConfig> Containers = new Dictionary<string, ContainerConfig>()
            {
                ["bbq.deployed"] = new ContainerConfig { Position = new Vector3(0.1f, 0, 0.3f), RotationAngle = 90 },
                ["coffinstorage"] = new ContainerConfig { Position = new Vector3(-0.8f, 0.57f, 0), RotationAngle = 10 },
                ["composter"] = new ContainerConfig { Position = new Vector3(0, 1.54f, 0.4f) },
                ["crudeoutput"] = new ContainerConfig { Position = new Vector3(-0.4f, 0, 2.5f), RotationAngle = 90 },
                ["dropbox.deployed"] = new ContainerConfig { Position = new Vector3(0.3f, 0.545f, -0.155f), RotationAngle = 184 },
                ["fridge.deployed"] = new ContainerConfig { Position = new Vector3(-0.2f, 1.995f, 0.2f), RotationAngle = 10 },
                ["fuelstorage"] = new ContainerConfig { Position = new Vector3(-1.585f, -0.034f, 0) },
                ["furnace"] = new ContainerConfig { Position = new Vector3(0, 1.53f, 0.05f) },
                ["furnace.large"] = new ContainerConfig { Position = new Vector3(0.31f, 0.748f, -1.9f), RotationAngle = 190 },
                ["guntrap.deployed"] = new ContainerConfig { Position = new Vector3(0, 0.032f, -0.3f), RotationAngle = 180 },
                ["hitchtrough.deployed"] = new ContainerConfig { Position = new Vector3(-0.82f, 0.65f, 0.215f) },
                ["hopperoutput"] = new ContainerConfig { Position = new Vector3(-0.71f, -0.02f, 1.25f) },
                ["locker.deployed"] = new ContainerConfig { Position = new Vector3(-0.67f, 2.238f, 0.04f), RotationAngle = 10 },
                ["mailbox.deployed"] = new ContainerConfig { Position = new Vector3(0f, 1.327f, 0.21f) },
                ["mixingtable.deployed"] = new ContainerConfig { Position = new Vector3(-0.9f, 0, 0) },
                ["planter.small.deployed"] = new ContainerConfig { Position = new Vector3(-1.22f, 0.482f, 0.3f) },
                ["planter.large.deployed"] = new ContainerConfig { Position = new Vector3(-1.22f, 0.482f, 1.22f) },
                ["refinery_small_deployed"] = new ContainerConfig { Position = new Vector3(0, 2.477f, 0), RotationAngle = 180 },
                ["survivalfishtrap.deployed"] = new ContainerConfig { Position = new Vector3(0, 0.4f, -0.6f) },
                ["woodbox_deployed"] = new ContainerConfig { Position = new Vector3(-0.24f, 0.55f, 0.14f), RotationAngle = 10 },
            };
        }

        private class ContainerConfig
        {
            [JsonProperty("Enabled")]
            public bool Enabled = false;

            [JsonProperty("RequirePermission")]
            public bool RequirePermission = false;

            [JsonProperty("Position")]
            public Vector3 Position;

            [JsonProperty("RotationAngle", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public float RotationAngle;

            private bool _rotationCached = false;
            private Quaternion _rotation;

            [JsonIgnore]
            public Quaternion Rotation
            {
                get
                {
                    if (_rotationCached)
                        return _rotation;

                    _rotation = RotationAngle == 0
                        ? Quaternion.identity
                        : Quaternion.Euler(0, RotationAngle, 0);
                    _rotationCached = true;

                    return _rotation;
                }
            }

            [JsonIgnore]
            public string PermissionName;
        }

        private Configuration GetDefaultConfig() => new Configuration();

        #region Configuration Helpers

        private class BaseConfiguration
        {
            public string ToJson() => JsonConvert.SerializeObject(this);

            public Dictionary<string, object> ToDictionary() => JsonHelper.Deserialize(ToJson()) as Dictionary<string, object>;
        }

        private static class JsonHelper
        {
            public static object Deserialize(string json) => ToObject(JToken.Parse(json));

            private static object ToObject(JToken token)
            {
                switch (token.Type)
                {
                    case JTokenType.Object:
                        return token.Children<JProperty>()
                                    .ToDictionary(prop => prop.Name,
                                                  prop => ToObject(prop.Value));

                    case JTokenType.Array:
                        return token.Select(ToObject).ToList();

                    default:
                        return ((JValue)token).Value;
                }
            }
        }

        private bool MaybeUpdateConfig(BaseConfiguration config)
        {
            var currentWithDefaults = config.ToDictionary();
            var currentRaw = Config.ToDictionary(x => x.Key, x => x.Value);
            return MaybeUpdateConfigDict(currentWithDefaults, currentRaw);
        }

        private bool MaybeUpdateConfigDict(Dictionary<string, object> currentWithDefaults, Dictionary<string, object> currentRaw)
        {
            bool changed = false;

            foreach (var key in currentWithDefaults.Keys)
            {
                object currentRawValue;
                if (currentRaw.TryGetValue(key, out currentRawValue))
                {
                    var defaultDictValue = currentWithDefaults[key] as Dictionary<string, object>;
                    var currentDictValue = currentRawValue as Dictionary<string, object>;

                    if (defaultDictValue != null)
                    {
                        if (currentDictValue == null)
                        {
                            currentRaw[key] = currentWithDefaults[key];
                            changed = true;
                        }
                        else if (MaybeUpdateConfigDict(defaultDictValue, currentDictValue))
                            changed = true;
                    }
                }
                else
                {
                    currentRaw[key] = currentWithDefaults[key];
                    changed = true;
                }
            }

            return changed;
        }

        protected override void LoadDefaultConfig() => _config = GetDefaultConfig();

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null)
                {
                    throw new JsonException();
                }

                if (MaybeUpdateConfig(_config))
                {
                    LogWarning("Configuration appears to be outdated; updating and saving");
                    SaveConfig();
                }
            }
            catch (Exception e)
            {
                LogError(e.Message);
                LogWarning($"Configuration file {Name}.json is invalid; using defaults");
                LoadDefaultConfig();
            }
        }

        protected override void SaveConfig()
        {
            Log($"Configuration changes saved to {Name}.json");
            Config.WriteObject(_config, true);
        }

        #endregion

        #endregion
    }
}
