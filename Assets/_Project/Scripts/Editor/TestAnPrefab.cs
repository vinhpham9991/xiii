using UnityEngine;
using UnityEditor;

public class TestAnPrefab
{
    [MenuItem("Tools/An/Test An Prefab")]
    public static void Run()
    {
        string[] guids = AssetDatabase.FindAssets("An t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            QuadAnimator quad = prefab.GetComponentInChildren<QuadAnimator>();
            if (quad != null)
            {
                Debug.Log("Prefab: " + path);
                Debug.Log("- idleSprites count: " + (quad.idleSprites != null ? quad.idleSprites.Length : 0));
                Debug.Log("- jumpSprites count: " + (quad.jumpSprites != null ? quad.jumpSprites.Length : 0));
                Debug.Log("- attackSprites count: " + (quad.attackSprites != null ? quad.attackSprites.Length : 0));
                
                Renderer r = quad.GetComponent<Renderer>();
                if (r != null && r.sharedMaterial != null)
                {
                    Debug.Log("- Material: " + r.sharedMaterial.name);
                    Debug.Log("- BaseMap: " + (r.sharedMaterial.GetTexture("_BaseMap") != null ? r.sharedMaterial.GetTexture("_BaseMap").name : "null"));
                    Debug.Log("- BaseMap Scale: " + r.sharedMaterial.GetTextureScale("_BaseMap"));
                }
            }
        }
    }
}
