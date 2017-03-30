using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UTJ
{
    /// <summary>
    /// AssetBundleManifest
    /// </summary>
    public class AssetBundleBuildsManifest
    {
        private List<string> allAssetBundles = new List<string>();
        private Dictionary<string, string[]> allDependencies = new Dictionary<string, string[]>();
        private Dictionary<string, Hash128> allHash = new Dictionary<string, Hash128>();

        public string[] GetAllAssetBundles()
        {
            return allAssetBundles.ToArray();
        }
        public string[] GetAllDependencies(string name)
        {
            string[] val = null;
            allDependencies.TryGetValue(name, out val);
            return val;
        }
        public Hash128 GetAssetBundleHash(string name)
        {
            Hash128 hash = new Hash128();
            allHash.TryGetValue(name, out hash);
            return hash;
        }

        public void AddUnityManifest(UnityEngine.AssetBundleManifest manifest)
        {
            if (manifest == null) { return; }
            string[] inOriginList = manifest.GetAllAssetBundles();
            foreach (var origin in inOriginList)
            {
                allAssetBundles.Add(origin);
                allDependencies.Add(origin, manifest.GetAllDependencies(origin));
                allHash.Add(origin, manifest.GetAssetBundleHash(origin));
            }
        }
    }
}