using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

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
/// 因为Assets下的其他脚本会被编译到AssemblyCsharp.dll中
/// 跟随着包体打包出去（如APK）,所以不允许使用来自UnityEditor命名空间下的方法
/// </summary>
public class HelloWorld : MonoBehaviour
{
    public AssetBundlePattern LoadPattern;

    AssetBundle CubeBundle;
    AssetBundle SphereBundle;
    GameObject SampleObject;

    public Button LoadAssetBundleButton;
    public Button LoadAssetButton;
    public Button UnloadFalseButton;
    public Button UnloadTrueButton;

    /// <summary>
    /// 打包的包名本应该由Editor类管理，但因为资源加载也需要访问
    /// 所以放在资源加载类中
    /// </summary>
    //public static string MainAssetBundleName = "SampleAssetBundle";

    /// <summary>
    /// 除了主包外，实际包名都必须全部小写
    /// </summary>
    //public static string ObjectAssetBundleName = "resourcesbundle";

    public string AssetBundleLoadPath;

    public string HTTPAddress = "http://192.168.203.54:8080/";

    public string HTTPAssetBundlePath;

    public string DownloadPath;
    void Start()
    {
        AssetManagerRuntime.AssetManagerInit(LoadPattern);
        if (LoadPattern == AssetBundlePattern.Remote)
        {
            StartCoroutine(GetRemoteVersion());
        }
        else
        {
            LoadAsset();
        }

        //AssetManagerRuntime.AssetManagerInit(LoadPattern);


        //CheckAssetBundleLoadPath();
        //LoadAssetBundleButton.onClick.AddListener(CheckAssetBundlePattern);
        //LoadAssetButton.onClick.AddListener(LoadAsset);
        //UnloadFalseButton.onClick.AddListener(() => { UnloadAssetBundle(false); });
        //UnloadTrueButton.onClick.AddListener(() => { UnloadAssetBundle(true); });
    }
    void LoadAsset()
    {

        AssetPackage assetPackage = AssetManagerRuntime.Instance.LoadPackage("A");

        Debug.Log(assetPackage.PackageName);

        GameObject sampleObject = assetPackage.LoadAsset<GameObject>("Assets/Resources/Capsule.prefab");
        Instantiate(sampleObject);
    }
    IEnumerator GetRemoteVersion()
    {
        string remoteVersionFilePath = Path.Combine(HTTPAddress, "BuildOutput", "BuildVersion.version");

        UnityWebRequest request = UnityWebRequest.Get(remoteVersionFilePath);

        request.SendWebRequest();

        while(!request.isDone)
        {
            //返回null代表等待一帧
            yield return null;
        }
        if(!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError(request.error);
            yield break;
        }

        int version = int.Parse(request.downloadHandler.text);
        if (AssetManagerRuntime.Instance.LocalAssetVersion == version)
        {
            LoadAsset();
            yield break;
        }
            AssetManagerRuntime.Instance.RemoteAssetVersion = version;

        Debug.Log($"远端资源版本{version}");

        string downloadPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());

        if (!Directory.Exists(downloadPath))
        {
            Directory.CreateDirectory(downloadPath);
        }

        if(AssetManagerRuntime.Instance.LocalAssetVersion!=AssetManagerRuntime.Instance.RemoteAssetVersion)
        {
            StartCoroutine(GetRemotePackages());
        }
        yield return null;
    }

    IEnumerator GetRemotePackages()
    {
        string remotePackagePath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(),"AllPackages");

        UnityWebRequest request = UnityWebRequest.Get(remotePackagePath);

        request.SendWebRequest();

        while (!request.isDone)
        {
            //返回null代表等待一帧
            yield return null;
        }
        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError(request.error);
            yield break;
        }

        string allPackagesString = request.downloadHandler.text;

        
        string packagesSavePath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), "AllPackages");
        File.WriteAllText(packagesSavePath, allPackagesString);

        Debug.Log($"Packages下载完毕{packagesSavePath}");

        List<string> packagesNames = JsonConvert.DeserializeObject<List<string>>(allPackagesString);

        foreach(string packageName in packagesNames)
        {
            remotePackagePath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), packageName);

            request=UnityWebRequest.Get(remotePackagePath);

            request.SendWebRequest();

            while (!request.isDone)
            {
                //返回null代表等待一帧
                yield return null;
            }
            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError(request.error);
                yield break;
            }

            string packageString = request.downloadHandler.text;

            packagesSavePath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), packageName);

            File.WriteAllText(packagesSavePath, packageString);

            Debug.Log($"package下载完毕{packageName}");
        }

        StartCoroutine(GetRemoteAssetBundleHash());
        yield return null;
    }

    IEnumerator GetRemoteAssetBundleHash()
    {
        string remoteHashPath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), "AssetBundleHashs");

        UnityWebRequest request = UnityWebRequest.Get(remoteHashPath);

        request.SendWebRequest();

        while (!request.isDone)
        {
            //返回null代表等待一帧
            yield return null;
        }
        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError(request.error);
            yield break;
        }

        string hashString = request.downloadHandler.text;
        string HashSavePath= Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), "AssetBundleHashs");
        File.WriteAllText(HashSavePath, hashString);
        Debug.Log($"AssetBundleHash列表下载完成{hashString}");
        CreateDownloadList();
        yield return null;
    }


    void CreateDownloadList()
    {
        string localAssetBundleHashPath = Path.Combine(AssetManagerRuntime.Instance.AssetBundleLoadPath, "AssetBundleHashs");

        string assetBundleHashString = "";
        string[] localAssetBundleHash = null;

        if(File.Exists(localAssetBundleHashPath))
        {
            assetBundleHashString = File.ReadAllText(localAssetBundleHashPath);
            localAssetBundleHash = JsonConvert.DeserializeObject<string[]>(assetBundleHashString);
        }

        string remoteAssetBundleHashPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), "AssetBundleHashs");

        string remoteAssetBundleHashString = "";
        string[] remoteAssetBundleHash = null;

        if(File.Exists(remoteAssetBundleHashPath))
        {
            remoteAssetBundleHashString = File.ReadAllText(remoteAssetBundleHashPath);
            remoteAssetBundleHash = JsonConvert.DeserializeObject<string[]>(remoteAssetBundleHashString);
        }

        if(remoteAssetBundleHash==null)
        {
            Debug.LogError($"远程表读取失败{remoteAssetBundleHashPath}");
            return;
        }

        //将要下载的AB包名称
        List<string> assetBundleNames = null;

        if(localAssetBundleHash==null)
        {
            Debug.LogWarning($"本地表读取失败");
            assetBundleNames = remoteAssetBundleHash.ToList();
        }
        else
        {
            AssetBundleVersionDifference versionDifference = ContrastAssetBundleVersion(localAssetBundleHash, remoteAssetBundleHash);

            //新增AB包列表就是将要下载的文件列表
            assetBundleNames = versionDifference.AdditionAssetBundles;
        }

        if (assetBundleNames != null && assetBundleNames.Count > 0)
        {
            //添加主包包名
            assetBundleNames.Add("LocalAssets");

            StartCoroutine(DownnloadAssetBundle(assetBundleNames,()=> { 
                CopyDownloadAssetsToLocalPath();
                AssetManagerRuntime.Instance.UpdataLocalAssetVersion();
                LoadAsset();
            }));
        }
    }

    void CopyDownloadAssetsToLocalPath()
    {
        string downloadAssetVersionPath = Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());
        
        DirectoryInfo directoryInfo = new DirectoryInfo(downloadAssetVersionPath);

        string localVersionPath = Path.Combine(AssetManagerRuntime.Instance.LocalAssetPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString());

        directoryInfo.MoveTo(localVersionPath);
    }
    IEnumerator DownnloadAssetBundle(List<string>fileNames,Action callBack=null)
    {
        foreach(string fileName in fileNames)
        {
            string assetBundleName = fileName;

            if(fileName.Contains("_"))
            {
                //下化线最后一位才是AssetbundleName
                int startIndex=fileName.IndexOf("_")+1;

                assetBundleName = fileName.Substring(startIndex);
            }

            string assetBundleDownloadPath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), assetBundleName);

            UnityWebRequest request = UnityWebRequest.Get(assetBundleDownloadPath);

            request.SendWebRequest();

            while (!request.isDone)
            {
                //返回null代表等待一帧
                yield return null;
            }
            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError(request.error);
                yield break;
            }

            string assetBundleSavePath= Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), assetBundleName);

            File.WriteAllBytes(assetBundleSavePath, request.downloadHandler.data);

            Debug.Log($"AssetBundle下载完毕{assetBundleName}");
        }
        callBack?.Invoke();

        //if(callBack!=null)
        //{
        //    callBack.Invoke();
        //}
        yield return null;
    }
    AssetBundleVersionDifference ContrastAssetBundleVersion(string[] oldVersionAssets, string[] newVersionAssets)
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
    void CheckAssetBundleLoadPath()
    {
        switch (LoadPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                AssetBundleLoadPath = Path.Combine(Application.streamingAssetsPath);
                break;
            case AssetBundlePattern.Remote:
                HTTPAssetBundlePath = Path.Combine(HTTPAddress);
                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssetBundle");
                AssetBundleLoadPath = Path.Combine(DownloadPath);
                if (!Directory.Exists(AssetBundleLoadPath))
                {
                    Directory.CreateDirectory(AssetBundleLoadPath);
                }
                break;
        }
    }


    IEnumerator DownloadFile(string fileName, Action callBack, bool isSaveFile = true)
    {
        string assetBundleDownloadPath = Path.Combine(HTTPAssetBundlePath, fileName);

        UnityWebRequest webRequest = UnityWebRequest.Get(assetBundleDownloadPath);

        yield return webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            //下载总字节数
            Debug.Log(webRequest.downloadedBytes);
            //下载进度
            Debug.Log(webRequest.downloadProgress);
            yield return new WaitForEndOfFrame();
        }
        string fileSavePath = Path.Combine(AssetBundleLoadPath, fileName);
        Debug.Log(webRequest.downloadHandler.data.Length);
        if (isSaveFile)
        {
            yield return SaveFile(fileSavePath, webRequest.downloadHandler.data, callBack);
        }
        else
        {
            //三目运算符判断对象是否为空
            callBack?.Invoke();
        }
    }

    IEnumerator SaveFile(string savePath, byte[] bytes, Action callBack)
    {
        //所有的System.IO方法都只能在Windows平台上运行
        //如果想要跨平台保存文件，应该每个平台调用不同的API
        FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate);

        yield return fileStream.WriteAsync(bytes, 0, bytes.Length);
        //刷新文件状态
        fileStream.Flush();
        //关闭
        fileStream.Close();
        //释放文件流，否则文件会一直处于读取状态而不能被其他进程读取
        fileStream.Dispose();

        callBack?.Invoke();
        Debug.Log($"{savePath}文件保存完成");
    }

    void CheckAssetBundlePattern()
    {
        if (LoadPattern == AssetBundlePattern.Remote)
        {
            StartCoroutine(DownloadFile("", LoadAssetBundle));
        }
        else
        {
            LoadAssetBundle();
        }
    }
    void LoadAssetBundle()
    {
        //通过外部路径加载AB包的方式
        //因为persistentDataPath在移动端可读可写的特性
        //远程下载的AB包都可以放置在该路径下
        //AB包加载可以允许加载工程路径外的路径，加载方式由Unity维护

        string assetBundlePath = Path.Combine(AssetBundleLoadPath, "");
        //加载清单捆绑包
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //manifest文件实际上是明文储存给我们开发者查找索引的
        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));

        //manifest.GetAllDependencies获取的是一个AB包所有直接或间接的引用
        //为避免某些间接引用的资源没有被加载到，建议使用GetAllDependencies
        //manifest.GetDirectDependencies获取的是直接的引用
        foreach (string depAssetBundleName in assetBundleManifest.GetAllDependencies("1"))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(AssetBundleLoadPath, depAssetBundleName);
            //如果不需要使用依赖包实例，可以不用变量储存该实例，但是该实例仍然会存在于内存中
            AssetBundle.LoadFromFile(assetBundlePath);
        }

        assetBundlePath = Path.Combine(AssetBundleLoadPath, "1");

        CubeBundle = AssetBundle.LoadFromFile(assetBundlePath);

        assetBundlePath = Path.Combine(AssetBundleLoadPath, "2");

        SphereBundle = AssetBundle.LoadFromFile(assetBundlePath);
    }



    void UnloadAssetBundle(bool isTrue)
    {
        Debug.Log(isTrue);
        //当前帧销毁对象
        DestroyImmediate(SampleObject);
        //CubeBundle.Unload(isTrue);
        //SphereBundle.Unload(isTrue);

        //使用unload(false)方法有一个很显著的优势，就是不会破坏当前运行时的效果
        //如果有什么资源是AB包创建，但是没有被管理，导致资源仍然被使用而AB包使用unload(true)方法卸载
        //就会导致当前运行时突然丢失某些来自卸载AB包的资源
        //那么很显然的，在不破坏运行时的效果的情况下（也就是调用unload(false)的情况下），使用Resources.unloadUnusedAsset()方法来回收
        //是效果最好的
        //因为所有对内存的操作，都会占据CPU的使用，所以最好在CPU使用情况较低的情况下进行强制的资源卸载
        //例如游戏过场动画，或场景加载时
        Resources.UnloadUnusedAssets();
    }
}
