
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using UnityEngine.UI;
//using UnityEngine.Networking;

//public enum AssetBundlePattern
//{
//    /// <summary>
//    /// �༭��ģ�����Ӧ��ʹ��AssetDataPath������Դ���أ������ý��д��
//    /// </summary>
//    EditorSimulation,
//    /// <summary>
//    /// ���ؼ���ģʽ��Ӧ���������·������StreamingAssets·���£��Ӹ�·������
//    /// </summary>
//    Local,
//    /// <summary>
//    /// Զ�˼���ģʽ��Ӧ������������Դ��������ַ��Ȼ��ͨ������������ص�ɳ��·��persistenceDataPath���ٽ��м���
//    /// </summary>
//    Remote
//}
///// <summary>
///// ��Ϊ�����ű��ᱻ���뵽AssetmblyCharp.dll��
///// �����Ű�������ȥ����APK�����в�����ʹ������Unity Editor�����ؼ��µķ���
///// </summary>

//public class HelloWorld : MonoBehaviour
//{
//    public AssetBundlePattern LoadPattern;
//    AssetBundle SphereAssetBundle;
//    AssetBundle CubeAssetBundle;
//    GameObject SampleObject;

//    public Button LoadAssetBundleButton;
//    public Button LoadAssetButton;
//    public Button UnloadFalseButton;
//    public Button UnloadTrueButton;

//    /// <summary>
//    /// ����İ�������Ӧ����Editor���������Ϊ������Ҳ��Ҫ���ʣ����Է�����Դ��������
//    /// </summary>
//    public static string MainAssetBundleName = "SampleAssetBundle";//������
//    /// <summary>
//    /// ���������⣬����������ҪСд
//    /// </summary>
//    public static string ObjectAssetBundleName = "resourcesbundle";//�ΰ���

//    public string AssetBundleLoadPath;

//    public string HTTPAddress = "http://127.0.0.1:8080/";

//    public string HTTPAssetBundlePath;

//    public string DownloadPath;
//    void Start()
//    {
//        CheckAssetBundleLoadPath();
//        LoadAssetBundleButton.onClick.AddListener(CheckAssetPattern);

//        LoadAssetButton.onClick.AddListener(LoadAsset);

//        UnloadFalseButton.onClick.AddListener(() => { UnloadAssetBundle(false); });

//        UnloadTrueButton.onClick.AddListener(() => { UnloadAssetBundle(true); });
//        /*�����ֺ���ʱ
//        //ͨ���ⲿ·������AB���ķ�ʽ
//        //��ΪpersisitenceDataPath���ƶ��˿ɶ���д�����ԣ�Զ�����ص�AB�������Է����ڸ�·����
//        //string assetBundlePath = Path.Combine(Application.persistentDataPath, "Bundles", "prefab");
//        //string assetBundlePath= Path.Combine(Application.dataPath, "Bundles","prefab");
//        //������ʶ�ʧ

//        string mainBundleName = "Bundles";
//        string assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);
//        //�����嵥�����
//        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

//        //mainfest�ļ�����ʵ���������Ĵ�������ǿ����߿��Ĳ�������

//        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(assetBundleManifest));

//        foreach(string depAssetBundleName in assetBundleManifest.GetAllDependencies("prefab"))
//        {
//            Debug.Log(depAssetBundleName);
//            assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, depAssetBundleName);
//            //�������Ҫʹ��������ʾ�������Բ��ñ��������ʾ�������Ǹ�ʾ�����Ǵ���������
//            AssetBundle.LoadFromFile(assetBundlePath);
//        }
//        //AB�����������м��ع���·�����·�������ط�ʽ����Unityά��
//        assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, "prefab");
//        AssetBundle prefabAB = AssetBundle.LoadFromFile(assetBundlePath);

//        GameObject cubeObject = prefabAB.LoadAsset<GameObject>("Cube");
//        Instantiate(cubeObject);
//        */
//    }


//    void CheckAssetBundleLoadPath()
//    {
//        switch (LoadPattern)
//        {
//            case AssetBundlePattern.EditorSimulation:
//                break;
//            case AssetBundlePattern.Local:
//                AssetBundleLoadPath= Path.Combine(Application.streamingAssetsPath, MainAssetBundleName);
//                break;
//            case AssetBundlePattern.Remote:
//                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssetBundle");
//                AssetBundleLoadPath= Path.Combine(DownloadPath, MainAssetBundleName);
//                if (!Directory.Exists(AssetBundleLoadPath))
//                {
//                    Directory.CreateDirectory(AssetBundleLoadPath);
//                }
//                    break;
//        }

//    }


//    /// <summary>
//    /// ����һ��Я��
//    /// </summary>
//    /// <returns></returns>
//    /// IEnumerator DownloadAssetBundle()����������������
//    /// IEnumerator DownloadFile(string fileName,bool isSavFile)���������ļ�

//    IEnumerator DownloadAssetBundle()
//    {
//        string remotePath = Path.Combine(HTTPAddress, MainAssetBundleName);
//        string mainBundleDownPath = Path.Combine(remotePath, MainAssetBundleName);

//        UnityWebRequest webRequest = UnityWebRequest.Get(mainBundleDownPath);

//        webRequest.SendWebRequest();

//        while(!webRequest.isDone)
//        {
//            //ÿ�ζ���ӡ�������ֽ���
//            Debug.Log(webRequest.downloadedBytes);
//            //���ؽ���
//            Debug.Log(webRequest.downloadProgress);
//            yield return new WaitForEndOfFrame();
//        }

//        string mainBundleSavePath = Path.Combine(AssetBundleLoadPath, MainAssetBundleName);

//        /*
//        AssetBundle mainBundle = DownloadHandlerAssetBundle.GetContent(webRequest);
//        Debug.Log(mainBundle);

//        AssetBundleManifest manifest = mainBundle.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));
//        foreach(string bundleName in manifest.GetAllAssetBundles())
//        {
//            Debug.Log((bundleName));
//        }
//        */
//        Debug.Log(webRequest.downloadHandler.data.Length);//�����ص��ֽ�����ӡ����
//        yield return SaveFile(mainBundleSavePath, webRequest.downloadHandler.data);
//    }

//    IEnumerator SaveFile(string savePath,byte[] bytes)
//    {
//        FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate); 
//        yield return fileStream.WriteAsync(bytes,0,bytes.Length);

//        //�ͷ��ļ����������ļ���һֱ���ڶ�ȡ״̬�����ܱ��������̶�ȡ
//        fileStream.Flush();
//        fileStream.Close();
//        fileStream.Dispose();

//        Debug.Log($"{savePath}�ļ��������");
//    }

//    void CheckAssetPattern()
//    {
//        if (LoadPattern == AssetBundlePattern.Remote)
//        {
//            //StartCoroutine((ObjectAssetBundleName,LoadAssetBundle));
//        }
//        else
//        {
//            LoadAssetBundle();
//        }
//    }
//    /// <summary>
//    /// ����AB������
//    /// </summary>
//    void  LoadAssetBundle()
//    {
//        //ƴ�ӵļ��ش��·��
//        /*
//        string mainBundleName = "Bundles";
//        string assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);
//        */

//        string assetBundlePath = Path.Combine(AssetBundleLoadPath, MainAssetBundleName);
//        //�����嵥�����
//        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

//        //mainfest�ļ�����ʵ���������Ĵ�������ǿ����߿��Ĳ�������

//        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(assetBundleManifest));

//        foreach (string depAssetBundleName in assetBundleManifest.GetAllDependencies(ObjectAssetBundleName))
//        {
//            Debug.Log(depAssetBundleName);
//            assetBundlePath = Path.Combine(AssetBundleLoadPath, depAssetBundleName);
//            //�������Ҫʹ��������ʾ�������Բ��ñ��������ʾ�������Ǹ�ʾ�����Ǵ���������
//            AssetBundle.LoadFromFile(assetBundlePath);
//        }
//        //AB�����������м��ع���·�����·�������ط�ʽ����Unityά��
//        assetBundlePath = Path.Combine(AssetBundleLoadPath, "Cube");
//        CubeAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
//        assetBundlePath = Path.Combine(AssetBundleLoadPath, "Sphere");
//        SphereAssetBundle = AssetBundle.LoadFromFile(assetBundlePath);
//    }

//    /// <summary>
//    /// ������Դ����
//    /// </summary>
//    void LoadAsset()
//    {
//        if(SampleBundle==null)
//        {
//            Debug.Log("����AB��ʧ��");
//            return;
//        }
//        GameObject prefab = CubeAssetBundle.LoadAsset<GameObject>("Cube");
//        Instantiate(prefab);
//        prefab = SphereAssetBundle.LoadAsset<GameObject>("Sphere");
//        Instantiate(prefab);
//    }

//    void UnloadAssetBundle(bool isTrue)
//    {
//        Debug.Log(isTrue);
//        //��ǰ֡���ٶ���
//        DestroyImmediate(SampleObject);
//        //SampleBundle.Unload(isTrue);

//        //ʹ��unload��false����һ�������������ƾ��ǲ����ƻ���ǰ����ʱ��Ч���������ʲô��Դ������AB��������û�б���������Դ
//        //�Ա�ʹ�ö�AB��ʹ��unload��true��ж�ؾͻᵼ�µ�ǰ����ʱ��ʧĳЩ����ж��AB������Դ
//        //��ô����Ȼ�ģ��ڲ��ƻ�����ʱЧ��������£�Ҳ���ǵ���unload��false��������£���ʹ��resources.unloadUnusedAsset()����������Ч��ʱ��õ�
//        //��Ϊ���ж��ڴ�Ĳ�������ռ��CPU��ʹ�ã����������CPUʹ������ϵ͵�����½���ǿ�Ƶ���Դж��
//        //��������Ϸ�й����������߳�������ʱ
//        Resources.UnloadUnusedAssets();
//    }
//}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public enum AssetBundlePattern
{
    /// <summary>
    /// �༭��ģ����أ�Ӧ��ʹ��AssetDataBase������Դ���أ������ý��д��
    /// </summary>
    EditorSimulation,
    /// <summary>
    /// ���ؼ���ģʽ��Ӧ���������·����StreamingAssets·���£��Ӹ�·������
    /// </summary>
    Local,
    /// <summary>
    /// Զ�˼���ģʽ��Ӧ�����������Դ��������ַ��Ȼ��ͨ�������������
    /// ���ص�ɳ��·��persistentDataPath���ٽ��м���
    /// </summary>
    Remote
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
    public static string MainAssetBundleName = "SampleAssetBundle";

    /// <summary>
    /// ���������⣬ʵ�ʰ���������ȫ��Сд
    /// </summary>
    public static string ObjectAssetBundleName = "resourcesbundle";

    public string AssetBundleLoadPath;

    public string HTTPAddress = "http://10.24.4.179:8080/";

    public string HTTPAssetBundlePath;

    public string DownloadPath;
    void Start()
    {
        CheckAssetBundleLoadPath();
        LoadAssetBundleButton.onClick.AddListener(CheckAssetBundlePattern);
        LoadAssetButton.onClick.AddListener(LoadAsset);
        UnloadFalseButton.onClick.AddListener(() => { UnloadAssetBundle(false); });
        UnloadTrueButton.onClick.AddListener(() => { UnloadAssetBundle(true); });
    }

    void CheckAssetBundleLoadPath()
    {
        switch (LoadPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                AssetBundleLoadPath = Path.Combine(Application.streamingAssetsPath, MainAssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                HTTPAssetBundlePath = Path.Combine(HTTPAddress, MainAssetBundleName);
                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssetBundle");
                AssetBundleLoadPath = Path.Combine(DownloadPath, MainAssetBundleName);
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
            StartCoroutine(DownloadFile(ObjectAssetBundleName, LoadAssetBundle));
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

        string assetBundlePath = Path.Combine(AssetBundleLoadPath, MainAssetBundleName);
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

    void LoadAsset()
    {
        GameObject cubeObject = CubeBundle.LoadAsset<GameObject>("Cube");
        Instantiate(cubeObject);
        cubeObject = SphereBundle.LoadAsset<GameObject>("Sphere");
        Instantiate(cubeObject);
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
