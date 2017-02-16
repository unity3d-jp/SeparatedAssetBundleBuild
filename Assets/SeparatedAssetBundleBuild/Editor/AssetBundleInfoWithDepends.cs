using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UTJ
{
    public class AssetBundleInfoWithDepends
    {
        public string assetBundleName { private set; get; }
        public HashSet<string> dependsAssetBundleList;
        public HashSet<string> useThisAssetBundleList;
        public int groupId = -1;
        public bool isNonDepend
        {
            get
            {
                return (dependsAssetBundleList == null) && (useThisAssetBundleList == null);
            }
        }

        public AssetBundleInfoWithDepends(string name)
        {
            this.assetBundleName = name;

            string[] depends = AssetDatabase.GetAssetBundleDependencies(this.assetBundleName, true);
            if (depends == null)
            {
                return;
            }
            foreach (var depend in depends)
            {
                AddList(depend, ref this.dependsAssetBundleList);
            }
        }
        public void AddUseThisAssetBundleList(string name)
        {
            AddList(name, ref this.useThisAssetBundleList);
        }

        private void AddList(string assetBundle, ref HashSet<string> dataset)
        {
            if (string.IsNullOrEmpty(assetBundle))
            {
                return;
            }
            if (dataset == null)
            {
                dataset = new HashSet<string>();
            }
            if (!dataset.Contains(assetBundle))
            {
                dataset.Add(assetBundle);
            }
        }
    }
}