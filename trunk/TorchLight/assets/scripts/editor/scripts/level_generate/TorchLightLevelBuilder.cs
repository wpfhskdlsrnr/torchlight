
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class TorchLightLevelBuilder : MonoBehaviour 
{
    static Vector3  OneVector3 = new Vector3(1.0f, 1.0f, 1.0f);

    static Dictionary<string, GameObject>                   PrefabsCache        = new Dictionary<string, GameObject>();
    static Dictionary<string, Mesh>                         CollisionMeshCache  = new Dictionary<string, Mesh>();
    static Dictionary<string, TorchLightLevel.PirceItem>    GAllPieceItems      = null;

    static GameObject GetCachedGameObject(string FbxPath, TorchLightLevel.LevelItem AItem)
    {
        GameObject Obj = null;
        if (PrefabsCache.ContainsKey(FbxPath))
        {
            Obj = PrefabsCache[FbxPath];
        }
        else
        {
            GameObject ObjToPrefab = AssetDatabase.LoadAssetAtPath(FbxPath, typeof(GameObject)) as GameObject;
            if (ObjToPrefab != null)
            {
                Obj = ObjToPrefab;
                PrefabsCache.Add(FbxPath, Obj);
            }
            else
                Debug.LogError(FbxPath + "  Not Found");
        }

        return Obj;
    }

    static Mesh GetCachedCollisionMesh(string ConllisionMeshFile, TorchLightLevel.LevelItem AItem)
    {
        Mesh CollisionMesh = null;
        if (CollisionMeshCache.ContainsKey(ConllisionMeshFile))
            CollisionMesh = CollisionMeshCache[ConllisionMeshFile];
        else
        {
            GameObject ObjToPrefab = AssetDatabase.LoadAssetAtPath(ConllisionMeshFile, typeof(GameObject)) as GameObject;
            if (ObjToPrefab != null)
            {
                ObjToPrefab = Instantiate(ObjToPrefab) as GameObject;

                MeshFilter Mesher = ObjToPrefab.GetComponentInChildren<MeshFilter>();
                CollisionMesh = Mesher.sharedMesh != null ? Mesher.sharedMesh : Mesher.mesh;

                CollisionMeshCache.Add(ConllisionMeshFile, CollisionMesh);
                DestroyImmediate(ObjToPrefab);
            }
        }

        return CollisionMesh;
    }

    static Vector3 LightOffset = new Vector3(0.0f, 1.0f, 0.0f);
    static public void InstanceLight(TorchLightLevel.LevelItem AItem, GameObject LevelLights)
    {
        GameObject LightObj = new GameObject("Light");

        Light ALight    = LightObj.AddComponent<Light>();
        ALight.type     = LightType.Point;
        ALight.range    = AItem.Scaling;

        LightObj.light.color        = Color.white;
        LightObj.transform.position = AItem.Position + LightOffset;
        LightObj.transform.parent   = LevelLights.transform;
    }

    static public void InstanceObj(TorchLightLevel.LevelItem AItem, GameObject LevelObjects)
    {
        string MeshFile = "";
        string ConllisionMeshFile = "";
        bool bHasCollision = false;

        if (AItem.GUID != null && AItem.GUID.Length > 0)
        {
            if (!GAllPieceItems.ContainsKey(AItem.GUID))
            {
                Debug.LogError(AItem.GUID + " Resource Not Found");
                return;
            }

            TorchLightLevel.PirceItem PieceItem = GAllPieceItems[AItem.GUID];
            MeshFile = PieceItem.Meshes[0];

            if (PieceItem.CollisionMeshes.Count > 0)
            {
                bHasCollision = true;
                ConllisionMeshFile = PieceItem.CollisionMeshes[0];
            }
        }
        else
            MeshFile = TorchLightConfig.TorchLightModelsFolder + AItem.ResFile;

        GameObject Obj = GetCachedGameObject(MeshFile, AItem);
        if (Obj != null)
        {
            Obj = Instantiate(Obj, AItem.Position, AItem.Rotation) as GameObject;
            if (bHasCollision)
            {
                Mesh CollisionMesh = GetCachedCollisionMesh(ConllisionMeshFile, AItem);
                if (CollisionMesh != null)
                {
                    try
                    {
                        MeshCollider Collider = Obj.GetComponentInChildren<MeshRenderer>().gameObject.AddComponent<MeshCollider>();
                        Collider.sharedMesh = CollisionMesh;
                    }
                    catch (System.Exception){}
                }
            }

            Obj.name = AItem.Name;
            Obj.transform.localScale = OneVector3 * AItem.Scaling;
            Obj.transform.parent = LevelObjects.transform;
        }
        else
            Debug.LogError(MeshFile + "  Not Found");
    }

    static public void InitiAllPieceItems()
    {
        if (GAllPieceItems == null)
            GAllPieceItems = TorchLightLevel.GetAllPieceItems();
    }

    static public GameObject LoadLevelLayoutToScene(string LayoutPath)
    {
        InitiAllPieceItems();

        List<TorchLightLevel.LevelItem> LevelItems = TorchLightLevel.ParseLevelLayout(LayoutPath);
        if (LevelItems != null)
        {
            GameObject Level                = new GameObject(EditorTools.GetUpFolderName(LayoutPath));
            GameObject LevelObjects         = new GameObject("LevelObjects");
            GameObject LevelNpcs            = new GameObject("LevelNpcs");
            GameObject LevelLights          = new GameObject("LevelLights");
            GameObject LevelTriggers        = new GameObject("LevelTriggers");
            GameObject LevelWarpers         = new GameObject("LevelWarpers");
            LevelObjects.transform.parent   = Level.transform;
            LevelNpcs.transform.parent      = Level.transform;
            LevelLights.transform.parent    = Level.transform;
            LevelTriggers.transform.parent  = Level.transform;
            LevelWarpers.transform.parent   = Level.transform;

            foreach (TorchLightLevel.LevelItem AItem in LevelItems)
            {
                if (AItem.Tag == TorchLightLevel.DESCREPTION_LIGHT)
                {
                    TorchLightLevelBuilder.InstanceLight(AItem, LevelLights);
                }

                if (AItem.Tag == TorchLightLevel.DESCREPTION_MONSTER)
                {
                    TorchLightLevelBuilder.InstanceObj(AItem, LevelNpcs);
                }

                if (AItem.Tag == TorchLightLevel.DESCREPTION_ROOM_PIECE)
                {
                    TorchLightLevelBuilder.InstanceObj(AItem, LevelObjects);
                }

            }
            return Level;
        }

        return null;
    }
}