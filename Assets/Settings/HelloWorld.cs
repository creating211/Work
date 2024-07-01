using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;

public enum AssetBundlePattern
{
    /// <summary>
    /// 编辑器模拟加载应该使用AssetDataPath进行资源加载，而不用进行打包
    /// </summary>
    EditorSimulation,
    /// <summary>
    /// 本地加载模式，应打包到本地路径或者StreamingAssets路径下，从该路径下载
    /// </summary>
    Local,
    /// <summary>
    /// 远端加载模式，应打包到任意的资源服务器地址，然后通过网络进行下载到沙盒路径persistenceDataPath后再进行加载
    /// </summary>
    Remote
}
/// <summary>
/// 因为其他脚本会被编译到AssetmblyCharp.dll中
/// 跟随着包体打包出去（如APK）所有不允许使用来自Unity Editor命名控件下的方法
/// </summary>

public class HelloWorld : MonoBehaviour
{
    public AssetBundlePattern LoadPattern;

    AssetBundle SampleBundle;
    GameObject SampleObject;

    public Button LoadAssetBundleButton;
    public Button LoadAssetButton;
    public Button UnloadFalseButton;
    public Button UnloadTrueButton;

    /// <summary>
    /// 打包的包名本来应该由Editor类管理，但因为加载类也需要访问，所以放在资源加载类中
    /// </summary>
    public static string MainAssetBundleName = "SampleAssetBundle";//主包名
    /// <summary>
    /// 除了主包外，其它包名都要小写
    /// </summary>
    public static string ObjectAssetBundleName = "resourcesbundle";//次包名

    public string AssetBundleLoadPath;

    public string HTTPAddress = "http://127.0.0.1:8080/";

    public string HTTPAssetBundlePath;

    public string DownloadPath;
    void Start()
    {
        CheckAssetBundleLoadPath();
        LoadAssetBundleButton.onClick.AddListener(LoadAssetBundle);

        LoadAssetButton.onClick.AddListener(LoadAsset);

        UnloadFalseButton.onClick.AddListener(() => { UnloadAssetBundle(false); });

        UnloadTrueButton.onClick.AddListener(() => { UnloadAssetBundle(true); });
        /*当不分函数时
        //通过外部路径加载AB包的方式
        //因为persisitenceDataPath在移动端可读可写的特性，远程下载的AB包都可以放置在该路径下
        //string assetBundlePath = Path.Combine(Application.persistentDataPath, "Bundles", "prefab");
        //string assetBundlePath= Path.Combine(Application.dataPath, "Bundles","prefab");
        //处理材质丢失

        string mainBundleName = "Bundles";
        string assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);
        //加载清单捆绑包
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //mainfest文件本身实际上是明文储存给我们开发者看的查找索引

        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(assetBundleManifest));

        foreach(string depAssetBundleName in assetBundleManifest.GetAllDependencies("prefab"))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, depAssetBundleName);
            //如果不需要使用依赖包示例，可以不用变量储存该示例，但是该示例还是存在内容中
            AssetBundle.LoadFromFile(assetBundlePath);
        }
        //AB包还可以运行加载工程路径外的路径，加载方式是由Unity维护
        assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, "prefab");
        AssetBundle prefabAB = AssetBundle.LoadFromFile(assetBundlePath);

        GameObject cubeObject = prefabAB.LoadAsset<GameObject>("Cube");
        Instantiate(cubeObject);
        */
    }
    

    void CheckAssetBundleLoadPath()
    {
        switch (LoadPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                AssetBundleLoadPath= Path.Combine(Application.streamingAssetsPath, MainAssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssetBundle");
                AssetBundleLoadPath= Path.Combine(DownloadPath, MainAssetBundleName);
                if (!Directory.Exists(AssetBundleLoadPath))
                {
                    Directory.CreateDirectory(AssetBundleLoadPath);
                }
                    break;

        }
    }


    /// <summary>
    /// 声明一个携程
    /// </summary>
    /// <returns></returns>
    /// IEnumerator DownloadAssetBundle()是用来下载主包，
    /// IEnumerator DownloadFile(string fileName,bool isSavFile)用来下载文件

    IEnumerator DownloadAssetBundle()
    {
        string remotePath = Path.Combine(HTTPAddress, MainAssetBundleName);
        string mainBundleDownPath = Path.Combine(remotePath, MainAssetBundleName);

        UnityWebRequest webRequest = UnityWebRequest.Get(mainBundleDownPath);

        webRequest.SendWebRequest();
        
        while(!webRequest.isDone)
        {
            //每次都打印下载总字节数
            Debug.Log(webRequest.downloadedBytes);
            //下载进度
            Debug.Log(webRequest.downloadProgress);
            yield return new WaitForEndOfFrame();
        }

        string mainBundleSavePath = Path.Combine(AssetBundleLoadPath, MainAssetBundleName);

        /*
        AssetBundle mainBundle = DownloadHandlerAssetBundle.GetContent(webRequest);
        Debug.Log(mainBundle);

        AssetBundleManifest manifest = mainBundle.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
        foreach(string bundleName in manifest.GetAllAssetBundles())
        {
            Debug.Log((bundleName));
        }
        */
        Debug.Log(webRequest.downloadHandler.data.Length);//将下载的字节数打印出来
        yield return SaveFile(mainBundleSavePath, webRequest.downloadHandler.data);
    }

    IEnumerator SaveFile(string savePath,byte[] bytes)
    {
        FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate); 
        yield return fileStream.WriteAsync(bytes,0,bytes.Length);

        //释放文件流，否则文件会一直处于读取状态而不能被其他进程读取
        fileStream.Flush();
        fileStream.Close();
        fileStream.Dispose();

        Debug.Log($"{savePath}文件保存完成");
    }
    /// <summary>
    /// 加载AB包方法
    /// </summary>
    void  LoadAssetBundle()
    {
        if(LoadPattern==AssetBundlePattern.Remote)
        {
            StartCoroutine(DownloadAssetBundle());
        }

        //拼接的加载打包路径
        /*
        string mainBundleName = "Bundles";
        string assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);
        */

        string assetBundlePath = Path.Combine(AssetBundleLoadPath, MainAssetBundleName);
        //加载清单捆绑包
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //mainfest文件本身实际上是明文储存给我们开发者看的查找索引

        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(assetBundleManifest));

        foreach (string depAssetBundleName in assetBundleManifest.GetAllDependencies(ObjectAssetBundleName))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(AssetBundleLoadPath, depAssetBundleName);
            //如果不需要使用依赖包示例，可以不用变量储存该示例，但是该示例还是存在内容中
            AssetBundle.LoadFromFile(assetBundlePath);
        }
        //AB包还可以运行加载工程路径外的路径，加载方式是由Unity维护
        assetBundlePath = Path.Combine(AssetBundleLoadPath, ObjectAssetBundleName);
        SampleBundle = AssetBundle.LoadFromFile(assetBundlePath);
    }

    /// <summary>
    /// 加载资源方法
    /// </summary>
    void LoadAsset()
    {
        GameObject cubeObject = SampleBundle.LoadAsset<GameObject>("Cube");
        SampleObject=Instantiate(cubeObject);
    }

    void UnloadAssetBundle(bool isTrue)
    {
        Debug.Log(isTrue);
        //当前帧销毁对象
        DestroyImmediate(SampleObject);
        SampleBundle.Unload(isTrue);

        //使用unload（false）的一个很显著的优势就是不会破坏当前运行时的效果，如果有什么资源是我们AB包创建但没有被管理导致资源
        //仍被使用而AB包使用unload（true）卸载就会导致当前运行时丢失某些来自卸载AB包的资源
        //那么很显然的，在不破坏运行时效果的情况下（也就是调用unload（false）的情况下），使用resources.unloadUnusedAsset()方法来回收效果时最好的
        //因为所有对内存的操作都会占据CPU的使用，所以最好在CPU使用情况较低的情况下进行强制的资源卸载
        //例如在游戏中过场动画或者场景加载时
        Resources.UnloadUnusedAssets();
    }
}
