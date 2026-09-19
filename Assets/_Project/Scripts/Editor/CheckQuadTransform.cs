using UnityEngine;
using UnityEditor;

public class CheckQuadTransform
{
    [MenuItem("Tools/Check Quad Transform")]
    public static void Run()
    {
        var quads = Object.FindObjectsByType<QuadAnimator>(FindObjectsSortMode.None);
        foreach(var q in quads)
        {
            if (q.transform.parent != null && q.transform.parent.name.Contains("An"))
            {
                Debug.Log($"An Quad: pos={q.transform.position}, rot={q.transform.eulerAngles}, scale={q.transform.localScale}");
                Debug.Log($"Camera: rot={Camera.main.transform.eulerAngles}");
                
                Billboard bb = q.GetComponent<Billboard>();
                if (bb == null) Debug.Log("No Billboard!");
                else Debug.Log("Billboard is present.");
            }
        }
    }
}
