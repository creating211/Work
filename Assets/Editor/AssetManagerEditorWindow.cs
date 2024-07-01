using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//������Ⱦ
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
    /// ÿ�����̷����޸�ʱ����ø÷���
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
    /// �����������ÿ����Ⱦ֡���ã����Կ���������ȾUI����
    /// ��Ϊ�÷���������Editor���У�����ˢ��Ƶ��ȡ����Editor������֡��
    /// </summary>
    private void OnGUI()
    {
        //Ĭ������´�ֱ�Ű�
        //GUI���մ���˳�������Ⱦ
        GUILayout.Space(20);

        if (WindowConfig.LogoTexture != null)
        {
            GUILayout.Label(WindowConfig.LogoTexture, WindowConfig.LogoTextureStyle);
        }

        #region Title��������
        GUILayout.Space(20);
        GUILayout.Label(nameof(AssetManagerEditor), WindowConfig.TitleTextStyle);

        #endregion
        GUILayout.Space(20);
        GUILayout.Label(VersionString, WindowConfig.VersionTextStyle);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig.BuildingPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("���ģʽ",
            AssetManagerEditor.AssetManagerConfig.BuildingPattern);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig.CompressionPattern = (AssetBundleCompressionPattern)EditorGUILayout.EnumPopup("ѹ����ʽ",
            AssetManagerEditor.AssetManagerConfig.CompressionPattern);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig._IncrementalBuildMode = (IncrementalBuildMode)EditorGUILayout.EnumPopup("�������",
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
        if (GUILayout.Button("���AssetBundle"))
        {
            AssetManagerEditor.BuildAssetBundleFromDirectedGraph();
            Debug.Log("EditorButton����");
        }

        GUILayout.Space(20);
        if (GUILayout.Button("����Config"))
        {
            AssetManagerEditor.SaveConfigToJson();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("��ȡConfig�ļ�"))
        {
            AssetManagerEditor.LoadCongifFromJson();
        }
    }
}
