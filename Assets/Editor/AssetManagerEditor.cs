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
/// ����EditorĿ¼�µ�C#�ű������ᱻ�������ִ���ļ���
/// </summary>

public class AssetManagerEditor : MonoBehaviour
{
    public static string AssetManagerVersion = "1.0.0";

    /// <summary>
    /// �༭��ģ���£������д��
    /// ����ģʽ�£������StreamingAssets
    /// Զ��ģʽ�����������Զ��·�����ڸ�ʾ����ΪpersistenceDataPath
    /// </summary>
    public static AssetBundlePattern BuilidingPattern;

    public static AssetBundleCompresionPattern CompressionPattern;

    /// <summary>
    /// ��Ҫ������ļ���
    /// </summary>
    public static DefaultAsset AssetBundleDirectory;

    //������Դ������helloworld��public static string MainAssetBundleName = "SampleAssetBundle";

    //public static string AssetBundleOutputPath = Path.Combine(Application.persistentDataPath, MainAssetBundleName);

    //��ʹ����AssetBundlePattern��ʽʱ��Ĭ������´��·����û��ֵ��
    public static string AssetBundleOutputPath ;

    //public const string AssetManagerName = nameof(AssetManager);
    /// <summary>
    /// ͨ��MenuItem���ԣ�����Editor�����˵���ѡ��
    /// </summary>
    [MenuItem(nameof(AssetManagerEditor)+"/"+nameof(BuildAssetBundle))]
    static void BuildAssetBundle()

    {
        CheckBuildOutputPath();
        //�����persistenceDataPath������ڹ�������ļ��п�����
        //Debug.Log(Application.persistentDataPath);�鿴������ĸ�·������
        //string outputPath = Path.Combine(Application.persistentDataPath, "Bundles");

        if(!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }

        //��ͬƽ̨֮���Asset Bundle������ͨ��
        //�÷����������������������˰�����AB��
        //optionΪnoneʱʹ��LZMAѹ��
        //UncompressedAssetBundle������ѹ��
        //ChunkBasedCompression����LZ4���п�ѹ��
        //���·����ѹ���ŷ���ʲôϵͳactiveBuildTarget��������Ӧ

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
        Debug.Log("AB��������");
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
        //����ֱ����getwindow����,�����Զ����С
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
    /// ���ָ���ļ����µ�������ԴΪAsset Bundle
    /// </summary>
    public static void BuildAssetBundleFromDirectory()
    {
        CheckBuildOutputPath();
        if (AssetBundleDirectory==null)
        {
            Debug.Log("���Ŀ¼������");
            return;
        }
        string directoryPath = AssetDatabase.GetAssetPath((AssetManagerEditor.AssetBundleDirectory));
        string[] assetPath=AssetManagerEditor.FindAllAssetNameFromDirectory(directoryPath).ToArray();

        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[1];

        //��Ҫ����ľ�����������������
        assetBundleBuilds[0].assetBundleName = HelloWorld.ObjectAssetBundleName;

        //������Ȼ��ΪName��ʵ������Ҫ��Դ�ڹ����µ�·��
        assetBundleBuilds[0].assetNames = assetPath;

        if(string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("���·��Ϊ��");
            return;
        }
        else if(!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }
        
        //Unity��inspector������õ�assetbundle��Ϣ����ʵ����һ��Assetbundle�ṹ��
        //unityֻ�����Ǳ����������ļ��У������������ļ��������õ�AssetBundleBuild�ռ��������
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckCompressionPattern(), BuildTarget.StandaloneWindows);

        Debug.Log(AssetBundleOutputPath);

        //ˢ��project���棬������Ǵ��������������Ҫִ��
        AssetDatabase.Refresh();

    }
    public static List<string> FindAllAssetNameFromDirectory(string directoryPath)
    {
        List<string> assetPaths = new List<string>();

        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            Debug.Log("�ļ���·��������");
            return null;
        }

        //System.IO�෽����Ҳ����Windows�Դ��Ķ��ļ��н��в�������
        //System.IO�µ��ֻ࣬����Windows����pcƽ̨�϶�д�ļ������ƶ��˲�����
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        //��ȡ��Ŀ¼�������ļ���Ϣ
        //Directory�ļ��в�����File���ͣ��������ﲻ���ȡ���ļ���
        FileInfo[] fileInfos = directoryInfo.GetFiles();

        //���еķ�Ԫ�����ļ�·������ӵ��б������ڴ����Щ�ļ�
        foreach(FileInfo info in fileInfos)
        {
            if(info.Extension.Contains(".meta"))
            {
                continue;
            }
            //Asset���ֻ��Ҫ�ļ���
            string assetPath = Path.Combine(directoryPath,info.Name);
            assetPaths.Add(assetPath);
            Debug.Log(assetPath);
        }

        return assetPaths;
    }
}
