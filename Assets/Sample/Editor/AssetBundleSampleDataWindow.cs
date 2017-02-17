using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Sample
{
    public class AssetBundleSampleDataWindow : EditorWindow
    {
        private const string outputDir = "Test";
        private int testDataNum = 1000;
        private List<string> testVariants = new List<string>();

        [MenuItem("Samples/SampleWindow")]
        public static void Create()
        {
            EditorWindow.GetWindow<AssetBundleSampleDataWindow>();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Create Test Data");
            this.testDataNum = EditorGUILayout.IntField("Number of testdata",this.testDataNum);
            int newCount = Mathf.Max(0, EditorGUILayout.IntField("Number of variants", this.testVariants.Count));
            while (newCount < this.testVariants.Count)
                this.testVariants.RemoveAt(this.testVariants.Count - 1);
            while (newCount > this.testVariants.Count)
                this.testVariants.Add(null);
            for(int i = 0; i < this.testVariants.Count; i++)
            {
                this.testVariants[i] = EditorGUILayout.TextField(this.testVariants[i]);
            }

            if (GUILayout.Button("CreateTestData"))
            {
                if (this.testVariants.Count > 0)
                {
                    this.CreateDatasetWithVariants(this.testDataNum, this.testVariants.ToArray());
                }
                else
                {
                    this.CreateDataset(this.testDataNum);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("BuildAssetBundles");
            if (GUILayout.Button("BuildPipeline.BuildAssetBundles"))
            {
                BuildBundle(false);
            }
            if (GUILayout.Button(" UTJ.SeparateAssetBundleBuild.BuildAssetBundles"))
            {
                BuildBundle(true);
            }
        }

        private void BuildBundle(bool isSeparate)
        {
            BuildAssetBundleOptions assetBundleBuildOption = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle;
            BuildTarget targetPlatform = EditorUserBuildSettings.activeBuildTarget;

            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, true);
            }
            Directory.CreateDirectory(outputDir);

            float beforeAssetBundle = Time.realtimeSinceStartup;

            if (isSeparate)
            {
                UTJ.SeparatedAssetBundleBuild.BuildAssetBundles(outputDir, assetBundleBuildOption, targetPlatform);
            }
            else
            {
                BuildPipeline.BuildAssetBundles(outputDir, assetBundleBuildOption, targetPlatform);
            }
            float afterAssetBundle = Time.realtimeSinceStartup;

            UnityEngine.Debug.Log("BuildAssetBundle " + (afterAssetBundle - beforeAssetBundle));
            string title = "UTJ.SeparatedAssetBundleBuild.BuildAssetBundles\n";
            if (isSeparate)
            {
                title = "BuildPipeline.BuildAssetBundles \n";
            }
            EditorUtility.DisplayDialog(title , "It took " + (afterAssetBundle - beforeAssetBundle) + " sec.", "ok");
        }

        public void CreateDataset(int num)
        {
            string testDir = "Assets/Sample/TestBundles";

            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
            Directory.CreateDirectory(testDir);

            CreateTestAsset(testDir, num);

            AssetDatabase.Refresh();
            AssetDatabase.RemoveUnusedAssetBundleNames();

            SetAssetBundleName(testDir, num);
        }

        public void CreateDatasetWithVariants(int num, string[] variants)
        {
            string testDir = "Assets/Sample/TestBundles";

            if (Directory.Exists(testDir))
            {
                Directory.Delete(testDir, true);
            }
            Directory.CreateDirectory(testDir);

            for (int i = 0; i < variants.Length; i++)
            {
                string targetDirectory = Path.Combine(testDir, variants[i]);
                Directory.CreateDirectory(targetDirectory);
                CreateTestAsset(targetDirectory, num);
            }

            AssetDatabase.Refresh();
            AssetDatabase.RemoveUnusedAssetBundleNames();

            for (int i = 0; i < variants.Length; i++)
            {
                string targetDir = Path.Combine(testDir, variants[i]);
                SetAssetBundleName(targetDir, num, variants[i]);
            }
        }

        private void CreateTestAsset(string targerDirectory, int num)
        {
            for (int i = 0; i < num; ++i)
            {
                string targetPath = Path.Combine(targerDirectory, i + ".png");
                System.IO.File.Copy("Assets/Sample/origin.png", targetPath);
            }
        }

        private void SetAssetBundleName(string targerDirectory, int num, string variant = null)
        {
            for (int i = 0; i < num; ++i)
            {
                string targetPath = Path.Combine(targerDirectory, i + ".png");
                TextureImporter importer = TextureImporter.GetAtPath(targetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }
                importer.assetBundleName = "test_with_variant" + i;
                importer.assetBundleVariant = variant;
                importer.textureType = TextureImporterType.GUI;
                importer.mipmapEnabled = false;
            }
        }
    }
}
