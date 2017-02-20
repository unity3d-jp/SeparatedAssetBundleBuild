//#define ASSET_BUNDLE_GROUPING_DEBUG
//#define ASSET_BUNDLE_BUILD_TIME_CHECK

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;



namespace UTJ
{
    /// <summary>
    /// call BuildPipeline.BuildAssetBundles separately.
    /// because of Unity5.5 regression.
    /// </summary>
    public class SeparatedAssetBundleBuild
    {

        // The number of batchingAssetBundleBuilds.
        private const int BulkConvertNum = 300;
        public static List<string> ReservedVariants = new List<string>(0);

        public static AssetBundleBuildsManifest BuildAssetBundles(string outputDir, BuildAssetBundleOptions buildOption, BuildTarget targetPlatform)
        {
#if ASSET_BUNDLE_BUILD_TIME_CHECK
            float startTime = Time.realtimeSinceStartup;
#endif

            AssetBundleBuildsManifest manifest = new AssetBundleBuildsManifest();

            AssetBundleManifest unityManifest = null;
            // append
            AllAssetBundleInformation allAssetBundleInfo = GetAllAssetBundleInfoList();
            int allAssetBundleCount = AssetDatabase.GetAllAssetBundleNames().Length;
            //
            List<AssetBundleBuild> buildTargetBuffer = new List<AssetBundleBuild>();

#if ASSET_BUNDLE_BUILD_TIME_CHECK
            float groupingComplete = Time.realtimeSinceStartup;
#endif


            /// Build NoDependAssetBundles
            int idx = 0;
            foreach (var noDependAssetBundle in allAssetBundleInfo.noDependList)
            {
                string assetBundleName = noDependAssetBundle.assetBundleName;
                buildTargetBuffer.Add(CreateAssetBundleBuildFromAssetBundleName(assetBundleName));
                if (buildTargetBuffer.Count > BulkConvertNum)
                {
#if ASSET_BUNDLE_GROUPING_DEBUG
                    DebugPrintBuildPipeline(buildTargetBuffer);
#endif
                    unityManifest = BuildPipeline.BuildAssetBundles(outputDir, buildTargetBuffer.ToArray(), buildOption, targetPlatform);
                    manifest.AddUnityManifest(unityManifest);
                    buildTargetBuffer.Clear();
                }
                ++idx;
                EditorUtility.DisplayProgressBar("process no dependence bundles", assetBundleName, (float)idx / (float)allAssetBundleCount);
            }
            if (buildTargetBuffer.Count > 0)
            {
#if ASSET_BUNDLE_GROUPING_DEBUG
                DebugPrintBuildPipeline(buildTargetBuffer);
#endif
                unityManifest = BuildPipeline.BuildAssetBundles(outputDir, buildTargetBuffer.ToArray(), buildOption, targetPlatform);
                manifest.AddUnityManifest(unityManifest);
                buildTargetBuffer.Clear();
            }

            // group set
            var dependGroupList = new List<List<AssetBundleInfoWithDepends>>(allAssetBundleInfo.dependGroup.Values);
            int length = dependGroupList.Count;

            for (int i = 0; i < length; ++i)
            {
                string assetBundleName = "";
                foreach (var assetBundle in dependGroupList[i])
                {
                    assetBundleName = assetBundle.assetBundleName;
                    buildTargetBuffer.Add(CreateAssetBundleBuildFromAssetBundleName(assetBundleName));
                    ++idx;
                }
                bool buildFlag = false;
                buildFlag |= (buildTargetBuffer.Count > BulkConvertNum);
                buildFlag |= (i == length - 1);
                if (i < length - 1)
                {
                    buildFlag |= (dependGroupList[i + 1].Count > BulkConvertNum);
                }

                if (buildFlag)
                {
#if ASSET_BUNDLE_GROUPING_DEBUG
                    DebugPrintBuildPipeline(buildTargetBuffer);
#endif
                    unityManifest = BuildPipeline.BuildAssetBundles(outputDir, buildTargetBuffer.ToArray(), buildOption, targetPlatform);
                    manifest.AddUnityManifest(unityManifest);
                    buildTargetBuffer.Clear();
                }
                EditorUtility.DisplayProgressBar("process dependence", "group " + i + " " + assetBundleName, (float)idx / (float)allAssetBundleCount);
            }
            EditorUtility.ClearProgressBar();
#if ASSET_BUNDLE_BUILD_TIME_CHECK
            float buildCompleteTime = Time.realtimeSinceStartup;

            UnityEngine.Debug.Log("All BuildTime:" + (buildCompleteTime - startTime) + "sec\n" +
                "GroupingTime:" + (groupingComplete - startTime) + "sec\n" +
                "AssetBundleBuild:" + (buildCompleteTime - groupingComplete) + "sec\n");
#endif
            return manifest;
        }
#if ASSET_BUNDLE_GROUPING_DEBUG
        private static void DebugPrintBuildPipeline(List<AssetBundleBuild> buildTargetBuffer)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("BuildPipeline.BuildAssetBundles ").Append(buildTargetBuffer.Count).Append("\n");
            foreach (var target in buildTargetBuffer)
            {
                sb.Append(target.assetBundleName).Append("\n");
            }

            UnityEngine.Debug.Log(sb.ToString());
        }
#endif

        private static AllAssetBundleInformation GetAllAssetBundleInfoList()
        {
            AllAssetBundleInformation allAsssetBundleInfo = new AllAssetBundleInformation();
            var assetBundleNames = AssetDatabase.GetAllAssetBundleNames();

            Dictionary<string, AssetBundleInfoWithDepends> infoDictionary = new Dictionary<string, AssetBundleInfoWithDepends>(assetBundleNames.Length);
            // Check Depencencies
            int idx = 0;
            foreach (var assetBundleName in assetBundleNames)
            {
                infoDictionary.Add(assetBundleName, new AssetBundleInfoWithDepends(assetBundleName));
                EditorUtility.DisplayProgressBar("Check the dependencies", assetBundleName, (float)idx / (float)assetBundleNames.Length * 2.0f);
                ++idx;
            }
            foreach (var dependInfo in infoDictionary.Values)
            {
                if (dependInfo.dependsAssetBundleList == null)
                {
                    ++idx;
                    continue;
                }
                foreach (var depend in dependInfo.dependsAssetBundleList)
                {
                    infoDictionary[depend].AddUseThisAssetBundleList(dependInfo.assetBundleName);
                }
                EditorUtility.DisplayProgressBar("Check the dependencies", dependInfo.assetBundleName, (float)idx / (float)assetBundleNames.Length * 2.0f);
                ++idx;
            }
            // remove no depends
            List<AssetBundleInfoWithDepends> dataList = new List<AssetBundleInfoWithDepends>(infoDictionary.Values);
            foreach (var data in dataList)
            {
                if (data.isNonDepend)
                {
                    infoDictionary.Remove(data.assetBundleName);
                    allAsssetBundleInfo.noDependList.Add(data);
                }
            }
#if ASSET_BUNDLE_GROUPING_DEBUG
            UnityEngine.Debug.Log("dependBundles : " + infoDictionary.Count + ":: noDependBundles" + (dataList.Count - infoDictionary.Count));
#endif
            // Grouping
            EditorUtility.DisplayProgressBar("Grouping...", "", 0.0f);

            int leftAssetBundles = infoDictionary.Count;
            int currentGroupId = 0;
            HashSet<string> groupHashSet = new HashSet<string>();
            while (infoDictionary.Count > 0)
            {
                var enumrator = infoDictionary.Values.GetEnumerator();
                enumrator.MoveNext();
                AssetBundleInfoWithDepends currentValue = enumrator.Current;
                if (currentValue == null)
                {
                    continue;
                }
                currentValue.groupId = currentGroupId;
                groupHashSet.Clear();
                groupHashSet.Add(currentValue.assetBundleName);



                while (currentValue != null)
                {
                    currentValue.groupId = currentGroupId;
                    if (currentValue.dependsAssetBundleList != null)
                    {
                        foreach (var depend in currentValue.dependsAssetBundleList)
                        {
                            groupHashSet.Add(depend);
                        }
                    }
                    if (currentValue.useThisAssetBundleList != null)
                    {
                        foreach (var used in currentValue.useThisAssetBundleList)
                        {
                            groupHashSet.Add(used);
                        }
                    }
                    bool flag = false;
                    currentValue = null;
                    foreach (string assetBundle in groupHashSet)
                    {
                        if (infoDictionary.TryGetValue(assetBundle, out currentValue))
                        {
                            if (currentValue != null && currentValue.groupId < 0)
                            {
                                flag = true;
                                break;
                            }
                        }
                        else
                        {
                            UnityEngine.Debug.LogError("Something wrong " + assetBundle + " @ " + currentGroupId);
                        }
                    }
                    if (!flag)
                    {
                        currentValue = null;
                    }
                }
                // remove from dictionary
#if ASSET_BUNDLE_GROUPING_DEBUG
                string debugPrint = "current Group " + currentGroupId + " num:" + groupHashSet.Count;
#endif
                List<AssetBundleInfoWithDepends> groupBundleList = new List<AssetBundleInfoWithDepends>(groupHashSet.Count);
                foreach (string assetBundleName in groupHashSet)
                {
#if ASSET_BUNDLE_GROUPING_DEBUG
                    debugPrint += "\n  " + assetBundleName;
#endif
                    AssetBundleInfoWithDepends assetBundleInfo;
                    if (infoDictionary.TryGetValue(assetBundleName, out assetBundleInfo))
                    {
                        groupBundleList.Add(assetBundleInfo);
                        infoDictionary.Remove(assetBundleName);
                    }
                }
                allAsssetBundleInfo.dependGroup.Add(currentGroupId, groupBundleList);

#if ASSET_BUNDLE_GROUPING_DEBUG
                UnityEngine.Debug.Log(debugPrint);
#endif

                ++currentGroupId;
                EditorUtility.DisplayProgressBar("Grouping...", "Group " + currentGroupId, (-infoDictionary.Count + leftAssetBundles) / (float)leftAssetBundles);
            }

            EditorUtility.ClearProgressBar();

            return allAsssetBundleInfo;
        }

        private static AssetBundleBuild CreateAssetBundleBuildFromAssetBundleName(string assetBundleName)
        {
            AssetBundleBuild build = new AssetBundleBuild();

            string bundleName;
            string variant;
            GetAssetBundleNameAndVariantByReservedVariants(assetBundleName, out bundleName, out variant);

            build.assetBundleName = bundleName;
            build.assetBundleVariant = variant;
            build.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            return build;
        }

        private static void GetAssetBundleNameAndVariantByReservedVariants(string originAssetBundleName, out string assetBundleName, out string assetBundleVariant)
        {
            assetBundleName = originAssetBundleName;
            assetBundleVariant = "";

            string extension = Path.GetExtension(assetBundleName);
            if (!string.IsNullOrEmpty(extension))
            {
                extension = extension.Substring(1);
                if (ReservedVariants.Contains(extension))
                {
                    assetBundleName = Path.Combine(Path.GetDirectoryName(assetBundleName), Path.GetFileNameWithoutExtension(assetBundleName));
                    assetBundleVariant = extension;
                }
            }
        }

    }

}