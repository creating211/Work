using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//窗口渲染
public class AssetManagerEditorWindow : EditorWindow
{


    public string VersionString;

    public AssetManagerEditorWindowConfigSO WindowConfig;

    public void Awake()
    {
        AssetManagerEditor.LoadWindowConfig(this);
        AssetManagerEditor.LoadConfig(this);
    }

    /// <summary>
    /// 每当工程发生修改时会调用该方法
    /// </summary>
    private void OnValidate()
    {
        AssetManagerEditor.LoadWindowConfig(this);
        AssetManagerEditor.LoadConfig(this);
    }

    private void OnInspectorUpdate()
    {
        AssetManagerEditor.LoadWindowConfig(this);
        AssetManagerEditor.LoadConfig(this);
    }

    private void OnEnable()
    {
        AssetManagerEditor.AssetManagerConfig.GetCurrentDirectoryAllAssets();
    }
    public DefaultAsset editorWindowDirectory = null;
    /// <summary>
    /// 这个方法会在每个渲染帧调用，所以可以用来渲染UI界面
    /// 因为该方法运行在Editor类中，所以刷新频率取决于Editor的运行帧率
    /// </summary>
    private void OnGUI()
    {
        //默认情况下垂直排版
        //GUI按照代码顺序进行渲染
        GUILayout.Space(20);

        if (WindowConfig.LogoTexture != null)
        {
            GUILayout.Label(WindowConfig.LogoTexture, WindowConfig.LogoTextureStyle);
        }

        #region Title文字内容
        GUILayout.Space(20);
        GUILayout.Label(nameof(AssetManagerEditor), WindowConfig.TitleTextStyle);

        #endregion
        GUILayout.Space(20);
        GUILayout.Label(VersionString, WindowConfig.VersionTextStyle);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig.BuildingPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("打包模式",
            AssetManagerEditor.AssetManagerConfig.BuildingPattern);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig.CompressionPattern = (AssetBundleCompressionPattern)EditorGUILayout.EnumPopup("压缩格式",
            AssetManagerEditor.AssetManagerConfig.CompressionPattern);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig._IncrementalBuildMode = (IncrementalBuildMode)EditorGUILayout.EnumPopup("增量打包",
            AssetManagerEditor.AssetManagerConfig._IncrementalBuildMode);
        GUILayout.Space(20);
        editorWindowDirectory = EditorGUILayout.ObjectField(editorWindowDirectory, typeof
            (DefaultAsset), true) as DefaultAsset;
        if (AssetManagerEditor.AssetManagerConfig.AssetBundleDirectory != editorWindowDirectory)
        {
            if (editorWindowDirectory == null)
            {
                AssetManagerEditor.AssetManagerConfig.CurrentAllAssets.Clear();
            }
            AssetManagerEditor.AssetManagerConfig.AssetBundleDirectory = editorWindowDirectory;
            AssetManagerEditor.AssetManagerConfig.GetCurrentDirectoryAllAssets();
        }
        if (AssetManagerEditor.AssetManagerConfig.CurrentAllAssets != null && AssetManagerEditor.AssetManagerConfig.CurrentAllAssets.Count > 0)
        {
            for (int i = 0; i < AssetManagerEditor.AssetManagerConfig.CurrentAllAssets.Count; i++)
            {
                AssetManagerEditor.AssetManagerConfig.CurrentSelectedAssets[i] = EditorGUILayout.ToggleLeft(AssetManagerEditor.AssetManagerConfig.CurrentAllAssets[i], AssetManagerEditor.AssetManagerConfig.CurrentSelectedAssets[i]);
            }
        }

        GUILayout.Space(20);
        if (GUILayout.Button("打包AssetBundle"))
        {
            AssetManagerEditor.BuildAssetBundleFromDirectedGraph();
            Debug.Log("EditorButton按下");
        }

        GUILayout.Space(20);
        if (GUILayout.Button("保存Config"))
        {
            AssetManagerEditor.SaveConfigToJson();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("读取Config文件"))
        {
            AssetManagerEditor.LoadCongifFromJson();
        }
    }
}
