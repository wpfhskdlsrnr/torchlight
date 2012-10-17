using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TorchLightLevelLayoutSetConvert
{
    public static string DESCREPTION_MONSTER        = "Monster";
    public static string DESCREPTION_ROOM_PIECE     = "Room Piece";
    public static string DESCREPTION_PARTICLE       = "Layout Link Particle";
    public static string DESCREPTION_LIGHT          = "Light";
    public static string DESCREPTION_LAYOUT_LINK    = "Layout Link";

    public static string DESCREPTION_UNIT_TRIGGER   = "Unit Trigger";
    public static string DESCREPTION_WARPER         = "Warper";
    public static string DESCREPTION_MUSIC          = "Music";

    static string ChunkBegin    = "[PROPERTIES]";
    static string ChunkEnd      = "[/PROPERTIES]";

    public class LevelItem
    {
        public string       Name;
        public string       Tag;
        public Vector3      Position    = Vector3.zero;
        public Quaternion   Rotation    = Quaternion.identity;
        public float        Scaling     = 1.0f;
        public string       GUID        = null;
        public string       ResFile    = null;

        public string       ExternInfo;
    }

    static Vector3 RIGHT_VECTOR = new Vector3(1.0f, 0.0f, 0.0f);
    static public List<LevelItem> ParseLevelLayout(string LayoutPath)
    {
        List<LevelItem> LevelItems = new List<LevelItem>();
        {
            StreamReader Reader = null;
            
            try
            {
                Reader = new StreamReader(LayoutPath);
            }
            catch (System.Exception) { Debug.Log(LayoutPath + " Not Found."); return null; }
            
            while (!Reader.EndOfStream)
            {
                string Line = Reader.ReadLine().Trim();

                if (Line == ChunkBegin)
                {
                    Line = Reader.ReadLine().Trim();
                    
                    string Tag = ""; string Value = "";
                    TorchLightTools.ParseLine(Line, ref Tag, ref Value);

                    if (Value == DESCREPTION_ROOM_PIECE ||
                        Value == DESCREPTION_MONSTER ||
                        Value == DESCREPTION_LIGHT ||
                        Value == DESCREPTION_UNIT_TRIGGER ||
                        Value == DESCREPTION_WARPER)
                    {
                        LevelItem AItem = new LevelItem();
                        AItem.Tag = Value;

                        Vector3 RightDirection = -RIGHT_VECTOR;

                        Line = Reader.ReadLine().Trim();
                        while (Line != ChunkEnd)
                        {
                            TorchLightTools.ParseLine(Line, ref Tag, ref Value);


                            if      (Tag == "NAME") AItem.Name = Value;
                            else if (Tag == "POSITIONX")    AItem.Position.x    = float.Parse(Value);
                            else if (Tag == "POSITIONY")    AItem.Position.y    = float.Parse(Value);
                            else if (Tag == "POSITIONZ")    AItem.Position.z    = -float.Parse(Value);
                            else if (Tag == "RIGHTX")       RightDirection.x    = -float.Parse(Value);
                            else if (Tag == "RIGHTY")       RightDirection.y    = -float.Parse(Value);
                            else if (Tag == "RIGHTZ")       RightDirection.z    = float.Parse(Value);
                            else if (Tag == "SCALE")        AItem.Scaling       = float.Parse(Value);
                            else if (Tag == "SCALE X")      AItem.Scaling       = float.Parse(Value);
                            else if (Tag == "GUID")         AItem.GUID          = Value;
                            else if (Tag == "FILE")         AItem.ResFile       = ConvertToUnityPath(Value); // Mesh Path
                            else if (Tag == "DUNGEON NAME") AItem.ExternInfo    = Value;

                            Line = Reader.ReadLine().Trim();
                        }

                        AItem.Rotation = Quaternion.FromToRotation(RIGHT_VECTOR, RightDirection);
                        LevelItems.Add(AItem);
                    }
                }
            }

            Reader.Close();
        }
        return LevelItems;
    }

    static string CHUNK_PIECE_BEGIN = "[PIECE]";
    static string CHUNK_PIECE_END   = "[/PIECE]";

    public class PirceItem
    {
        public string Name;
        public string GUID;
        public List<string> Meshes = new List<string>();
        public List<string> CollisionMeshes = new List<string>();
    }

    static string ConvertToUnityPath(string Path)
    {
        string NewPath  = TorchLightConfig.TorchLightAssetFolder + Path;
        NewPath         = NewPath.Substring(0, NewPath.IndexOf('.')) + ".FBX";
        return NewPath;
    }

    public static bool ParseLevelSet()
    {
        string LevelSetPath = TorchLightConfig.TorchLightOrignalLevelSetPath;

        StreamReader Reader = null;
        try
        {
            Reader = new StreamReader(LevelSetPath);
        }
        catch (System.Exception) { Debug.Log(LevelSetPath + " Not Found."); return false; }

        List<PirceItem> Pieces = new List<PirceItem>();
        while (!Reader.EndOfStream)
        {
            string Line = Reader.ReadLine().Trim();

            if (Line == CHUNK_PIECE_BEGIN)
            {
                PirceItem AItem = new PirceItem();

                Line = Reader.ReadLine().Trim();
                while (Line != CHUNK_PIECE_END)
                {
                    string Tag = ""; string Value = "";

                    TorchLightTools.ParseLine(Line, ref Tag, ref Value);

                    if      (Tag == "NAME")             AItem.Name = Value;
                    else if (Tag == "GUID")             AItem.GUID = Value;
                    else if (Tag == "FILE")             AItem.Meshes.Add(ConvertToUnityPath(Value));
                    else if (Tag == "COLLISIONFILE")    AItem.CollisionMeshes.Add(ConvertToUnityPath(Value));

                    Line = Reader.ReadLine().Trim();
                }

                Pieces.Add(AItem);
            }
        }
        Reader.Close();

        StreamWriter Write = null;
        try
        {
            int Index       = LevelSetPath.IndexOf('.');
            string FilePath = LevelSetPath.Substring(0, Index);
            Write           = new StreamWriter(FilePath + ".Items");

            Debug.Log("Saved Level Layout Set At : " + FilePath);
        }
        catch (System.Exception) { return false; }

        foreach (PirceItem AItem in Pieces)
        {
            Write.Write(CHUNK_PIECE_BEGIN + "\n");
            Write.Write("NAME:" + AItem.Name + "\n");
            Write.Write("GUID:" + AItem.GUID + "\n");

            foreach (string Name in AItem.Meshes)
                Write.Write("MESH:" + Name + "\n");
            foreach (string Name in AItem.CollisionMeshes)
                Write.Write("COLLSIONMESH:" + Name + "\n");
            
            Write.Write(CHUNK_PIECE_END + "\n");
        }

        Write.Close();

        Debug.Log("Parse Level Set Item Finished.");

        return true;
    }

    static public Dictionary<string, PirceItem> GetAllPieceItems()
    {
        string LayoutSetItemPath = TorchLightConfig.TorchLightConvertedLevelSetPath;

        Dictionary<string,PirceItem> PieceItems = new Dictionary<string,PirceItem>();
        {
            StreamReader Reader = null;
            try
            {
                Reader = new StreamReader(LayoutSetItemPath);
            }
            catch (System.Exception) { Debug.Log(LayoutSetItemPath + " Not Found. Please Convert Level Layout File To Level Items File"); return null; }

            while (!Reader.EndOfStream)
            {
                string Line = Reader.ReadLine().Trim();

                if (Line == CHUNK_PIECE_BEGIN)
                {
                    PirceItem AItem = new PirceItem();

                    Line = Reader.ReadLine().Trim();
                    while (Line != CHUNK_PIECE_END)
                    {
                        string Tag = ""; string Value = "";

                        TorchLightTools.ParseTag(Line, ref Tag, ref Value);

                        if (Tag == "NAME")                  AItem.Name = Value;
                        else if (Tag == "GUID")             AItem.GUID = Value;
                        else if (Tag == "MESH")             AItem.Meshes.Add(Value);
                        else if (Tag == "COLLSIONMESH")     AItem.CollisionMeshes.Add(Value);

                        Line = Reader.ReadLine().Trim();
                    }

                    if (AItem.GUID != null)
                        PieceItems.Add(AItem.GUID, AItem);
                }
            }
            Reader.Close();
        }
        return PieceItems;
    }
}
