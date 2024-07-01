using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 这个类只用于收集Editor环境下存在的Package信息而非打包后的Package信息
/// </summary>

[Serializable]
public class PackageInfoEditor
{
    /// <summary>
    /// 当前包的名称，可以由开发者在编辑器窗口自由定义
    /// </summary>
    public string PackageName;

    /// <summary>
    /// 归属于当前包中的资源列表，可以由开发者在编辑器窗口中自由定义
    /// </summary>
    public List<UnityEngine.Object> AssetList = new List<UnityEngine.Object>();
}


[CreateAssetMenu(fileName = "AssetManagerConfig", menuName = "AssetManager/CreateManagerConfig")]
public class AssetManagerConfigScriptableObject : ScriptableObject
{
    /// <summary>
    /// 编辑器模拟下，不进行打包
    /// 本地模式，打包到StreamingAssets
    /// 远端模式，打包到任意远端路径，在该示例中为persistentDataPath
    /// </summary>
    public AssetBundlePattern BuildingPattern;

    /// <summary>
    /// 是否应用增量打包
    /// </summary>
    public IncrementalBuildMode _IncrementalBuildMode;

    /// <summary>
    /// AssetBundle压缩格式
    /// </summary>
    public AssetBundleCompressionPattern CompressionPattern;

    /// <summary>
    /// 资源管理器工具的版本
    /// </summary>
    public int AssetManagerVersion = 100;

    /// <summary>
    /// 资源打包的版本
    /// </summary>
    public int CurrentBuildVersion = 100;

    /// <summary>
    ///需要打包的 AssetBundle文件夹
    /// </summary>
    [SerializeField]

    public DefaultAsset AssetBundleDirectory;

    /// <summary>
    /// 当文件夹变量赋值时，用于储存该文件夹下所有资源路径
    /// </summary>
    public List<string> CurrentAllAssets = new List<string>();
    /// <summary>
    /// 在Editor界面中选择资源，以数组的索引对应
    /// </summary>
    public bool[] CurrentSelectedAssets;

    public List<PackageInfoEditor> packageInfoEditors = new List<PackageInfoEditor>();
    public void GetCurrentDirectoryAllAssets()
    {
        if (AssetBundleDirectory == null)
        {
            return;
        }
        string directoryPath = AssetDatabase.GetAssetPath(AssetBundleDirectory);

        CurrentAllAssets = FindAllAssetFromDirectory(directoryPath);
        
        CurrentSelectedAssets = new bool[CurrentAllAssets.Count];

    }

    public List<string> FindAllAssetFromDirectory(string directoryPath)
    {
        List<string> assetPaths = new List<string>();
        //如果传入的路径为空或者不存在的话
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            Debug.Log("文件夹路径不存在");
            return null;
        }
        //System.IO命名空间下的类，也就是Windows自带的对文件夹进行操作的类
        //System.IO下的类，只能在PC平台或者Windows上读写文件，在移动端不适用
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        //获取该目录下所有文件信息
        //Directory文件夹不属于File类型，所以这里不会获取子文件夹
        FileInfo[] fileInfos = directoryInfo.GetFiles();

        //所有非元数据文件(后缀不是meta的文件)路径都添加到列表中用于打包这些文件
        foreach (FileInfo info in fileInfos)
        {
            //.meta文件代表描述文件
            if (!isValidExtensionName(info.Extension))
            {
                continue;
            }
            //AssetBundle打包只需要文件名
            string assetPath = Path.Combine(directoryPath, info.Name);
            assetPaths.Add(assetPath);
        }
        return assetPaths;
    }
    /// <summary>
    /// 需要排除的Asset拓展名
    /// </summary>
    public string[] InvalidExtensionNames = new string[] { ".meta", ".cs" };
    public bool isValidExtensionName(string fileName)
    {
        bool isValid = true;
        foreach (string invalidName in InvalidExtensionNames)
        {
            if (fileName.Contains(invalidName))
            {
                isValid = false;
                return isValid;
            }
        }
        return isValid;
    }
}
