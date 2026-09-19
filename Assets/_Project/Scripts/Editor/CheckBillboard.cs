using UnityEngine;
using UnityEditor;

public class CheckBillboard
{
    [MenuItem("Tools/Check Billboard")]
    public static void Run()
    {
        QuadAnimator[] anims = Object.FindObjectsByType<QuadAnimator>(FindObjectsSortMode.None);
        foreach(var a in anims)
        {
            Billboard bb = a.GetComponent<Billboard>();
            if (bb == null)
            {
                Debug.LogWarning($"QuadAnimator {a.name} (parent {a.transform.parent?.name}) is MISSING Billboard script!");
            }
            else
            {
                Debug.Log($"QuadAnimator {a.name} (parent {a.transform.parent?.name}) HAS Billboard script. Rotation: {a.transform.rotation.eulerAngles}");
            }
        }
    }
}
