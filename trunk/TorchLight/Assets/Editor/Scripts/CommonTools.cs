using UnityEngine;
using UnityEditor;
using System.Collections;

public class CommonTools : EditorWindow {

    [MenuItem("TorchLight/CommonEditor")]
    static void Execute()
    {
        CommonTools ToolWindow = CreateInstance<CommonTools>();
        ToolWindow.Show();
    }


    void OnGUI()
    {
        DrawDisableCastShadowForAlphaMaterial();
    }

    GameObject RootGameObject = null;
    void DrawDisableCastShadowForAlphaMaterial()
    {
        RootGameObject = (GameObject)EditorGUILayout.ObjectField(RootGameObject, typeof(GameObject));
        bool ButtonPress = GUILayout.Button("Disable CastShadow(Alpha Material)");
        ProcessDisableCastShadow(ButtonPress);
    }

    void ProcessDisableCastShadow(bool Press)
    {
        if (Press && RootGameObject != null)
        {
            MeshRenderer[] Renders = RootGameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer Render in Renders)
            {
                string ShaderName = Render.material.shader.name;
                if (ShaderName.IndexOf("Transparent") != -1)
                    Render.castShadows = false;
            }

            Debug.Log("Finish Process Disable Cast Shadow [" + Renders.Length + "]");
        }
    }
}
