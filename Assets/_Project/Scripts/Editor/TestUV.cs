using UnityEngine;
using UnityEditor;

public class TestUV
{
    [MenuItem("Tools/XIII/Test UV")]
    public static void Run()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Sprites/XIII/XIII_hit.png");
        foreach(var a in assets) {
            if (a is Sprite sp) {
                Debug.Log(sp.name + " outerUV: " + UnityEngine.Sprites.DataUtility.GetOuterUV(sp));
            }
        }
    }
}
