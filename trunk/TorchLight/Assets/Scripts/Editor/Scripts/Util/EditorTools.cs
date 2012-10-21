using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class EditorTools : MonoBehaviour {

    public static string MeshConvertToUnityPath(string Path)
    {
        string NewPath = TorchLightConfig.TorchLightAssetFolder + Path;
        NewPath = NewPath.Substring(0, NewPath.IndexOf('.')) + ".FBX";
        NewPath = NewPath.ToLower();
        NewPath = NewPath.Replace("media", "resources");
        return NewPath;
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

    public static void CheckFolderExit(string Folder)
    {
        DirectoryInfo Dir = new DirectoryInfo(Folder);
        if (!Dir.Exists)
            Dir.CreateSubdirectory(Folder);
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

        return FileName.Substring(StartIndex, FileName.LastIndexOf('.') - StartIndex);
    }
}
