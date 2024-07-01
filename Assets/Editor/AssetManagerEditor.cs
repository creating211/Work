using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public enum AssetBundleCompresionPattern
{ 
    LZMA,
    LZ4,
    None
}

/// <summary>
/// 所有Editor目录下的C#脚本都不会被打包到可执行文件中
/// </summary>

public class AssetManagerEditor : MonoBehaviour
{
    public static string AssetManagerVersion = "1.0.0";

    /// <summary>
    /// 编辑器模拟下，不进行打包
    /// 本地模式下，打包到StreamingAssets
    /// 远端模式，打包到任意远端路径，在该示例中为persistenceDataPath
    /// </summary>
    public static AssetBundlePattern BuilidingPattern;

    public static AssetBundleCompresionPattern CompressionPattern;

    /// <summary>
    /// 需要打包的文件夹
    /// </summary>
    public static DefaultAsset AssetBundleDirectory;

    //放在资源加载类helloworld中public static string MainAssetBundleName = "SampleAssetBundle";

    //public static string AssetBundleOutputPath = Path.Combine(Application.persistentDataPath, MainAssetBundleName);

    //当使用了AssetBundlePattern方式时，默认情况下打包路径是没有值的
    public static string AssetBundleOutputPath ;

    //public const string AssetManagerName = nameof(AssetManager);
    /// <summary>
    /// 通过MenuItem特性，声明Editor顶部菜单栏选项
    /// </summary>
    [MenuItem(nameof(AssetManagerEditor)+"/"+nameof(BuildAssetBundle))]
    static void BuildAssetBundle()

    {
        CheckBuildOutputPath();
        //如果用persistenceDataPath将打包在工程外的文件夹可以用
        //Debug.Log(Application.persistentDataPath);查看打包在哪个路径下面
        //string outputPath = Path.Combine(Application.persistentDataPath, "Bundles");

        if(!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }

        //不同平台之间的Asset Bundle不可以通用
        //该方法会打包工程内所有配置了包名的AB包
        //option为none时使用LZMA压缩
        //UncompressedAssetBundle不进行压缩
        //ChunkBasedCompression进行LZ4进行块压缩
        //打包路径，压缩放法，什么系统activeBuildTarget可以自适应

        /*
        BuildAssetBundleOptions option = new BuildAssetBundleOptions();
        switch (CompressionPattern)
        {
            case AssetBundleCompresionPattern.LZMA:
                option = BuildAssetBundleOptions.None;
                break;
            case AssetBundleCompresionPattern.LZ4:
                option = BuildAssetBundleOptions.ChunkBasedCompression;
                break;
            case AssetBundleCompresionPattern.None:
                option = BuildAssetBundleOptions.UncompressedAssetBundle;
                break;
        }
        */

        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, CheckCompressionPattern(), EditorUserBuildSettings.activeBuildTarget);
        Debug.Log("AB包打包完成");
        //Debug.Log(Application.persistentDataPath);
    }

    static BuildAssetBundleOptions CheckCompressionPattern()
    {
        BuildAssetBundleOptions option = new BuildAssetBundleOptions();
        switch (CompressionPattern)
        {
            case AssetBundleCompresionPattern.LZMA:
                option = BuildAssetBundleOptions.None;
                break;
            case AssetBundleCompresionPattern.LZ4:
                option = BuildAssetBundleOptions.ChunkBasedCompression;
                break;
            case AssetBundleCompresionPattern.None:
                option = BuildAssetBundleOptions.UncompressedAssetBundle;
                break;
        }
        return option;
    }

    [MenuItem(nameof(AssetManagerEditor)+"/"+nameof(OpenAssetManagerWindow))]
    static void OpenAssetManagerWindow()
    {
        /*
        Rect windowRect = new Rect(0, 0, 500, 500);
        AssetManagerEditorWindow window =(AssetManagerEditorWindow) EditorWindow.GetWindowWithRect(typeof(AssetManagerEditorWindow), windowRect, true,nameof(AssetManagerEditor));
        */
        //或者直接用getwindow方法,可以自定义大小
        AssetManagerEditorWindow window = (AssetManagerEditorWindow)EditorWindow.GetWindow(typeof(AssetManagerEditorWindow), true, nameof(AssetManagerEditor));
    }

    static void CheckBuildOutputPath()
    {
        switch(BuilidingPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                AssetBundleOutputPath= Path.Combine(Application.streamingAssetsPath,HelloWorld.MainAssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                AssetBundleOutputPath = Path.Combine(Application.persistentDataPath,HelloWorld.MainAssetBundleName);
                break;
        }
    }

    /// <summary>
    /// 打包指定文件夹下的所有资源为Asset Bundle
    /// </summary>
    public static void BuildAssetBundleFromDirectory()
    {
        CheckBuildOutputPath();
        if (AssetBundleDirectory==null)
        {
            Debug.Log("打包目录不存在");
            return;
        }
        string directoryPath = AssetDatabase.GetAssetPath((AssetManagerEditor.AssetBundleDirectory));
        string[] assetPath=AssetManagerEditor.FindAllAssetNameFromDirectory(directoryPath).ToArray();

        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[1];

        //将要打包的具体名，而非主包名
        assetBundleBuilds[0].assetBundleName = HelloWorld.ObjectAssetBundleName;

        //这里虽然名为Name，实际上需要资源在工程下的路径
        assetBundleBuilds[0].assetNames = assetPath;

        if(string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("输出路径为空");
            return;
        }
        else if(!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }
        
        //Unity中inspector面板配置的assetbundle信息，其实就是一个Assetbundle结构体
        //unity只不过是遍历了所有文件夹，并把我们在文件夹中配置的AssetBundleBuild收集起来打包
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckCompressionPattern(), BuildTarget.StandaloneWindows);

        Debug.Log(AssetBundleOutputPath);

        //刷新project界面，如果不是打包到工程内则不需要执行
        AssetDatabase.Refresh();

    }
    public static List<string> FindAllAssetNameFromDirectory(string directoryPath)
    {
        List<string> assetPaths = new List<string>();

        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            Debug.Log("文件夹路径不存在");
            return null;
        }

        //System.IO类方法，也就是Windows自带的对文件夹进行操作的类
        //System.IO下的类，只能在Windows或者pc平台上读写文件，在移动端不适用
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        //获取该目录下所有文件信息
        //Directory文件夹不适于File类型，所以这里不会获取子文件夹
        FileInfo[] fileInfos = directoryInfo.GetFiles();

        //所有的非元数据文件路径都添加到列表中用于打包这些文件
        foreach(FileInfo info in fileInfos)
        {
            if(info.Extension.Contains(".meta"))
            {
                continue;
            }
            //Asset打包只需要文件名
            string assetPath = Path.Combine(directoryPath,info.Name);
            assetPaths.Add(assetPath);
            Debug.Log(assetPath);
        }

        return assetPaths;
    }
}
