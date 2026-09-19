using UnityEngine;
using UnityEditor;

public class FixSprite
{
    public static void Run()
    {
        Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII/XIII stance.png");
        if (newSprite == null)
        {
            Debug.LogError("Could not find XIII stance.png");
            return;
        }

        bool changed = false;

        // Find in scene
        CharacterInteraction[] chars = Object.FindObjectsOfType<CharacterInteraction>();
        foreach(var c in chars)
        {
            if (c.characterName.Contains("XIII"))
            {
                Transform visual = c.transform.Find("Visual");
                if (visual != null)
                {
                    SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sprite = newSprite;
                        
                        // Add animator if missing
                        Animator anim = visual.GetComponent<Animator>();
                        if (anim == null) anim = visual.gameObject.AddComponent<Animator>();
                        
                        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/XIII/XIII_Animator.controller");
                        anim.runtimeAnimatorController = controller;
                        
                        EditorUtility.SetDirty(c.gameObject);
                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            Debug.Log("Successfully updated XIII sprite and animator in the scene!");
        }
        else
        {
            Debug.Log("Could not find any XIII character in the scene.");
        }
    }
}
