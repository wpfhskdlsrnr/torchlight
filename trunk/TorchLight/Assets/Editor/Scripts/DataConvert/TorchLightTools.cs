using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class TorchLightTools
{
    public static string GetFileName(string Path)
    {
        Path = Path.Replace('\\', '/');
        int IndexA = Path.LastIndexOf('/');
        int IndexB = Path.LastIndexOf('/', IndexA - 1);
        return Path.Substring(IndexB + 1, IndexA - IndexB - 1);
    }

    // Parse Line
    // <INT>NAME:10
    // Tag = Name, Value = 10
    public static void ParseLine(string Line, ref string Tag, ref string Value)
    {
        if (Line.IndexOf('>') == -1) return;

        int Index = Line.IndexOf('>');
        int Index1 = Line.IndexOf(':');

        Tag = Line.Substring(Index + 1, Index1 - Index - 1);
        Value = Line.Substring(Index1 + 1);
    }

    // c:/111/111/111/1.txt
    // return c:/111/111/111/
    public static string GetFolder(string Path)
    {
        string APath = Path.Replace('\\', '/');
        return APath.Substring(0, APath.LastIndexOf('/'));
    }

    // c:/111/111/111.txt
    // return 111
    public static string GetName(string FileName)
    {
        FileName = FileName.Replace('\\', '/');

        int StartIndex = 0;
        if (FileName.IndexOf('/') != -1)
            StartIndex = FileName.LastIndexOf('/') + 1;

        return FileName.Substring(StartIndex, FileName.LastIndexOf('.'));
    }

    // get all file name with suffixs
    public static List<string> GetAllFileInFolder(string Folder, string Suffix)
    {
        List<string> Files = new List<string>();
        DirectoryInfo Dir = new DirectoryInfo(Folder);
        if (Dir.Exists)
        {
            foreach (FileInfo File in Dir.GetFiles())
            {
                if (File.Name.EndsWith(Suffix, StringComparison.CurrentCultureIgnoreCase))
                    Files.Add(GetName(File.Name));
            }
        }
        return Files;
    }

    public static List<string> GetAllFileInFolderFullPath(string Folder, string Suffix)
    {
        List<string> Files = new List<string>();
        DirectoryInfo Dir = new DirectoryInfo(Folder);
        if (Dir.Exists)
        {
            foreach (FileInfo File in Dir.GetFiles())
            {
                if (File.FullName.EndsWith(Suffix, StringComparison.CurrentCultureIgnoreCase))
                    Files.Add(File.FullName);
            }

            foreach (DirectoryInfo D in Dir.GetDirectories())
            {
                Files.AddRange(GetAllFileInFolderFullPath(D.FullName, Suffix));
            }
        }
        return Files;
    }

    public static void ParseTag(string Line, ref string Tag, ref string Value)
    {
        // 1111:2222
        int Index = Line.IndexOf(':');
        if (Index == -1) return;

        Tag = Line.Substring(0, Index);
        Value = Line.Substring(Index + 1);
    }

    public static Color ParseColor(string Line)
    {
        Color OutColor = new Color();
        string[] Value = Line.Split(',');
        OutColor.r = float.Parse(Value[0]) / 255.0f;
        OutColor.g = float.Parse(Value[1]) / 255.0f;
        OutColor.b = float.Parse(Value[2]) / 255.0f;
        return OutColor;
    }

    public static Vector3 ParseVector3(string Line)
    {
        Vector3 Vec = new Vector3();
        string[] Value = Line.Split(',');
        Vec.x = float.Parse(Value[0]);
        Vec.y = float.Parse(Value[1]);
        Vec.z = float.Parse(Value[2]);
        return Vec;
    }
}
