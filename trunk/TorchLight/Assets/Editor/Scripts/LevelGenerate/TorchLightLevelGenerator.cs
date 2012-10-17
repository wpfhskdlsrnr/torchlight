using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

class FStrata
{
    public string RuleSet = "";
    public int MonsterLevelMin = 0;
    public int MonsterLevelMax = 0;

    public string   RuleSetName = "Unkown";
    public string   BackgroundMusic = "";
    public Vector3  LightDir = Vector3.zero;
    public Color    LightColor = Color.white;
    public Color    AmbientColot = Color.white;
    public Color    FogColor = Color.white;
    public float    FogStart = 0.0f;
    public float    FogEnd   = 1.0f;

    public FStrata(string InRuleSet)
    {
        RuleSet = TorchLightConfig.TorchLightAssetFolder + InRuleSet + ".txt";
        LoadRuleSet();
    }

    void LoadRuleSet()
    {
        try
        {
            StreamReader Reader = new StreamReader(RuleSet);
            while(!Reader.EndOfStream)
            {
                string Line = Reader.ReadLine();
                string Tag = "", Value = "";

                TorchLightTools.ParseTag(Line, ref Tag, ref Value);
                if (Tag == "LEVELNAME")
                    RuleSetName = Value;
                else if (Tag == "AMBIENT")
                    AmbientColot = TorchLightTools.ParseColor(Value);
                else if (Tag == "BGMUSIC")
                    BackgroundMusic = Value;
                else if (Tag == "DIRECTION_COLOR")
                    LightColor = TorchLightTools.ParseColor(Value);
                else if (Tag == "DIRECTION_DIR")
                    LightDir = TorchLightTools.ParseVector3(Value);
                else if (Tag == "FOG_COLOR")
                    FogColor = TorchLightTools.ParseColor(Value);
                else if (Tag == "FOG_BEGIN")
                    FogStart = float.Parse(Value);
                else if (Tag == "FOG_END")
                    FogEnd = float.Parse(Value);
            }
            Reader.Close();
        }
        catch (System.Exception)
        {
        	Debug.LogError("Load RuleSet Failed : " + RuleSet);
        }
    }
}

class FDungeon
{
    public string   FilePath        = "";

    public string   Name            = "Default Dungeon";
    public string   DisplayName     = "";
    public string   ParentDungeon   = "";
    public int      PlayerLevelMin  = 0;
    public int      PlayerLevelMax  = 0;

    public List<FStrata> Startas = new List<FStrata>();

    static string GetRelativePath(string Path)
    {
        return Path.Substring(Path.IndexOf("Assets", StringComparison.CurrentCultureIgnoreCase));
    }

    public static List<FDungeon> LoadAllDungeons()
    {
        List<FDungeon> Dungeons = new List<FDungeon>();
        {
            List<string> Files = TorchLightTools.GetAllFileInFolderFullPath(TorchLightConfig.TorchLightDungeonFolder, "dat");
            foreach(string AFile in Files)
            {
                try
                {
                    FStrata CurStrata   = null;
                    FDungeon Dungeon    = new FDungeon();
                    Dungeon.FilePath    = GetRelativePath(AFile);
                    StreamReader Reader = new StreamReader(AFile);

                    while(!Reader.EndOfStream)
                    {
                        string Line = Reader.ReadLine();
                        string Tag = "", Value = "";
                        TorchLightTools.ParseLine(Line, ref Tag, ref Value);

                        if (Tag == "NAME")
                            Dungeon.Name = Value;
                        else if (Tag == "DISPLAYNAME")
                            Dungeon.DisplayName = Value;
                        else if (Tag == "PARENT_DUNGEON")
                            Dungeon.ParentDungeon = Value;
                        else if (Tag == "PLAYER_LVL_MATCH_MIN")
                            Dungeon.PlayerLevelMin = int.Parse("0" + Value);
                        else if (Tag == "PLAYER_LVL_MATCH_MAX")
                            Dungeon.PlayerLevelMax = int.Parse("0" + Value);
                        else if (Tag == "RULESET")
                        {
                            CurStrata = new FStrata(Value);
                            Dungeon.Startas.Add(CurStrata);
                        }
                        else if (Tag == "MONSTER_LVL_MIN")
                        {
                            if (CurStrata != null) CurStrata.MonsterLevelMin = int.Parse("0" + Value);
                        }
                        else if (Tag == "MONSTER_LVL_MAX")
                        {
                            if (CurStrata != null) CurStrata.MonsterLevelMax = int.Parse("0" + Value);
                        }
                    }
                    Reader.Close();

                    Dungeons.Add(Dungeon);
                }
                catch (System.Exception)
                {
                    Debug.LogError("Dungeon Load Failed : " + AFile);
                }
             }
        }
        return Dungeons;
    }
}

public class TorchLightLevelGenerator : EditorWindow {

    [MenuItem("TorchLight/LevelGenerator")]
    static void Execute()
    {
        TorchLightLevelGenerator LevelGenerator = CreateInstance<TorchLightLevelGenerator>();
        LevelGenerator.title = "Level Generator";
        LevelGenerator.Init();
        LevelGenerator.Show();
    }

    List<FDungeon> Dungeons = null;
    void Init()
    {
        Dungeons = FDungeon.LoadAllDungeons();
        GetBackgroundTexture();
    }

    static Texture2D BackgroundTexture = null;
    static Texture2D GetBackgroundTexture()
    {
        if (BackgroundTexture == null)
            BackgroundTexture = AssetDatabase.LoadAssetAtPath(TorchLightConfig.TorchLightLogoIconPath, typeof(Texture2D)) as Texture2D;
        return BackgroundTexture;
    }

    Rect PropertyWindowRect;
    void OnGUI()
    {
        float DungeonWindowWidth    = 340.0f;
        float StrataWindowWidth     = 230.0f;
        float WindowY               = 24.0f;
        float WindowWidth = position.width, windowHeight = position.height - WindowY;

        bool ButtonPress = false;
        GUILayout.BeginHorizontal();
        ButtonPress = GUILayout.Button("Convert TorchLight Resource", GUILayout.Width(200));
        ProcessConvertTorchLightResource(ButtonPress);
        ButtonPress = GUILayout.Button("Reload Dungeons", GUILayout.Width(200));
        ProcessReloadDungeons(ButtonPress);
        GUILayout.EndHorizontal();
        
        BeginWindows();
        GUI.Window(0, new Rect(0, WindowY, DungeonWindowWidth, windowHeight), DoDrawDungeonListWindow, "", GUI.skin.box);
        GUI.Window(1, new Rect(DungeonWindowWidth +1, WindowY, StrataWindowWidth, windowHeight), DoDrawRuleListWindow, "", GUI.skin.box);
        PropertyWindowRect = GUI.Window(2, new Rect(StrataWindowWidth + DungeonWindowWidth + 2, WindowY, WindowWidth - StrataWindowWidth - DungeonWindowWidth - 3, windowHeight), DoDrawPropertyWindow, "", GUI.skin.box);
        EndWindows();
    }

    void ProcessConvertTorchLightResource(bool ButtonPress)
    {
        if (ButtonPress)
        {
            // Convert TL LevelSet Items
            TorchLightLevelLayoutSetConvert.ParseLevelSet();
            // Convert TL LevelLayout Rule Files
            TorchLightLevelLayoutRuleConvert.ConvertLevelRuleFile();
            Debug.Log("TorchLight Level Resource Convert Finished.");
        }
    }

    void ProcessReloadDungeons(bool ButtonPress)
    {
        if (ButtonPress)
        {
            Init();
        }
    }

    FDungeon CurSelectDungeon = null;
    Vector2 DungeonScrollPosition = Vector2.zero;
    void DoDrawDungeonListWindow(int WindowID)
    {
        if (Dungeons == null)
            return;

        DungeonScrollPosition = GUILayout.BeginScrollView(DungeonScrollPosition);
        {
            GUILayout.BeginVertical();
            foreach(FDungeon Dungeon in Dungeons)
            {
                if (Dungeon == CurSelectDungeon)
                    GUI.color = Color.green;

                bool ButtonPress = GUILayout.Button(Dungeon.FilePath, GUI.skin.label);
                ProcessSelectDungeon(ButtonPress, Dungeon);
                GUI.color = Color.white;
            }
            GUILayout.EndVertical();
        }      
        GUILayout.EndScrollView();
    }

    void ProcessSelectDungeon(bool ButtonPress, FDungeon CurDungeon)
    {
        if (ButtonPress)
        {
            if (CurSelectDungeon != CurDungeon)
                CurSelectStrata     = null;

            CurSelectDungeon    = CurDungeon;
        }
    }

    FStrata CurSelectStrata = null;
    Vector2 RuleListScrollPosition = Vector2.zero;
    void DoDrawRuleListWindow(int WindowID)
    {
        float WindowWidth = position.width, windowHeight = position.height;

        if (CurSelectDungeon == null)
            return;

        GUILayout.BeginVertical();
        {
            GUILayout.Label("Name", GUILayout.Width(100));
            CurSelectDungeon.Name = GUILayout.TextField(CurSelectDungeon.Name);
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        {
            GUILayout.Label("Display Name", GUILayout.Width(100));
            CurSelectDungeon.DisplayName = GUILayout.TextField(CurSelectDungeon.DisplayName);
        }
        GUILayout.EndVertical();

        GUILayout.Space(5);
        RuleListScrollPosition = GUILayout.BeginScrollView(RuleListScrollPosition);
        {
            if (CurSelectDungeon != null)
            {
                int Num = 0;
                foreach(FStrata Strata in CurSelectDungeon.Startas)
                {
                    if (Strata == CurSelectStrata)
                        GUI.color = Color.green;

                    bool ButtonPress = GUILayout.Button("Starta" + Num + "  (" + Strata.RuleSetName + ")", GUI.skin.label);
                    ProcessSelectStrata(ButtonPress, Strata);
                    Num++;

                    GUI.color = Color.white;
                }
            }           
        }
        GUILayout.EndScrollView();
        GUILayout.Space(5);
    }

    void ProcessSelectStrata(bool PressButton, FStrata Strata)
    {
        if (PressButton)
        {
            CurSelectStrata = Strata;
        }
    }

    void DoDrawPropertyWindow(int WindowID)
    {
        float WindowWidth = PropertyWindowRect.width, windowHeight = PropertyWindowRect.height;
        if (BackgroundTexture != null)
            GUI.DrawTexture(new Rect(WindowWidth - 125, windowHeight - 140, 128, 128), BackgroundTexture);

        if (CurSelectStrata == null)
            return;

        EditorGUIUtility.LookLikeControls();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Parent Dungeon", GUILayout.Width(150));
            CurSelectDungeon.ParentDungeon = GUILayout.TextField(CurSelectDungeon.ParentDungeon, GUILayout.Width(100));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Player Level Match", GUILayout.Width(150));
            CurSelectDungeon.PlayerLevelMin = EditorGUILayout.IntField(CurSelectDungeon.PlayerLevelMin, GUILayout.Width(100));
            GUILayout.Label("--", GUILayout.Width(10));
            CurSelectDungeon.PlayerLevelMax = EditorGUILayout.IntField(CurSelectDungeon.PlayerLevelMax, GUILayout.Width(100));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Monster Level Match", GUILayout.Width(150));
            CurSelectStrata.MonsterLevelMin = EditorGUILayout.IntField(CurSelectStrata.MonsterLevelMin, GUILayout.Width(100));
            GUILayout.Label("--", GUILayout.Width(10));
            CurSelectStrata.MonsterLevelMax = EditorGUILayout.IntField(CurSelectStrata.MonsterLevelMax, GUILayout.Width(100));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        {
            //string [] Rules = new string []{"11111", "22222"};
            GUILayout.Label("Rule Set", GUILayout.Width(150));
            CurSelectStrata.RuleSet = GUILayout.TextField(CurSelectStrata.RuleSet);
            //EditorGUILayout.Popup(0, Rules);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(15);
        GUILayout.BeginVertical("box");
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Background Music", GUILayout.Width(145));
                CurSelectStrata.BackgroundMusic = GUILayout.TextField(CurSelectStrata.BackgroundMusic);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                CurSelectStrata.LightColor = EditorGUILayout.ColorField("DirLight Color", CurSelectStrata.LightColor, GUILayout.Width(300));
                GUILayout.Space(10);
                CurSelectStrata.LightDir = EditorGUILayout.Vector3Field("Light Direction", CurSelectStrata.LightDir, GUILayout.Width(335));
            }
            GUILayout.EndHorizontal();

            CurSelectStrata.AmbientColot = EditorGUILayout.ColorField("Ambient Color", CurSelectStrata.AmbientColot, GUILayout.Width(300));

            GUILayout.BeginHorizontal();
            {
                CurSelectStrata.FogColor = EditorGUILayout.ColorField("Fog Color", CurSelectStrata.FogColor, GUILayout.Width(300));
                GUILayout.Space(10);
                GUILayout.Label("Fog : Start", GUILayout.Width(75));
                CurSelectStrata.FogStart = EditorGUILayout.FloatField("", CurSelectStrata.FogStart, GUILayout.Width(100));
                GUILayout.Label(" - End", GUILayout.Width(50));
                CurSelectStrata.FogEnd = EditorGUILayout.FloatField("", CurSelectStrata.FogEnd, GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        GUILayout.EndVertical();

        EditorGUIUtility.LookLikeInspector();

        GUILayout.Space(15);
        GUILayout.BeginHorizontal();
        {
            CreateNewScene  = GUILayout.Toggle(false, "Create New Scene", GUILayout.Width(150));
            SaveAfterCreate = GUILayout.Toggle(SaveAfterCreate, "Save After Create", GUILayout.Width(150));
        }
        GUILayout.EndHorizontal();
        bool ButtonPress = false;
        ButtonPress = GUILayout.Button("Generate Level", GUILayout.Width(150));
        ProcessGenerateLevel(ButtonPress);
    }

    bool CreateNewScene  = true;
    bool SaveAfterCreate = true;
    void ProcessGenerateLevel(bool ButtonPress)
    {
        if (ButtonPress)
        {
            if (CreateNewScene)
                EditorApplication.NewScene();

            TorchLightLevelRandomGenerater Loader = new TorchLightLevelRandomGenerater();
            Loader.LoadLevelRuleFileToScene(CurSelectStrata.RuleSet);

            if (SaveAfterCreate)
                EditorApplication.SaveScene(TorchLightConfig.TorchLightSceneFolder + "temp.unity");
        }
    }
}
