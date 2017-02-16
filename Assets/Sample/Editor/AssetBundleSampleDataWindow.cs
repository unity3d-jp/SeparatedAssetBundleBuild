using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace Sample
{
    public class AssetBundleSampleDataWindow : EditorWindow
    {
        private const string outputDir = "Test";
        private int testDataNum = 1000;

        [MenuItem("Samples/SampleWindow")]
        public static void Create()
        {
            EditorWindow.GetWindow<AssetBundleSampleDataWindow>();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Create Test Data");
            this.testDataNum = EditorGUILayout.IntField("Number of testdata",this.testDataNum);
            if (GUILayout.Button("CreateTestData"))
            {
                this.CreateDataset(this.testDataNum);
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

            for (int i = 0; i < num; ++i)
            {
                string targetPath = Path.Combine(testDir, i + ".png");
                System.IO.File.Copy("Assets/Sample/origin.png", targetPath);
            }
            AssetDatabase.Refresh();
            AssetDatabase.RemoveUnusedAssetBundleNames();

            for (int i = 0; i < num; ++i)
            {
                string targetPath = Path.Combine(testDir, i + ".png");
                TextureImporter importer = TextureImporter.GetAtPath(targetPath) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }
                importer.assetBundleName = "test" + i;
                importer.textureType = TextureImporterType.GUI;
                importer.mipmapEnabled = false;
            }
        }
    }
}
