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
    /// �༭��ģ�����Ӧ��ʹ��AssetDataPath������Դ���أ������ý��д��
    /// </summary>
    EditorSimulation,
    /// <summary>
    /// ���ؼ���ģʽ��Ӧ���������·������StreamingAssets·���£��Ӹ�·������
    /// </summary>
    Local,
    /// <summary>
    /// Զ�˼���ģʽ��Ӧ������������Դ��������ַ��Ȼ��ͨ������������ص�ɳ��·��persistenceDataPath���ٽ��м���
    /// </summary>
    Remote
}
/// <summary>
/// ��Ϊ�����ű��ᱻ���뵽AssetmblyCharp.dll��
/// �����Ű�������ȥ����APK�����в�����ʹ������Unity Editor�����ؼ��µķ���
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
    /// ����İ�������Ӧ����Editor���������Ϊ������Ҳ��Ҫ���ʣ����Է�����Դ��������
    /// </summary>
    public static string MainAssetBundleName = "SampleAssetBundle";//������
    /// <summary>
    /// ���������⣬����������ҪСд
    /// </summary>
    public static string ObjectAssetBundleName = "resourcesbundle";//�ΰ���

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
        /*�����ֺ���ʱ
        //ͨ���ⲿ·������AB���ķ�ʽ
        //��ΪpersisitenceDataPath���ƶ��˿ɶ���д�����ԣ�Զ�����ص�AB�������Է����ڸ�·����
        //string assetBundlePath = Path.Combine(Application.persistentDataPath, "Bundles", "prefab");
        //string assetBundlePath= Path.Combine(Application.dataPath, "Bundles","prefab");
        //������ʶ�ʧ

        string mainBundleName = "Bundles";
        string assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);
        //�����嵥�����
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //mainfest�ļ�����ʵ���������Ĵ�������ǿ����߿��Ĳ�������

        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(assetBundleManifest));

        foreach(string depAssetBundleName in assetBundleManifest.GetAllDependencies("prefab"))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, depAssetBundleName);
            //�������Ҫʹ��������ʾ�������Բ��ñ��������ʾ�������Ǹ�ʾ�����Ǵ���������
            AssetBundle.LoadFromFile(assetBundlePath);
        }
        //AB�����������м��ع���·�����·�������ط�ʽ����Unityά��
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
    /// ����һ��Я��
    /// </summary>
    /// <returns></returns>
    /// IEnumerator DownloadAssetBundle()����������������
    /// IEnumerator DownloadFile(string fileName,bool isSavFile)���������ļ�

    IEnumerator DownloadAssetBundle()
    {
        string remotePath = Path.Combine(HTTPAddress, MainAssetBundleName);
        string mainBundleDownPath = Path.Combine(remotePath, MainAssetBundleName);

        UnityWebRequest webRequest = UnityWebRequest.Get(mainBundleDownPath);

        webRequest.SendWebRequest();
        
        while(!webRequest.isDone)
        {
            //ÿ�ζ���ӡ�������ֽ���
            Debug.Log(webRequest.downloadedBytes);
            //���ؽ���
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
        Debug.Log(webRequest.downloadHandler.data.Length);//�����ص��ֽ�����ӡ����
        yield return SaveFile(mainBundleSavePath, webRequest.downloadHandler.data);
    }

    IEnumerator SaveFile(string savePath,byte[] bytes)
    {
        FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate); 
        yield return fileStream.WriteAsync(bytes,0,bytes.Length);

        //�ͷ��ļ����������ļ���һֱ���ڶ�ȡ״̬�����ܱ��������̶�ȡ
        fileStream.Flush();
        fileStream.Close();
        fileStream.Dispose();

        Debug.Log($"{savePath}�ļ��������");
    }
    /// <summary>
    /// ����AB������
    /// </summary>
    void  LoadAssetBundle()
    {
        if(LoadPattern==AssetBundlePattern.Remote)
        {
            StartCoroutine(DownloadAssetBundle());
        }

        //ƴ�ӵļ��ش��·��
        /*
        string mainBundleName = "Bundles";
        string assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);
        */

        string assetBundlePath = Path.Combine(AssetBundleLoadPath, MainAssetBundleName);
        //�����嵥�����
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //mainfest�ļ�����ʵ���������Ĵ�������ǿ����߿��Ĳ�������

        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(assetBundleManifest));

        foreach (string depAssetBundleName in assetBundleManifest.GetAllDependencies(ObjectAssetBundleName))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(AssetBundleLoadPath, depAssetBundleName);
            //�������Ҫʹ��������ʾ�������Բ��ñ��������ʾ�������Ǹ�ʾ�����Ǵ���������
            AssetBundle.LoadFromFile(assetBundlePath);
        }
        //AB�����������м��ع���·�����·�������ط�ʽ����Unityά��
        assetBundlePath = Path.Combine(AssetBundleLoadPath, ObjectAssetBundleName);
        SampleBundle = AssetBundle.LoadFromFile(assetBundlePath);
    }

    /// <summary>
    /// ������Դ����
    /// </summary>
    void LoadAsset()
    {
        GameObject cubeObject = SampleBundle.LoadAsset<GameObject>("Cube");
        SampleObject=Instantiate(cubeObject);
    }

    void UnloadAssetBundle(bool isTrue)
    {
        Debug.Log(isTrue);
        //��ǰ֡���ٶ���
        DestroyImmediate(SampleObject);
        SampleBundle.Unload(isTrue);

        //ʹ��unload��false����һ�������������ƾ��ǲ����ƻ���ǰ����ʱ��Ч���������ʲô��Դ������AB��������û�б���������Դ
        //�Ա�ʹ�ö�AB��ʹ��unload��true��ж�ؾͻᵼ�µ�ǰ����ʱ��ʧĳЩ����ж��AB������Դ
        //��ô����Ȼ�ģ��ڲ��ƻ�����ʱЧ��������£�Ҳ���ǵ���unload��false��������£���ʹ��resources.unloadUnusedAsset()����������Ч��ʱ��õ�
        //��Ϊ���ж��ڴ�Ĳ�������ռ��CPU��ʹ�ã����������CPUʹ������ϵ͵�����½���ǿ�Ƶ���Դж��
        //��������Ϸ�й����������߳�������ʱ
        Resources.UnloadUnusedAssets();
    }
}
