using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public class TorchLightLevelLayoutRuleConvert
{
    static string GetScenePath(string Path)
    {
        Path            = Path.Replace('\\', '/');
        string APath    = Path.Substring(Path.IndexOf("layouts/", StringComparison.CurrentCultureIgnoreCase) + 8);
        return APath.Substring(0, APath.LastIndexOf('/'));
    }

    static string GetMusicPath(string Path)
    {
        if (Path == null)
            return "";

        Path            = Path.Replace('\\', '/');
        Path            = Path.Substring(Path.IndexOf("music/", StringComparison.CurrentCultureIgnoreCase));     
        return TorchLightConfig.TorchLightEditorResourceFolder + Path;
    }

    static string LEVEL_CHUNK           = "[LEVEL]";
    static string LEVEL_SUB_CHUNK_BEGIN = "[CHUNKTYPE]";
    static string LEVEL_SUB_CHUNK_END   = "[/CHUNKTYPE]";

    public static void ConvertLevelRuleFile()
    {
        string RuleFolder = TorchLightConfig.TorchLightLayoutFolder;
        List<string> AllRuleFiles = TorchLightTools.GetAllFileInFolderFullPath(RuleFolder, ".dat");
        foreach (string RuleFile in AllRuleFiles)
        {
            TorchLightLevelRandomGenerater.LevelBuildInfo LevelInfo = LoadLevelRuleFile(RuleFile);
            SaveLevelRuleFile(RuleFile, LevelInfo);
        }

        Debug.Log("Level Rule Files Convert Finished.");
    }

    static TorchLightLevelRandomGenerater.LevelBuildInfo LoadLevelRuleFile(string RulePath)
    {
        StreamReader Reader = null;

        try
        {
            Reader = new StreamReader(RulePath);
        }
        catch (System.Exception) { Debug.Log(RulePath + " Not Found."); return null; }

        TorchLightLevelRandomGenerater.LevelBuildInfo LevelInfo = new TorchLightLevelRandomGenerater.LevelBuildInfo();
        while (!Reader.EndOfStream)
        {
            string Line = Reader.ReadLine().Trim();

            string Tag = ""; string Value = "";
            TorchLightTools.ParseLine(Line, ref Tag, ref Value);

            if (Tag == "LEVELNAME")         LevelInfo.LevelName             = Value;
            if (Tag == "NAME")              LevelInfo.DisplayName           = Value;
            if (Tag == "MUSIC")             LevelInfo.BackgroundMusic       = Value;

            if (Tag == "AMBIENT RED")       LevelInfo.AmbientColor.r        = float.Parse(Value);
            if (Tag == "AMBIENT GREEN")     LevelInfo.AmbientColor.g        = float.Parse(Value);
            if (Tag == "AMBIENT BLUE")      LevelInfo.AmbientColor.b        = float.Parse(Value);

            if (Tag == "DIRECTIONAL RED")   LevelInfo.DirectionLightColor.r = float.Parse(Value);
            if (Tag == "DIRECTIONAL GREEN") LevelInfo.DirectionLightColor.g = float.Parse(Value);
            if (Tag == "DIRECTIONAL BLUE")  LevelInfo.DirectionLightColor.b = float.Parse(Value);

            if (Tag == "DIRECTIONAL X")     LevelInfo.DirectionLightDir.x   = float.Parse(Value);
            if (Tag == "DIRECTIONAL Y")     LevelInfo.DirectionLightDir.y   = float.Parse(Value);
            if (Tag == "DIRECTIONAL Z")     LevelInfo.DirectionLightDir.z   = -float.Parse(Value);

            if (Tag == "FOG RED")           LevelInfo.FogColor.r            = float.Parse(Value);
            if (Tag == "FOG GREEN")         LevelInfo.FogColor.g            = float.Parse(Value);
            if (Tag == "FOG BLUE")          LevelInfo.FogColor.b            = float.Parse(Value);

            if (Tag == "FOG START")         LevelInfo.FogBegin              = float.Parse(Value);
            if (Tag == "FOG END")           LevelInfo.FogEnd                = float.Parse(Value);

            if (Tag == "MINCHUNKS")         LevelInfo.MinChunkNum           = int.Parse(Value);
            if (Tag == "MAXCHUNKS")         LevelInfo.MaxChunkNum           = int.Parse(Value);

            if (Line == LEVEL_SUB_CHUNK_BEGIN)
            {
                TorchLightLevelRandomGenerater.LevelChunk Chunk = new TorchLightLevelRandomGenerater.LevelChunk();
                while (Line != LEVEL_SUB_CHUNK_END)
                {
                    TorchLightTools.ParseLine(Line, ref Tag, ref Value);

                    if (Tag == "NAME")
                    {
                        Chunk.ChunkName = Value;

                        string ChunFolder   = TorchLightTools.GetFolder(RulePath) + "/" + Value;
                        Chunk.SceneNames    = TorchLightTools.GetAllFileInFolder(ChunFolder, ".layout");
                    }

                    Line = Reader.ReadLine().Trim();
                }

                LevelInfo.LevelChunks.Add(Chunk);
            }
        }

        Reader.Close();

        return LevelInfo;
    }

    static void SaveLevelRuleFile(string LevelRulePath, TorchLightLevelRandomGenerater.LevelBuildInfo LevelInfo)
    {
        StreamWriter Writer = null;
        try
        {
            Writer = new StreamWriter(LevelRulePath + ".TXT");
        }
        catch (System.Exception) { Debug.Log(LevelRulePath + " Not Found."); return; }

        Writer.WriteLine("NAME:"            + LevelInfo.LevelName);
        Writer.WriteLine("LEVELNAME:"       + LevelInfo.DisplayName);
        Writer.WriteLine("BGMUSIC:"         + GetMusicPath(LevelInfo.BackgroundMusic));
        Writer.WriteLine("AMBIENT:"         + LevelInfo.AmbientColor.r + "," + LevelInfo.AmbientColor.g + "," + LevelInfo.AmbientColor.b);
        Writer.WriteLine("DIRECTION_COLOR:" + LevelInfo.DirectionLightColor.r + "," + LevelInfo.DirectionLightColor.g + "," + LevelInfo.DirectionLightColor.b);
        Writer.WriteLine("DIRECTION_DIR:"   + LevelInfo.DirectionLightDir.x + "," + LevelInfo.DirectionLightDir.y + "," + LevelInfo.DirectionLightDir.z);
        Writer.WriteLine("FOG_COLOR:"       + LevelInfo.FogColor.r + "," + LevelInfo.FogColor.g + "," + LevelInfo.FogColor.b);
        Writer.WriteLine("FOG_BEGIN:"       + LevelInfo.FogBegin);
        Writer.WriteLine("FOG_END:"         + LevelInfo.FogEnd);
        Writer.WriteLine("MINCHUNK:"        + LevelInfo.MinChunkNum);
        Writer.WriteLine("MAXCHUNK:"        + LevelInfo.MaxChunkNum);

        foreach (TorchLightLevelRandomGenerater.LevelChunk Chunk in LevelInfo.LevelChunks)
        {
            Writer.WriteLine(LEVEL_SUB_CHUNK_BEGIN);
            Writer.WriteLine("CHUNK_NAME:" + Chunk.ChunkName);
            foreach (string SubChunk in Chunk.SceneNames)
                Writer.WriteLine("CHUNK_FILE:" + TorchLightConfig.TorchLightLayoutFolder + GetScenePath(LevelRulePath) + "/" + Chunk.ChunkName + "/" + SubChunk + ".layout");
            Writer.WriteLine(LEVEL_SUB_CHUNK_END);
        }

        Writer.Close();
    }
}
