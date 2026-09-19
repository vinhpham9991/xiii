using UnityEngine;
using UnityEditor;

public class FixPivot : EditorWindow
{
    [MenuItem("Tools/Character Alignment")]
    public static void ShowWindow()
    {
        GetWindow<FixPivot>("Character Alignment");
    }

    void OnGUI()
    {
        GUILayout.Label("Điều chỉnh vị trí chân nhân vật", EditorStyles.boldLabel);
        GUILayout.Label("Kéo thanh trượt để chân chạm đúng vòng tròn.");
        GUILayout.Space(10);

        var anims = Object.FindObjectsOfType<QuadAnimator>();
        foreach(var a in anims)
        {
            if (a.transform.parent != null)
            {
                string charName = a.transform.parent.name;
                GUILayout.BeginHorizontal();
                GUILayout.Label(charName, GUILayout.Width(150));
                
                EditorGUI.BeginChangeCheck();
                float newY = GUILayout.HorizontalSlider(a.customOffset.y, -2f, 2f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(a, "Change Custom Offset");
                    a.customOffset = new Vector3(0, newY, 0);
                    EditorUtility.SetDirty(a);
                    
                    // Force update in edit mode if possible, 
                    // though QuadAnimator might need an Update method to show changes live
                }
                
                // Also allow typing the number
                a.customOffset.y = EditorGUILayout.FloatField(a.customOffset.y, GUILayout.Width(50));
                
                GUILayout.EndHorizontal();
            }
        }
        
        GUILayout.Space(20);
        if (GUILayout.Button("Force Update Position (Need Play Mode)"))
        {
            // Optional: force update
        }
    }
}
