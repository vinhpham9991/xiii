using UnityEngine;
using UnityEditor;

public class CheckAnSprite
{
    [MenuItem("Tools/Check An Sprite")]
    public static void Run()
    {
        string path = "Assets/_Project/Art/Sprites/An/An_idle.png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var asset in assets)
        {
            if (asset is Sprite s && s.name == "An_idle_0")
            {
                Debug.Log($"An_idle_0 rect: {s.rect}, pixelsPerUnit: {s.pixelsPerUnit}");
                Vector4 outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(s);
                Debug.Log($"An_idle_0 outerUV: {outerUV}");
                return;
            }
        }
        Debug.Log("An_idle_0 not found!");
    }
}
