using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetManagerEditorWindow : EditorWindow
{

    public static GUIStyle TitleTextStyle;

    public static GUIStyle VersionTextStyle;

    public static Texture2D LogoTexture;

    public static GUIStyle LogoTextureStyle;

    public void Awake()
    {
        TitleTextStyle = new GUIStyle();
        TitleTextStyle.fontSize = 26;
        TitleTextStyle.normal.textColor = Color.red;
        TitleTextStyle.alignment = TextAnchor.MiddleCenter;

        VersionTextStyle = new GUIStyle();
        VersionTextStyle.fontSize = 20;
        VersionTextStyle.normal.textColor = Color.cyan;
        VersionTextStyle.alignment = TextAnchor.MiddleRight;

        //����ͼƬ
        LogoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/background.jpg");
        LogoTextureStyle = new GUIStyle();
        LogoTextureStyle.alignment = TextAnchor.MiddleCenter;
    }
    /// <summary>
    /// �����������ÿ����Ⱦ֡���ã����Կ���������ȾUi����
    /// ��Ϊ�÷���������Editor�У�����ˢ��Ƶ��ȡ����Editor������Ƶ��
    /// </summary>
    private void OnGUI()
    {
        //Ĭ��������Ǵ�ֱ�Ű�
        //GUI���մ���˳�������Ⱦ
        GUILayout.Space(20);
        if (LogoTexture != null)
        {
            GUILayout.Label(LogoTexture, LogoTextureStyle);
        }

        #region Title��������
        GUILayout.Space(20);
        GUILayout.Label(nameof(AssetManagerEditor), TitleTextStyle);
        /*
        TitleTextStyle = new GUIStyle();
        TitleTextStyle.fontSize = 26;
        TitleTextStyle.normal.textColor = Color.red;
        TitleTextStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label(nameof(AssetManagerEditor), TitleTextStyle);
        */

        #endregion
        GUILayout.Space(20);
        GUILayout.Label(AssetManagerEditor.AssetManagerVersion, VersionTextStyle);

        GUILayout.Space(20);
        AssetManagerEditor.BuilidingPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("���ģʽ", AssetManagerEditor.BuilidingPattern);

        GUILayout.Space(20);
        AssetManagerEditor.CompressionPattern = (AssetBundleCompresionPattern)EditorGUILayout.EnumPopup("ѹ����ʽ", AssetManagerEditor.CompressionPattern);

        GUILayout.Space(20);
        AssetManagerEditor.AssetBundleDirectory = EditorGUILayout.ObjectField(AssetManagerEditor.AssetBundleDirectory, typeof(DefaultAsset), true) as DefaultAsset;

        GUILayout.Space(20);
        if (GUILayout.Button("���AssetBundle"))
        {
            //��д��BuildAssetBundleFromDirectory�����ָ���ļ����µ�������ԴΪAssetBundleʱ
            AssetManagerEditor.BuildAssetBundleFromDirectory();

            /*
            string directoryPath = AssetDatabase.GetAssetPath((AssetManagerEditor.AssetBundleDirectory));
            AssetManagerEditor.FindAllAssetNameFromDirectory(directoryPath);
            */
            Debug.Log("EditorButton����");
        }
    }
}

