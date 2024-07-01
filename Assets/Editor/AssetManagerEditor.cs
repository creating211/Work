using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public enum AssetBundleCompressionPattern
{
    LZMA,
    LZ4,
    None
}

/// <summary>
/// 任何BuildOption处于非forceRebuild选项下都默认为增量打包
/// </summary>
public enum IncrementalBuildMode
{
    None,
    IncrementalBuild,
    ForceRebuild
}

public class AssetBundleVersionDifference
{
    /// <summary>
    /// 新增资源包
    /// </summary>
    public List<string> AdditionAssetBundles = new List<string>();
    /// <summary>
    /// 减少资源包
    /// </summary>
    public List<string> ReducedAssetBundles = new List<string>();
}

/// <summary>
///代表了Node之间的引用关系，很显然一个Node之间可能引用多个Node，也可能被多个Node所引用
/// </summary>
public class AssetBundleEdge
{
    public List<AssetBundleNode> nodes = new List<AssetBundleNode>();
}

public class AssetBundleNode
{
    public string AssetName;
    /// <summary>
    /// 可以用与判断一个资源是否是SourceAsset，如果是-1说明是DerivedAsset
    /// </summary>
    public int SourceIndex = -1;

    /// <summary>
    /// 当前Node的Index列表，会沿着自身的OutEdge进行传递
    /// </summary>
    public List<int> SourceIndeices = new List<int>();
    /// <summary>
    /// 当前Node所引用的Nodes
    /// </summary>
    public AssetBundleEdge OutEdge;
    /// <summary>
    /// 引用当前Node的Nodes
    /// </summary>
    public AssetBundleEdge InEdge;

}


/// <summary>
/// 所有在Editor目录下的C#脚本都不会跟着资源打包到可执行文件包体中
/// </summary>
public class AssetManagerEditor
{
    //声明版本号
    //public static string AssetManagerVersion = "1.0.0";

    public static AssetManagerConfigScriptableObject AssetManagerConfig;

    public static string AssetBundleOutputPath;



    /// <summary>
    /// 通过MenuItem特性，声明Editor顶部菜单栏选项
    /// </summary>
    [MenuItem(nameof(AssetManagerEditor) + "/" + nameof(BuildAssetBundle))]
    static void BuildAssetBundle()
    {
        CheckBuildOutputPath();
        //PathCombine方法可以在几个字符串之间插入斜杠
        //string outputPath = "E:/AssetBundles/testAB1";
        //string outputPath = "E:/AssetBundles/testAB2";
        //string outputPath = "E:/AssetBundles/testAB3";

        if (!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }

        //不同平台之间的AssetBundle不可以通用
        //该方法会打包工程内所有配置了包名的AB包，即如果没有设置包名就打不出包
        //Options为None时使用LZMA压缩
        //UncompressedAssetBundle不进行压缩
        //ChunkBasedCompression进行LZ4块压缩

        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, CheckCompressionPattern(), EditorUserBuildSettings.activeBuildTarget);

        Debug.Log("AB包打包已完成");
    }
    public static void LoadConfig(AssetManagerEditorWindow window)
    {
        if (AssetManagerConfig == null)
        {
            AssetManagerConfig = AssetDatabase.LoadAssetAtPath<AssetManagerConfigScriptableObject>("Assets/Editor/AssetManagerConfig.asset");
            window.VersionString = AssetManagerConfig.AssetManagerVersion.ToString();
            for (int i = window.VersionString.Length; i >= 1; i--)
            {
                window.VersionString = window.VersionString.Insert(i, ".");
            }
            window.editorWindowDirectory = AssetManagerConfig.AssetBundleDirectory;
        }
    }
    public static void LoadWindowConfig(AssetManagerEditorWindow window)
    {
        if (window.WindowConfig == null)
        {
            //使用AssetDataBase加载资源只需要传入Assets目录下的路径即可
            window.WindowConfig = AssetDatabase.LoadAssetAtPath<AssetManagerEditorWindowConfigSO>("Assets/Editor/AssetManagerEditorWindowConfig.asset");
            window.WindowConfig.TitleTextStyle = new GUIStyle();
            window.WindowConfig.TitleTextStyle.fontSize = 26;
            window.WindowConfig.TitleTextStyle.normal.textColor = Color.red;
            window.WindowConfig.TitleTextStyle.alignment = TextAnchor.MiddleCenter;

            window.WindowConfig.VersionTextStyle = new GUIStyle();
            window.WindowConfig.VersionTextStyle.fontSize = 20;
            window.WindowConfig.VersionTextStyle.normal.textColor = Color.white;
            window.WindowConfig.VersionTextStyle.alignment = TextAnchor.MiddleRight;

            //加载图片资源到编辑器窗口中
            window.WindowConfig.LogoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/background.jpg");
            window.WindowConfig.LogoTextureStyle = new GUIStyle();
            window.WindowConfig.LogoTextureStyle.alignment = TextAnchor.MiddleCenter;
        }

    }

    public static void LoadCongifFromJson()
    {
        string configPath = Path.Combine(Application.dataPath, "Editor/AssetManagerConfig.amc");

        string configString = File.ReadAllText(configPath);

        JsonUtility.FromJsonOverwrite(configString, AssetManagerConfig);


    }
    public static void SaveConfigToJson()
    {
        if (AssetManagerConfig != null)
        {
            string configString = JsonUtility.ToJson(AssetManagerConfig);
            string outPath = Path.Combine(Application.dataPath, "Editor/AssetManagerConfig.amc");
            File.WriteAllText(outPath, configString);


            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
    /// <summary>
    /// 返回由一个包中所有Asset的GUID列表经过算法加密后得到的哈希码字符串
    /// 如果GUID列表不发生变化，以及加密算法和参数没有发生变化
    /// 那么总是能够得到相同的字符串
    /// </summary>
    /// <param name="assetNames"></param>
    /// <returns></returns>
    static string ComputeAssetSetSignature(IEnumerable<string> assetNames)
    {
        var assetGUIDs = assetNames.Select(AssetDatabase.AssetPathToGUID);
        MD5 currentMD5 = MD5.Create();

        foreach (var assetGUID in assetGUIDs.OrderBy(x => x))
        {
            byte[] bytes = Encoding.ASCII.GetBytes(assetGUID);
            //使用MD5算法加密字节数组
            currentMD5.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }
        currentMD5.TransformFinalBlock(new byte[0], 0, 0);
        return BytesToHexString(currentMD5.Hash);
    }
    /// <summary>
    /// byte转16进制字符串
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    static string BytesToHexString(byte[] bytes)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var aByte in bytes)
        {
            stringBuilder.Append(aByte.ToString("x2"));
        }
        return stringBuilder.ToString();
    }

    static string[] BuildAssetBundleHashTable(AssetBundleBuild[] assetBundleBuilds)
    {
        //表的长度和AssetBundle的数量保持一致
        string[] assetBundleHashs = new string[assetBundleBuilds.Length];

        for (int i = 0; i < assetBundleBuilds.Length; i++)
        {
            string assetBundlePath = Path.Combine(AssetBundleOutputPath, assetBundleBuilds[i].assetBundleName);
            FileInfo info = new FileInfo(assetBundlePath);
            //表中记录的是一个AssetBundle文件的长度，以及其内容的MD5哈希值
            assetBundleHashs[i] = $"{info.Length}_{assetBundleBuilds[i].assetBundleName}";
        }

        string hashString = JsonConvert.SerializeObject(assetBundleHashs);
        string hashFilePath = Path.Combine(AssetBundleOutputPath, "AssetBundleHashs");

        File.WriteAllText(hashFilePath, hashString);

        return assetBundleHashs;
    }

    static AssetBundleVersionDifference ContrastAssetBundleVersion(string[] oldVersionAssets, string[] newVersionAssets)
    {
        AssetBundleVersionDifference difference = new AssetBundleVersionDifference();
        foreach (var assetName in oldVersionAssets)
        {
            if (newVersionAssets.Contains(assetName))
            {
                difference.ReducedAssetBundles.Add(assetName);
            }
        }

        foreach (var assetName in newVersionAssets)
        {
            if (!oldVersionAssets.Contains(assetName))
            {
                difference.AdditionAssetBundles.Add(assetName);
            }
        }

        return difference;
    }

    public static void BuildAssetBundleFromDirectedGraph()
    {
        CheckBuildOutputPath();
        List<string> selectedAssets = GetAllSelectedAssets();
        List<AssetBundleNode> allNodes = new List<AssetBundleNode>();
        //当前所选中的资源就是SourceAsset，所以首先调加SourceAsset的Node
        for (int i = 0; i < selectedAssets.Count; i++)
        {
            AssetBundleNode currenNode = new AssetBundleNode();
            currenNode.AssetName = selectedAssets[i];
            currenNode.SourceIndex = i;
            currenNode.SourceIndeices = new List<int>() { i };
            currenNode.InEdge = new AssetBundleEdge();
            allNodes.Add(currenNode);

            GetNodeFromDependencies(currenNode, allNodes);
        }

        Dictionary<List<int>, List<AssetBundleNode>> assetBundleNodeDic = new Dictionary<List<int>, List<AssetBundleNode>>();
        foreach (AssetBundleNode node in allNodes)
        {
            bool isEquals = false;
            List<int> keyList = new List<int>();
            foreach (List<int> key in assetBundleNodeDic.Keys)
            {
                //判断key的长度是否和当前node的SourceIndeies长度相等
                isEquals = node.SourceIndeices.Count == key.Count && node.SourceIndeices.All(p => key.Any(k => k.Equals(p)));
                if (isEquals)
                {
                    keyList = key;
                    break;
                }
            }
            if (!isEquals)
            {
                keyList = node.SourceIndeices;
                assetBundleNodeDic.Add(node.SourceIndeices, new List<AssetBundleNode>());
            }
            assetBundleNodeDic[keyList].Add(node);
        }
        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[assetBundleNodeDic.Count];
        int buildIndex = 0;

        foreach (List<int> key in assetBundleNodeDic.Keys)
        {

            List<string> assetNames = new List<string>();
            //这一层循环都是从一个键值对中获取node
            //也就是从SourceIndeices相同的集合中获取相应的Node所代表的Asset
            foreach (AssetBundleNode node in assetBundleNodeDic[key])
            {
                assetNames.Add(node.AssetName);
            }
            string[] assetNamesArray = assetNames.ToArray();
            assetBundleBuilds[buildIndex].assetBundleName = ComputeAssetSetSignature(assetNamesArray);
            assetBundleBuilds[buildIndex].assetNames = assetNamesArray;
            buildIndex++;
        }
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckIncrementalBuildMode(),
            BuildTarget.StandaloneWindows);

        string[] currentVersionAssetHashs = BuildAssetBundleHashTable(assetBundleBuilds);

        CopyAssetBundleToVersionFolder();
        GetVersionDifference(currentVersionAssetHashs);
        AssetManagerConfig.CurrentBuildVersion++;
        //刷新Project界面，如果不是打包到工程内则不需要执行
        AssetDatabase.Refresh();

        /*
        foreach(AssetBundleNode node in allNodes)
        {
            if(node.SourceIndex>=0)
            {
                Debug.Log($"{ node.AssetName}是一个SourceAsset");
            }
            else
            {
                Debug.Log($"{ node.AssetName}是一个DrivedAsset,被{node.SourceIndeices.Count}个资源所引用");
            }
        }*/

    }

    static void CopyAssetBundleToVersionFolder()
    {
        string versionString = AssetManagerConfig.CurrentBuildVersion.ToString();
        for (int i = versionString.Length - 1; i >= 1; i--)
        {
            versionString = versionString.Insert(i, ".");
        }

        string assetBundleVersionPath = Path.Combine(Application.streamingAssetsPath, versionString, HelloWorld.MainAssetBundleName);
        if (!Directory.Exists(assetBundleVersionPath))
        {
            Directory.CreateDirectory(assetBundleVersionPath);
        }

        string[] assetNames = ReadAssetBundleHashTable(AssetBundleOutputPath);

        //复制哈希表
        string hashTableOriginPath = Path.Combine(AssetBundleOutputPath, "AssetBundleHashs");
        string hashTableVersionPath = Path.Combine(assetBundleVersionPath, "AssetBundleHashs");
        File.Copy(hashTableOriginPath, hashTableVersionPath);
        //复制主包
        string mainBundleOriginPath = Path.Combine(AssetBundleOutputPath, HelloWorld.MainAssetBundleName);
        string mainBundleVersionPath = Path.Combine(assetBundleVersionPath, HelloWorld.MainAssetBundleName);
        File.Copy(mainBundleOriginPath, mainBundleVersionPath);

        foreach (var assetName in assetNames)
        {
            string assetHashName = assetName.Substring(assetName.IndexOf("_") + 1);

            string assetOriginPath = Path.Combine(AssetBundleOutputPath, assetHashName);
            //fileInfo.Name是包含了扩展名的文件名
            string assetVersionPath = Path.Combine(assetBundleVersionPath, assetHashName);
            //fileInfo.FullName是包含了目录和文件名的文件完整路径
            File.Copy(assetOriginPath, assetVersionPath, true);
        }
    }
    static BuildAssetBundleOptions CheckIncrementalBuildMode()
    {
        BuildAssetBundleOptions option = BuildAssetBundleOptions.None;
        switch (AssetManagerConfig._IncrementalBuildMode)
        {
            case IncrementalBuildMode.None:
                option = BuildAssetBundleOptions.None;
                break;
            case IncrementalBuildMode.IncrementalBuild:
                option = BuildAssetBundleOptions.DeterministicAssetBundle;
                break;
            case IncrementalBuildMode.ForceRebuild:
                option = BuildAssetBundleOptions.ForceRebuildAssetBundle;
                break;
        }
        return option;
    }

    static string[] ReadAssetBundleHashTable(string outputPath)
    {
        string VersionHashTablePath = Path.Combine(outputPath, "AssetBundleHashs");

        string VersionHashString = File.ReadAllText(VersionHashTablePath);

        string[] VersionAssetHashs = JsonConvert.DeserializeObject<string[]>(VersionHashString);

        return VersionAssetHashs;
    }

    static void GetVersionDifference(string[] currentAssetHashs)
    {
        if (AssetManagerConfig.CurrentBuildVersion >= 101)
        {
            int lastVersion = AssetManagerConfig.CurrentBuildVersion - 1;
            string versionString = AssetManagerConfig.CurrentBuildVersion.ToString();
            for (int i = versionString.Length - 1; i >= 1; i--)
            {
                versionString = versionString.Insert(i, ".");
            }
            var lastOutputPath = Path.Combine(Application.streamingAssetsPath, versionString, HelloWorld.MainAssetBundleName);

            string[] lastVersionAssetHashs = ReadAssetBundleHashTable(lastOutputPath);

            AssetBundleVersionDifference difference = ContrastAssetBundleVersion(lastVersionAssetHashs, currentAssetHashs);


            foreach (var assetName in difference.AdditionAssetBundles)
            {
                Debug.Log($"当前版本新增资源{assetName}");

            }

            foreach (var assetName in difference.AdditionAssetBundles)
            {
                Debug.Log($"当前版本减少资源{assetName}");
            }

        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lastNode"></param>调用该函数的Node，本次创建的所有Node都为该Node的OutEdge
    /// <param name="allNode"></param>当前所有的Node，可以用成员变量代替
    public static void GetNodeFromDependencies(AssetBundleNode lastNode, List<AssetBundleNode> allNodes)
    {
        //因为有向图是一层一层建议依赖关系，所以不能直接获取当前资源的全部依赖
        //所以这里只获取当前资源的直接依赖
        string[] assetNames = AssetDatabase.GetDependencies(lastNode.AssetName, false);
        if (assetNames.Length == 0)
        {
            //有向图到了终点
            return;
        }
        if (lastNode.OutEdge == null)
        {
            lastNode.OutEdge = new AssetBundleEdge();
        }
        foreach (string assetName in assetNames)
        {
            if (!AssetManagerConfig.isValidExtensionName(assetName))
            {
                continue;
            }
            AssetBundleNode currentNode = null;
            foreach (AssetBundleNode existingNode in allNodes)
            {
                //如果当前资源名称已经被某个Node所使用，那么判断相同的资源直接使用已经存在的Node
                if (existingNode.AssetName == assetName)
                {
                    currentNode = existingNode;
                    break;
                }
            }
            if (currentNode == null)
            {
                currentNode = new AssetBundleNode();
                currentNode.AssetName = assetName;
                currentNode.InEdge = new AssetBundleEdge();
                allNodes.Add(currentNode);
            }

            currentNode.InEdge.nodes.Add(lastNode);
            lastNode.OutEdge.nodes.Add(currentNode);

            //如果lastNode是SourceAsset,则直接为当前Node添加last Node的Index
            //因为List是一个引用类型，所以SourceAsset的Sourceindeies哪怕内容和derived一样，也视为一个新的List
            if (lastNode.SourceIndex >= 0)
            {

                currentNode.SourceIndeices.Add(lastNode.SourceIndex);
            }
            //否则是DerivedAsset,直接获取last Node的SourceIndices即可
            else
            {
                foreach (int index in lastNode.SourceIndeices)
                {
                    if (currentNode.SourceIndeices.Contains(index))
                    {
                        currentNode.SourceIndeices.Add(index);
                    }
                }
                currentNode.SourceIndeices = lastNode.SourceIndeices;
            }

        }
    }
    public static List<string> GetAllSelectedAssets()
    {
        List<string> selectedAssets = new List<string>();

        if (AssetManagerConfig.CurrentSelectedAssets == null || AssetManagerConfig.CurrentSelectedAssets.Length == 0)
        {
            return null;
        }
        //将值为true的对应索引文件，添加到要打包的资源列表中
        for (int i = 0; i < AssetManagerConfig.CurrentSelectedAssets.Length; i++)
        {
            if (AssetManagerConfig.CurrentSelectedAssets[i])
            {
                selectedAssets.Add(AssetManagerConfig.CurrentAllAssets[i]);
            }
        }
        return selectedAssets;
    }

    public static List<string> GetSeletedAssetsDependencies()
    {
        List<string> depensencies = new List<string>();
        List<string> selecedAssets = GetAllSelectedAssets();
        for (int i = 0; i < selecedAssets.Count; i++)
        {
            //所有通过该方法获取到的数组，可以视为集合L中的一个元素
            string[] deps = AssetDatabase.GetDependencies(selecedAssets[i], true);
            foreach (string depName in deps)
            {
                Debug.Log(depName);
            }
        }
        return depensencies;
    }



    static BuildAssetBundleOptions CheckCompressionPattern()
    {
        BuildAssetBundleOptions option = new BuildAssetBundleOptions();
        switch (AssetManagerConfig.CompressionPattern)
        {
            case AssetBundleCompressionPattern.LZMA:
                option = BuildAssetBundleOptions.None;
                break;
            case AssetBundleCompressionPattern.LZ4:
                option = BuildAssetBundleOptions.ChunkBasedCompression;
                break;
            case AssetBundleCompressionPattern.None:
                option = BuildAssetBundleOptions.UncompressedAssetBundle;
                break;
        }
        return option;
    }
    [MenuItem(nameof(AssetManagerEditor) + "/" + nameof(OpenAssetManagerWindow))]
    static void OpenAssetManagerWindow()
    {
        //方法一,通过EditorWindow.GetWindowWithRect()获取一个具有具体矩形大小的窗口类
        //Rect windowRect = new Rect(0, 0, 500, 500);
        //AssetManagerEditorWindow window = (AssetManagerEditorWindow) EditorWindow.GetWindowWithRect(typeof
        //    (AssetManagerEditorWindow),windowRect,true,nameof(AssetManagerEditor));

        //方法二，通过EditorWindow.GetWindow()获取一个自定义大小，可任意拖拽的窗口
        //AssetManagerEditorWindow window = (AssetManagerEditorWindow)EditorWindow.GetWindow(typeof
        //    (AssetManagerEditorWindow), true, nameof(AssetManagerEditor));
        //如果不赋予名称就可以作为Unity窗口随意放置在面板中
        AssetManagerEditorWindow window = (AssetManagerEditorWindow)EditorWindow.GetWindow(typeof(AssetManagerEditorWindow));

    }

    static void CheckBuildOutputPath()
    {

        switch (AssetManagerConfig.BuildingPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                AssetBundleOutputPath = Path.Combine(Application.streamingAssetsPath, "Local", HelloWorld.MainAssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                AssetBundleOutputPath = Path.Combine(Application.persistentDataPath, "Remote", HelloWorld.MainAssetBundleName);
                break;
        }
        if (string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("输出路径为空");
            return;
        }
        else if (!Directory.Exists(AssetBundleOutputPath))
        {
            //若路径不存在就创建路径
            Directory.CreateDirectory(AssetBundleOutputPath);
        }
    }

    /// <summary>
    /// 因为List是引用型参数，所以方法中对于参数的修改会反应到传入参数的变量上
    /// 因为本质上，参数只是引用了变量的指针，所以最终汇修改的是同一个对象的值
    /// </summary>
    /// <param name="setsA"></param>
    /// <param name="setsB"></param>
    /// <returns></returns>

    public static List<GUID> ContrastDepedenciesFromGUID(List<GUID> setsA, List<GUID> setsB)
    {
        List<GUID> newDependencies = new List<GUID>();
        //取交集
        foreach (var assetGUID in setsA)
        {
            if (setsB.Contains(assetGUID))
            {
                newDependencies.Add(assetGUID);
            }
        }
        //取差集
        foreach (var assetGUID in newDependencies)
        {
            if (setsA.Contains(assetGUID))
            {
                setsA.Remove(assetGUID);
            }
            if (setsB.Contains(assetGUID))
            {
                setsB.Remove(assetGUID);
            }
        }
        //返回集合Snew
        return newDependencies;
    }
    public static void BuiAssetBundleFromSets()
    {
        CheckBuildOutputPath();
        
        //被选中将要打包的资源列表,即列表A
        List<string> selectedAssets = GetAllSelectedAssets();

        //集合列表L
        List<List<GUID>> selectedAssetsDependencies = new List<List<GUID>>();

        //遍历所有选择的SourceAssets以及依赖，获得集合L
        foreach (string selectedAsset in selectedAssets)
        {
            //获取所有SourceAsset的DerivedAsset
            string[] assetDeps = AssetDatabase.GetDependencies(selectedAsset, true);
            List<GUID> assetGUIDs = new List<GUID>();
            foreach (string assetdep in assetDeps)
            {
                GUID assetGUID = AssetDatabase.GUIDFromAssetPath(assetdep);
                assetGUIDs.Add(assetGUID);
            }

            //将包含了SourceAsset以及DerivedAsset的集合添加到集合L中
            selectedAssetsDependencies.Add(assetGUIDs);
        }
        for (int i = 0; i < selectedAssetsDependencies.Count; i++)
        {
            int nextIndex = i + 1;
            if (nextIndex >= selectedAssetsDependencies.Count)
            {
                break;
            }
            Debug.Log($"对比之前{selectedAssetsDependencies[i].Count}");
            Debug.Log($"对比之前{selectedAssetsDependencies[nextIndex].Count}");

            for (int j = 0; j <= i; j++)
            {
                List<GUID> newDependencies = ContrastDepedenciesFromGUID(selectedAssetsDependencies[j], selectedAssetsDependencies[nextIndex]);
                //将Snew集合添加到集合列表L中
                if (newDependencies != null && newDependencies.Count > 0)
                {
                    selectedAssetsDependencies.Add(newDependencies);
                }
            }
        }
        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[selectedAssetsDependencies.Count];
        for (int i = 0; i < assetBundleBuilds.Length; i++)
        {
            assetBundleBuilds[i].assetBundleName = i.ToString();
            string[] assetNames = new string[selectedAssetsDependencies[i].Count];
            List<GUID> assetGUIDs = selectedAssetsDependencies[i];
            for (int j = 0; j < assetNames.Length; j++)
            {
                string assetName = AssetDatabase.GUIDToAssetPath(assetGUIDs[j]);
                if (assetName.Contains(".cs"))
                {
                    continue;
                }
                assetNames[j] = assetName;
            }
            assetBundleBuilds[i].assetNames = assetNames;
        }

        /*
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckCompressionPattern(),
            BuildTarget.StandaloneWindows);

        //刷新Project界面，如果不是打包到工程内则不需要执行
        AssetDatabase.Refresh();
        */
    }
    public static void BuildAssetBundleFromEditorWindow()
    {
        CheckBuildOutputPath();
        if (AssetManagerConfig.AssetBundleDirectory == null)
        {
            Debug.LogError("打包目录不存在");
            return;
        }

        //被选中将要打包的资源列表
        List<string> selectedAssets = GetAllSelectedAssets();

        //选中多少个资源则打包多少个AB包
        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[selectedAssets.Count];

        string directoryPath = AssetDatabase.GetAssetPath(AssetManagerConfig.AssetBundleDirectory);

        for (int i = 0; i < assetBundleBuilds.Length; i++)
        {
            string bundleName = selectedAssets[i].Replace($@"{directoryPath}\", string.Empty);
            //Unity作导入.prefab文作时，会默认使用预制体导入器导入，而assetBundle不是预制体，所以会导致报错
            bundleName = bundleName.Replace(".prefab", string.Empty);

            assetBundleBuilds[i].assetBundleName = bundleName;

            assetBundleBuilds[i].assetNames = new string[] { selectedAssets[i] };
        }

        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckCompressionPattern(),
            BuildTarget.StandaloneWindows);

        //打印输出路径
        Debug.Log(AssetBundleOutputPath);

        //刷新Project界面，如果不是打包到工程内则不需要执行
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 打包指定文件夹下所有资源为AssetBundle
    /// </summary>
    public static void BuildAssetBundleFromDirectory()
    {
        CheckBuildOutputPath();
        if (AssetManagerConfig.AssetBundleDirectory == null)
        {
            Debug.LogError("打包目录不存在");
            return;
        }


        AssetBundleBuild[] assetBundleBuild = new AssetBundleBuild[1];

        //将要打包的具体包名，而不是主包名
        assetBundleBuild[0].assetBundleName = HelloWorld.ObjectAssetBundleName;

        //这里虽然名为Name，实际上需要资源在工程下的路径
        assetBundleBuild[0].assetNames = AssetManagerConfig.CurrentAllAssets.ToArray();

    }

    /// <summary>
    /// 传入包括拓展名的文件名，用于和无效拓展名数组进行对比
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>


}