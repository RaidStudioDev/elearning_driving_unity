using System.IO;
using UnityEditor;
using UnityEngine;

class BuildPreProcessor : BuildPreProcessorBase
{
     private static string assetBundleSourcesDirectory = "Assets/External/AssetBundles/Sources";
     private static string assetBundleResourcesDirectory = "Assets/External/AssetBundles/Resources";
     private static string assetsSourcesDirectory = "Assets/External/Assets/Sources";
     private static string assetsResourcesDirectory = "Assets/External/Assets/Resources";

     public override void OnPreprocessBuild(BuildTarget target, string path) 
     {       
        if (BuildTarget.StandaloneWindows64 == target 
            || BuildTarget.StandaloneOSX == target 
            || BuildTarget.iOS == target 
            || BuildTarget.Android == target )
        {
            AssetDatabase.StartAssetEditing();

            //copy assets and assetbundle sources to resources directory
            CopyAll(new DirectoryInfo(assetBundleSourcesDirectory), new DirectoryInfo(assetBundleResourcesDirectory));
            CopyAll(new DirectoryInfo(assetsSourcesDirectory), new DirectoryInfo(assetsResourcesDirectory));

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
        else
        {
            string assetBundleCompiledDirectory = "Assets/External/AssetBundles/Compiled";

            //copy assetbundles to build directory
            string assetBundlesBuildDirectory = "../Build/AssetBundles";
            CopyAll(new DirectoryInfo(assetBundleCompiledDirectory), new DirectoryInfo(assetBundlesBuildDirectory));
           
            //copy assets sources to build directory
            string assetsSourcesBuildDirectory = "../Build/DynamicAssets";
            CopyAll(new DirectoryInfo(assetsSourcesDirectory), new DirectoryInfo(assetsSourcesBuildDirectory));

            AssetDatabase.StartAssetEditing();

            //clear assetbundles resources directory
            foreach (FileInfo file in new DirectoryInfo(assetBundleResourcesDirectory).GetFiles()) file.Delete(); 
            foreach (DirectoryInfo dir in new DirectoryInfo(assetBundleResourcesDirectory).GetDirectories()) dir.Delete(true); 

            //clear assets resources directory
            foreach (FileInfo file in new DirectoryInfo(assetsResourcesDirectory).GetFiles()) file.Delete(); 
            foreach (DirectoryInfo dir in new DirectoryInfo(assetsResourcesDirectory).GetDirectories()) dir.Delete(true);

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
     }
     
    private static void Copy(string sourceDirectory, string targetDirectory)
    {
        DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
        DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

        CopyAll(diSource, diTarget);
    }

    private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        Directory.CreateDirectory(target.FullName);

        // Copy each file into the new directory.
        foreach (FileInfo fi in source.GetFiles())
        {
            fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }
}