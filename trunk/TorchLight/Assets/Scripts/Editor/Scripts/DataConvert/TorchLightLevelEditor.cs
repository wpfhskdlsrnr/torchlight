using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TorchLightLevelEditor :  EditorWindow{

    static private TorchLightLevelEditor TLEditorWindow = null;

    [MenuItem("TorchLight/LevelEditor")]
	public static void Execute()
    {
        if (TLEditorWindow == null)
        {
            TLEditorWindow = CreateInstance<TorchLightLevelEditor>();
            TLEditorWindow.minSize = new Vector2(410, 400);
            TLEditorWindow.title = "Level Editor";
        }

        TLEditorWindow.Show();
    }

    private string LayoutFile = "";
    private Vector2 ScrollPosition;
    private string LogContent = "";
    private string StateContent = "";
    void OnGUI()
    {
        bool ButtonPress = false;
        GUILayout.Label("Resource : " + LayoutFile);

        ButtonPress = GUILayout.Button("Convert TorchLight Level Resource");
        ProcessConvertTorchLightLevelResource(ButtonPress);

        GUILayout.Label("Convert Level Layout Into Scene");
        GUILayout.BeginHorizontal();
        ButtonPress = GUILayout.Button("Open Level Layout", GUILayout.Width(200));
        ProcessOpenFile(ButtonPress, "Select Level Layout File", "Layout");

        ButtonPress = GUILayout.Button("Load To Scene", GUILayout.Width(200));
        ProcessLoadToScene(ButtonPress);
        GUILayout.EndHorizontal();

        GUILayout.Label("----------------------------------------------");
        GUILayout.BeginHorizontal();
        ButtonPress = GUILayout.Button("TEST Open Level Rules", GUILayout.Width(200));
        ProcessOpenFile(ButtonPress, "TEST Select Level Rules", "txt");

        ButtonPress = GUILayout.Button("TEST Build Level", GUILayout.Width(200));
        ProcessBuildLevel(ButtonPress);
        GUILayout.EndHorizontal();

        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition,  GUILayout.Height(position.width - 20.0f), 
                                                                    GUILayout.Height(position.height - 90.0f));
        GUILayout.Label(LogContent);
        GUILayout.EndScrollView();

        GUILayout.Label("State : " + StateContent);
    }

    void ProcessBuildLevel(bool Press)
    {
        if (Press && LayoutFile.Length > 0)
        {
            TorchLightLevelRandomGenerater Loader = new TorchLightLevelRandomGenerater();
            Loader.LoadLevelRuleFileToScene(LayoutFile);
        }
    }

    void ProcessConvertTorchLightLevelResource(bool Press)
    {
        if (Press)
        {
            // Convert TL LevelSet Items
            TorchLightLevelLayoutSetConvert.ParseLevelSet();
            // Convert TL LevelLayout Rule Files
            TorchLightLevelLayoutRuleConvert.ConvertLevelRuleFile();
            StateContent = "TorchLight Level Resource Convert Finished.";
        }
    }

    void ProcessOpenFile(bool Press, string Description, string Suffix)
    {
        if (Press)
        {
            EditorGUIUtil.OpenFile(Description, Suffix, ref LayoutFile);
        }
    }

    void ProcessOpenFolder(bool Press, string Description)
    {
        if (Press)
        {
            EditorGUIUtil.OpenFolder(Description, ref LayoutFile);
        }
    }
    
    void ProcessLoadToScene(bool Press)
    {
        if (Press && LayoutFile.Length > 0)
        {
            EditorApplication.NewScene();
            TorchLightLevelBuilder.LoadLevelLayoutToScene(LayoutFile);
        }
    }
}
