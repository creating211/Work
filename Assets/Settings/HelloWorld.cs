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
    /// ������Դ��
    /// </summary>
    public List<string> AdditionAssetBundles = new List<string>();
    /// <summary>
    /// ������Դ��
    /// </summary>
    public List<string> ReducedAssetBundles = new List<string>();
}

/// <summary>
/// ��ΪAssets�µ������ű��ᱻ���뵽AssemblyCsharp.dll��
/// �����Ű�������ȥ����APK��,���Բ�����ʹ������UnityEditor�����ռ��µķ���
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
    /// ����İ�����Ӧ����Editor���������Ϊ��Դ����Ҳ��Ҫ����
    /// ���Է�����Դ��������
    /// </summary>
    //public static string MainAssetBundleName = "SampleAssetBundle";

    /// <summary>
    /// ���������⣬ʵ�ʰ���������ȫ��Сд
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
            //����null����ȴ�һ֡
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

        Debug.Log($"Զ����Դ�汾{version}");

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
            //����null����ȴ�һ֡
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

        Debug.Log($"Packages�������{packagesSavePath}");

        List<string> packagesNames = JsonConvert.DeserializeObject<List<string>>(allPackagesString);

        foreach(string packageName in packagesNames)
        {
            remotePackagePath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), packageName);

            request=UnityWebRequest.Get(remotePackagePath);

            request.SendWebRequest();

            while (!request.isDone)
            {
                //����null����ȴ�һ֡
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

            Debug.Log($"package�������{packageName}");
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
            //����null����ȴ�һ֡
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
        Debug.Log($"AssetBundleHash�б��������{hashString}");
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
            Debug.LogError($"Զ�̱��ȡʧ��{remoteAssetBundleHashPath}");
            return;
        }

        //��Ҫ���ص�AB������
        List<string> assetBundleNames = null;

        if(localAssetBundleHash==null)
        {
            Debug.LogWarning($"���ر��ȡʧ��");
            assetBundleNames = remoteAssetBundleHash.ToList();
        }
        else
        {
            AssetBundleVersionDifference versionDifference = ContrastAssetBundleVersion(localAssetBundleHash, remoteAssetBundleHash);

            //����AB���б���ǽ�Ҫ���ص��ļ��б�
            assetBundleNames = versionDifference.AdditionAssetBundles;
        }

        if (assetBundleNames != null && assetBundleNames.Count > 0)
        {
            //�����������
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
                //�»������һλ����AssetbundleName
                int startIndex=fileName.IndexOf("_")+1;

                assetBundleName = fileName.Substring(startIndex);
            }

            string assetBundleDownloadPath = Path.Combine(HTTPAddress, "BuildOutput", AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), assetBundleName);

            UnityWebRequest request = UnityWebRequest.Get(assetBundleDownloadPath);

            request.SendWebRequest();

            while (!request.isDone)
            {
                //����null����ȴ�һ֡
                yield return null;
            }
            if (!string.IsNullOrEmpty(request.error))
            {
                Debug.LogError(request.error);
                yield break;
            }

            string assetBundleSavePath= Path.Combine(AssetManagerRuntime.Instance.DownloadPath, AssetManagerRuntime.Instance.RemoteAssetVersion.ToString(), assetBundleName);

            File.WriteAllBytes(assetBundleSavePath, request.downloadHandler.data);

            Debug.Log($"AssetBundle�������{assetBundleName}");
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
            //�������ֽ���
            Debug.Log(webRequest.downloadedBytes);
            //���ؽ���
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
            //��Ŀ������ж϶����Ƿ�Ϊ��
            callBack?.Invoke();
        }
    }

    IEnumerator SaveFile(string savePath, byte[] bytes, Action callBack)
    {
        //���е�System.IO������ֻ����Windowsƽ̨������
        //�����Ҫ��ƽ̨�����ļ���Ӧ��ÿ��ƽ̨���ò�ͬ��API
        FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate);

        yield return fileStream.WriteAsync(bytes, 0, bytes.Length);
        //ˢ���ļ�״̬
        fileStream.Flush();
        //�ر�
        fileStream.Close();
        //�ͷ��ļ����������ļ���һֱ���ڶ�ȡ״̬�����ܱ��������̶�ȡ
        fileStream.Dispose();

        callBack?.Invoke();
        Debug.Log($"{savePath}�ļ��������");
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
        //ͨ���ⲿ·������AB���ķ�ʽ
        //��ΪpersistentDataPath���ƶ��˿ɶ���д������
        //Զ�����ص�AB�������Է����ڸ�·����
        //AB�����ؿ���������ع���·�����·�������ط�ʽ��Unityά��

        string assetBundlePath = Path.Combine(AssetBundleLoadPath, "");
        //�����嵥�����
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //manifest�ļ�ʵ���������Ĵ�������ǿ����߲���������
        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));

        //manifest.GetAllDependencies��ȡ����һ��AB������ֱ�ӻ��ӵ�����
        //Ϊ����ĳЩ������õ���Դû�б����ص�������ʹ��GetAllDependencies
        //manifest.GetDirectDependencies��ȡ����ֱ�ӵ�����
        foreach (string depAssetBundleName in assetBundleManifest.GetAllDependencies("1"))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(AssetBundleLoadPath, depAssetBundleName);
            //�������Ҫʹ��������ʵ�������Բ��ñ��������ʵ�������Ǹ�ʵ����Ȼ��������ڴ���
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
        //��ǰ֡���ٶ���
        DestroyImmediate(SampleObject);
        //CubeBundle.Unload(isTrue);
        //SphereBundle.Unload(isTrue);

        //ʹ��unload(false)������һ�������������ƣ����ǲ����ƻ���ǰ����ʱ��Ч��
        //�����ʲô��Դ��AB������������û�б�����������Դ��Ȼ��ʹ�ö�AB��ʹ��unload(true)����ж��
        //�ͻᵼ�µ�ǰ����ʱͻȻ��ʧĳЩ����ж��AB������Դ
        //��ô����Ȼ�ģ��ڲ��ƻ�����ʱ��Ч��������£�Ҳ���ǵ���unload(false)������£���ʹ��Resources.unloadUnusedAsset()����������
        //��Ч����õ�
        //��Ϊ���ж��ڴ�Ĳ���������ռ��CPU��ʹ�ã����������CPUʹ������ϵ͵�����½���ǿ�Ƶ���Դж��
        //������Ϸ�����������򳡾�����ʱ
        Resources.UnloadUnusedAssets();
    }
}
