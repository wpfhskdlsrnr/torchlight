using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TorchLightLevelLayoutSetConvert
{
    public static bool ParseLevelSet()
    {
        string LevelSetPath = TorchLightConfig.TorchLightOrignalLevelSetPath;

        StreamReader Reader = null;
        Reader = new StreamReader(LevelSetPath);

        List<TorchLightLevel.PirceItem> Pieces = new List<TorchLightLevel.PirceItem>();
        while (!Reader.EndOfStream)
        {
            string Line = Reader.ReadLine().Trim();

            if (Line == TorchLightLevel.CHUNK_PIECE_BEGIN)
            {
                TorchLightLevel.PirceItem AItem = new TorchLightLevel.PirceItem();

                Line = Reader.ReadLine().Trim();
                while (Line != TorchLightLevel.CHUNK_PIECE_END)
                {
                    string Tag = ""; string Value = "";

                    TorchLightTools.ParseLine(Line, ref Tag, ref Value);

                    if      (Tag == "NAME")             AItem.Name = Value;
                    else if (Tag == "GUID")             AItem.GUID = Value;
                    else if (Tag == "FILE")             AItem.Meshes.Add(EditorTools.MeshConvertToUnityPath(Value));
                    else if (Tag == "COLLISIONFILE")    AItem.CollisionMeshes.Add(EditorTools.MeshConvertToUnityPath(Value));

                    Line = Reader.ReadLine().Trim();
                }

                Pieces.Add(AItem);
            }
        }
        Reader.Close();

        StreamWriter Write = null;
        try
        {
            Write           = new StreamWriter(TorchLightConfig.TorchLightConvertedLevelSetPath);
            Debug.Log("Saved Level Layout Set At : " + TorchLightConfig.TorchLightConvertedLevelSetPath);
        }
        catch (System.Exception) { return false; }

        foreach (TorchLightLevel.PirceItem AItem in Pieces)
        {
            Write.WriteLine(TorchLightLevel.CHUNK_PIECE_BEGIN);
            Write.WriteLine("NAME:" + AItem.Name);
            Write.WriteLine("GUID:" + AItem.GUID);

            foreach (string Name in AItem.Meshes)
                Write.WriteLine("MESH:" + Name);
            foreach (string Name in AItem.CollisionMeshes)
                Write.WriteLine("COLLSIONMESH:" + Name);
            
            Write.WriteLine(TorchLightLevel.CHUNK_PIECE_END);
        }

        Write.Close();

        Debug.Log("Parse Level Set Item Finished.");

        return true;
    }
}
