using System.Collections.Generic;
using UnityEngine;

namespace UTJ
{
    public class AllAssetBundleInformation
    {
        public List<AssetBundleInfoWithDepends> noDependList = new List<AssetBundleInfoWithDepends>();
        public Dictionary<int, List<AssetBundleInfoWithDepends>> dependGroup = new Dictionary<int, List<AssetBundleInfoWithDepends>>();
    }
}