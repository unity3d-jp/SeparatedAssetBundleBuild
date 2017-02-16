# SeparatedAssetBundleBuild
Workaround for long time to build many AssetBundles.

## 発生している問題について
Unity 5.5にて、ビルド対象のAssetBundle数が増えれば増えるほど、ビルド時間が指数的に長くなるという問題が起きています。<br />

図1.AssetBundle数とビルド時間の関係<br />

![Alt text](/doc/img/AssetBundleBuildTime.png)

Issue Tracker:<br />
https://issuetracker.unity3d.com/issues/drastically-longer-asset-bundle-building-time-when-building-multiple-small-asset-bundles<br />

## このプロジェクトについて
このプロジェクトは、"BuildPipeline.BuildAssetBundles"を可能な限り分割して呼び出す事でビルド時間を短縮するためのプロジェクトです。

### 使用方法
1). SeparatedAssetBundleBuild.unitypackage をインポートします。<br />
2). プロジェクト内にある "BuildPipeline.BuildAssetBundles" を "UTJ.SeparatedAssetBundleBuild.BuildAssetBundles" に置き換えてください。<br />


### サンプルについて
テスト用にサンプルを用意しました。<br />
Menuの"Sample/SampleWindow"を呼び出してください。<br />

![Alt text](/doc/img/SampleWindow.png) <br />
<br />
1).テストに使用するアセットバンドル数をセットします。<br />
2).テスト用のデータを作成します。<br />
3).従来のやり方でAssetBundleを作成します。<br />
4).今回用意した方法で、AssetBundleを作成します。<br />


