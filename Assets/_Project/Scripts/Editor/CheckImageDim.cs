using UnityEngine;
using UnityEditor;

public class CheckImageDim
{
    [MenuItem("Tools/Check Jump Image")]
    public static void Check()
    {
        string p1 = "Assets/_Project/Art/Sprites/XIII/XIII_jump.png";
        string p2 = "Assets/_Project/Art/Sprites/XIII/XIII_attack.png";
        
        Texture2D t1 = AssetDatabase.LoadAssetAtPath<Texture2D>(p1);
        Texture2D t2 = AssetDatabase.LoadAssetAtPath<Texture2D>(p2);
        
        Debug.Log($"Jump: {(t1 != null ? t1.width + "x" + t1.height : "null")}");
        Debug.Log($"Attack: {(t2 != null ? t2.width + "x" + t2.height : "null")}");
    }
}
