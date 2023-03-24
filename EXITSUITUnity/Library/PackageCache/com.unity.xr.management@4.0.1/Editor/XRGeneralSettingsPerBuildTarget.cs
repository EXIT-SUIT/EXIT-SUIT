using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.XR.Management;

using UnityEditor.XR.Management.Metadata;

namespace UnityEditor.XR.Management
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
   /// <summary>Container class that holds general settings for each build target group installed in Unity.</summary>
   public class XRGeneralSettingsPerBuildTarget : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<BuildTargetGroup> Keys = new List<BuildTargetGroup>();

        [SerializeField]
        List<XRGeneralSettings> Values = new List<XRGeneralSettings>();
        Dictionary<BuildTargetGroup, XRGeneralSettings> Settings = new Dictionary<BuildTargetGroup, XRGeneralSettings>();


#if UNITY_EDITOR
        // Simple class to give us updates when the asset database changes.
        class AssetCallbacks : AssetPostprocessor
        {
            static bool m_Upgrade = true;
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (m_Upgrade)
                {
                    m_Upgrade = false;
                    BeginUpgradeSettings();
                }
            }

            static void BeginUpgradeSettings()
            {
                string searchText = "t:XRGeneralSettings";
                string[] assets = AssetDatabase.FindAssets(searchText);
                if (assets.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    XRGeneralSettingsUpgrade.UpgradeSettingsToPerBuildTarget(path);
                }
            }
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        }

        void OnEnable()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            foreach (var setting in Settings.Values)
            {
                var assignedSettings = setting.AssignedSettings;
                if (assignedSettings == null)
                    continue;

                var filteredLoaders = from ldr in assignedSettings.activeLoaders where ldr != null select ldr;
                assignedSettings.TrySetLoaders(filteredLoaders.ToList<XRLoader>());
            }
            XRGeneralSettings.Instance = XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
        }

        static void PlayModeStateChanged(PlayModeStateChange state)
        {
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
                return;

            XRGeneralSettings instance = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (instance == null || !instance.InitManagerOnStart)
                return;

            instance.InternalPlayModeStateChanged(state);
        }

        internal static bool ContainsLoaderForAnyBuildTarget(string loaderTypeName)
        {

            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
                return false;

            foreach (var settings in buildTargetSettings.Settings.Values)
            {
                if (XRPackageMetadataStore.IsLoaderAssigned(settings.Manager, loaderTypeName))
                    return true;
            }

            return false;
        }
#endif

        /// <summary>Set specific settings for a given build target.</summary>
        ///
        /// <param name="targetGroup">An enum specifying which platform group this build is for.</param>
        /// <param name="settings">An instance of <see cref="XRGeneralSettings"/> to assign for the given key.</param>
        public void SetSettingsForBuildTarget(BuildTargetGroup targetGroup, XRGeneralSettings settings)
        {
            // Ensures the editor's "runtime instance" is the most current for standalone settings
            if (targetGroup == BuildTargetGroup.Standalone)
                XRGeneralSettings.Instance = settings;
            Settings[targetGroup] = settings;
        }

        /// <summary>Get specific settings for a given build target.</summary>
        /// <param name="targetGroup">An enum specifying which platform group this build is for.</param>
        /// <returns>The instance of <see cref="XRGeneralSettings"/> assigned to the key, or null if not.</returns>
        public XRGeneralSettings SettingsForBuildTarget(BuildTargetGroup targetGroup)
        {
            XRGeneralSettings ret = null;
            Settings.TryGetValue(targetGroup, out ret);
            return ret;
        }

        /// <summary>Serialization override.</summary>
        public void OnBeforeSerialize()
        {
            Keys.Clear();
            Values.Clear();

            foreach (var kv in Settings)
            {
                Keys.Add(kv.Key);
                Values.Add(kv.Value);
            }
        }

        /// <summary>Serialization override.</summary>
        public void OnAfterDeserialize()
        {
            Settings = new Dictionary<BuildTargetGroup, XRGeneralSettings>();
            for (int i = 0; i < Math.Min(Keys.Count, Values.Count); i++)
            {
                Settings.Add(Keys[i], Values[i]);
            }
        }

        /// <summary>Given a build target, get the general settings container assigned to it.</summary>
        /// <param name="targetGroup">An enum specifying which platform group this build is for.</param>
        /// <returns>The instance of <see cref="XRGeneralSettings"/> assigned to the key, or null if not.</returns>
        public static XRGeneralSettings XRGeneralSettingsForBuildTarget(BuildTargetGroup targetGroup)
        {
            XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            if (buildTargetSettings == null)
                return null;

            return buildTargetSettings.SettingsForBuildTarget(targetGroup);
        }
    }
}
