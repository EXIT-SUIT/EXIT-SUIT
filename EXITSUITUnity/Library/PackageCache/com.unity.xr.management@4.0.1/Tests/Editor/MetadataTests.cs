using NUnit.Framework;

using System;
using System.Collections;
using System.Linq;

using UnityEngine.TestTools;
using UnityEditor.XR.Management.Metadata;

using Unity.XR.Management.TestPackage;
using Unity.XR.Management.TestPackage.Editor;

namespace UnityEditor.XR.Management.Tests
{
    class MetadataTests
    {
        [SetUp]
        public void Setup()
        {
            AssetDatabase.DeleteAsset("Assets/XR");
            XRPackageInitializationBootstrap.BeginPackageInitialization();

            TestPackage pkg = new TestPackage();
            XRPackageMetadataStore.AddPluginPackage(pkg);
            XRPackageInitializationBootstrap.InitPackage(pkg);
        }

        [TearDown]
        public void Teardown()
        {
            AssetDatabase.DeleteAsset("Assets/XR");
        }

        [UnityTest]
        public IEnumerator CreateSettingsAndLoaders()
        {
            int frameCount = 0;
            string[] assets = new string[0]{};


            while (frameCount < 60)
            {
                yield return null;
                frameCount++;
            }

            yield return null;

            string[] assetTypes = new string[]{"TestLoaderOne", "TestLoaderTwo", "TestLoaderThree","TestSettings"};
            foreach (var assetType in assetTypes)
            {
                assets = AssetDatabase.FindAssets($"t:{assetType}");
                Assert.IsTrue(assets.Length == 1);
            }
        }

        [Test, Sequential]
        public void FilteringMetadataByBuildTargetGroup(
            [Values(BuildTargetGroup.Standalone, BuildTargetGroup.Android, BuildTargetGroup.Unknown, BuildTargetGroup.WebGL)]BuildTargetGroup buildTargetGroup,
            [Values("Test Loader One", "Test Loader Two", "Test Loader Three", "")]string expectedType)
        {
            var loaders = XRPackageMetadataStore.GetLoadersForBuildTarget(buildTargetGroup);
            Assert.IsTrue((loaders.Count > 0 && !String.IsNullOrEmpty(expectedType)) ||
                            (loaders.Count <= 0 && String.IsNullOrEmpty(expectedType)));

            if (!String.IsNullOrEmpty(expectedType))
            {
                var loaderNames = from lm in loaders where String.Compare(lm.loaderName, expectedType, false) == 0 select lm.loaderName;
                Assert.IsTrue(loaderNames.Any());
                Assert.IsTrue(loaderNames.Count() == 1);
            }
        }

        [UnityTest]
        public IEnumerator AccessSettings()
        {
            int frameCount = 0;
            string[] assets = new string[0] { };

            TestPackage pkg = new TestPackage();
            XRPackageMetadataStore.AddPluginPackage(pkg);
            XRPackageInitializationBootstrap.InitPackage(pkg);

            while (frameCount < 60)
            {
                yield return null;
                frameCount++;
            }

            yield return null;

            var metadata = XRPackageMetadataStore.GetMetadataForPackage("com.unity.xr.testpackage");
            Assert.IsNotNull(metadata);

            assets = AssetDatabase.FindAssets($"t:{metadata.settingsType}");
            Assert.IsTrue(assets.Length == 1);
            var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);

            var instance = AssetDatabase.LoadAssetAtPath(assetPath, typeof(TestSettings));
            Assert.IsNotNull(instance);
        }

    }
}
