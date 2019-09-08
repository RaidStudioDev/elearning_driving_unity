using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{
    private static string assetBundleSourcesDirectory = "Assets/External/AssetBundles/Sources";
    private static string assetBundleResourcesDirectory = "Assets/External/AssetBundles/Resources";
    private static string assetsSourcesDirectory = "Assets/External/Assets/Sources";
    private static string assetsResourcesDirectory = "Assets/External/Assets/Resources";

    [MenuItem("Assets/Build AssetBundles")]
	static void BuildAllAssetBundles()
	{
		string assetBundleDirectory = "Assets/External/AssetBundles/Compiled";
		if (!Directory.Exists(assetBundleDirectory)) Directory.CreateDirectory(assetBundleDirectory);
		
        AssetDatabase.StartAssetEditing();

        //clear assetbundles resources directory
        foreach (FileInfo file in new DirectoryInfo(assetBundleResourcesDirectory).GetFiles()) file.Delete();
        foreach (DirectoryInfo dir in new DirectoryInfo(assetBundleResourcesDirectory).GetDirectories()) dir.Delete(true);

        //clear assets resources directory
        foreach (FileInfo file in new DirectoryInfo(assetsResourcesDirectory).GetFiles()) file.Delete();
        foreach (DirectoryInfo dir in new DirectoryInfo(assetsResourcesDirectory).GetDirectories()) dir.Delete(true);

        AssetDatabase.StopAssetEditing();

        //build assetbundles
        string assetBundleCompiledDirectory = "Assets/External/AssetBundles/Compiled";
        
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle, EditorUserBuildSettings.activeBuildTarget);
        
        AssetDatabase.StartAssetEditing();

        CopyAll(new DirectoryInfo(assetBundleSourcesDirectory), new DirectoryInfo(assetBundleResourcesDirectory));
        CopyAll(new DirectoryInfo(assetsSourcesDirectory), new DirectoryInfo(assetsResourcesDirectory));

        AssetDatabase.StopAssetEditing();

        //copy assetbundles to build directory
        string assetBundlesBuildDirectory = "Build/AssetBundles";
        CopyAll(new DirectoryInfo(assetBundleCompiledDirectory), new DirectoryInfo(assetBundlesBuildDirectory));

        //copy assets sources to build directory
        string assetsSourcesBuildDirectory = "Build/DynamicAssets";
        CopyAll(new DirectoryInfo(assetsSourcesDirectory), new DirectoryInfo(assetsSourcesBuildDirectory));

        // **** RAFAEL: Copy to localhost 
        // CopyAll(new DirectoryInfo(assetBundleCompiledDirectory), new DirectoryInfo(@"D:\Apps\wamp64\www\apps\switchback\AssetBundles"));
        // CopyAll(new DirectoryInfo(assetsSourcesDirectory), new DirectoryInfo(@"D:\Apps\wamp64\www\apps\switchback\DynamicAssets"));
    }

    [MenuItem("Assets/Copy Sources To Resources")]
    static void CopySourcesToResources()
    {
        string assetBundleSourcesDirectory = "Assets/External/AssetBundles/Sources";
        string assetBundleResourcesDirectory = "Assets/External/AssetBundles/Resources";
        string assetSourcesDirectory = "Assets/External/Assets/Sources";
        string assetResourcesDirectory = "Assets/External/Assets/Resources";

        if (!Directory.Exists(assetBundleSourcesDirectory) 
            || !Directory.Exists(assetSourcesDirectory))
        {
            Debug.Log(assetBundleSourcesDirectory + " does not exist. Exiting...");
            return;
        }

        CopyAll(new DirectoryInfo(assetBundleSourcesDirectory), new DirectoryInfo(assetBundleResourcesDirectory));
        CopyAll(new DirectoryInfo(assetSourcesDirectory), new DirectoryInfo(assetResourcesDirectory));

        EditorAlertPopUp window = ScriptableObject.CreateInstance<EditorAlertPopUp>();
        window.position = new Rect((Screen.currentResolution.width / 2) - 125, (Screen.currentResolution.height / 2) - 75, 250, 150);
        window.ShowPopup();
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