using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

class BuldPostProcessor : BuildPreProcessorBase
{
     private static string assetBundleSourcesDirectory = "Assets/External/AssetBundles/Sources";
     private static string assetBundleResourcesDirectory = "Assets/External/AssetBundles/Resources";
     private static string assetsSourcesDirectory = "Assets/External/Assets/Sources";
     private static string assetsResourcesDirectory = "Assets/External/Assets/Resources";

    public override void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        AssetDatabase.StartAssetEditing();

        CopyAll(new DirectoryInfo(assetBundleSourcesDirectory), new DirectoryInfo(assetBundleResourcesDirectory));
        CopyAll(new DirectoryInfo(assetsSourcesDirectory), new DirectoryInfo(assetsResourcesDirectory));

        AssetDatabase.StopAssetEditing();
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