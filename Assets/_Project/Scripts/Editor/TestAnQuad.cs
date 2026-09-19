using UnityEngine;
using UnityEditor;

public class TestAnQuad
{
    [MenuItem("Tools/An/Test An Quad")]
    public static void Run()
    {
        QuadAnimator[] allQuads = Object.FindObjectsByType<QuadAnimator>(FindObjectsSortMode.None);
        foreach (var quad in allQuads)
        {
            if (quad.transform.parent != null && quad.transform.parent.name.Contains("An"))
            {
                Debug.Log("Found An Quad inside: " + quad.transform.parent.name + " -> " + quad.name);
            }
        }
    }
}
