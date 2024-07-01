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

        //设置图片
        LogoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/background.jpg");
        LogoTextureStyle = new GUIStyle();
        LogoTextureStyle.alignment = TextAnchor.MiddleCenter;
    }
    /// <summary>
    /// 这个方法会在每个渲染帧调用，所以可以用来渲染Ui界面
    /// 因为该方法运行在Editor中，所以刷新频率取决于Editor的运行频率
    /// </summary>
    private void OnGUI()
    {
        //默认情况下是垂直排版
        //GUI按照代码顺序进行渲染
        GUILayout.Space(20);
        if (LogoTexture != null)
        {
            GUILayout.Label(LogoTexture, LogoTextureStyle);
        }

        #region Title文字内容
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
        AssetManagerEditor.BuilidingPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("打包模式", AssetManagerEditor.BuilidingPattern);

        GUILayout.Space(20);
        AssetManagerEditor.CompressionPattern = (AssetBundleCompresionPattern)EditorGUILayout.EnumPopup("压缩格式", AssetManagerEditor.CompressionPattern);

        GUILayout.Space(20);
        AssetManagerEditor.AssetBundleDirectory = EditorGUILayout.ObjectField(AssetManagerEditor.AssetBundleDirectory, typeof(DefaultAsset), true) as DefaultAsset;

        GUILayout.Space(20);
        if (GUILayout.Button("打包AssetBundle"))
        {
            //当写了BuildAssetBundleFromDirectory，打包指定文件夹下的所有资源为AssetBundle时
            AssetManagerEditor.BuildAssetBundleFromDirectory();

            /*
            string directoryPath = AssetDatabase.GetAssetPath((AssetManagerEditor.AssetBundleDirectory));
            AssetManagerEditor.FindAllAssetNameFromDirectory(directoryPath);
            */
            Debug.Log("EditorButton按下");
        }
    }
}

