using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class XIIIAnimationSetup : EditorWindow
{
    [MenuItem("Tools/Setup XIII Animations")]
    public static void SetupAnimations()
    {
        string spriteFolder = "Assets/_Project/Sprites/XIII";
        string animFolder = "Assets/_Project/Animations/XIII";

        if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations"))
        {
            AssetDatabase.CreateFolder("Assets/_Project", "Animations");
        }
        if (!AssetDatabase.IsValidFolder(animFolder))
        {
            AssetDatabase.CreateFolder("Assets/_Project/Animations", "XIII");
        }

        // 1. Re-import all pngs in spriteFolder as Sprites
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { spriteFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        AssetDatabase.Refresh();

        // 2. Load Sprites
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                sprites[sprite.name] = sprite;
            }
        }

        // 3. Create Animation Clips
        AnimationClip appear = CreateClip(animFolder + "/appear.anim", new[] { GetSprite(sprites, "XIII appear-1"), GetSprite(sprites, "XIII appear-2") }, 12f, false);
        AnimationClip stance = CreateClip(animFolder + "/stance.anim", new[] { GetSprite(sprites, "XIII stance") }, 12f, true);
        AnimationClip dash = CreateClip(animFolder + "/dash.anim", new[] { GetSprite(sprites, "XIII dash") }, 12f, false);
        AnimationClip attack = CreateClip(animFolder + "/attack.anim", new[] { GetSprite(sprites, "XIII attack-1"), GetSprite(sprites, "XIII attack-2"), GetSprite(sprites, "XIII attack-3") }, 12f, false);
        AnimationClip downed = CreateClip(animFolder + "/downed.anim", new[] { GetSprite(sprites, "XIII downed-1"), GetSprite(sprites, "XIII downed-2") }, 12f, false);
        AnimationClip hit = CreateClip(animFolder + "/hit.anim", new[] { GetSprite(sprites, "XIII hit") }, 12f, false);

        // 4. Create Animator Controller
        string controllerPath = animFolder + "/XIII_Animator.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        AnimatorState stanceState = rootStateMachine.AddState("stance");
        stanceState.motion = stance;
        rootStateMachine.defaultState = stanceState;

        AnimatorState appearState = rootStateMachine.AddState("appear");
        appearState.motion = appear;

        AnimatorState dashState = rootStateMachine.AddState("dash");
        dashState.motion = dash;

        AnimatorState attackState = rootStateMachine.AddState("attack");
        attackState.motion = attack;

        AnimatorState downedState = rootStateMachine.AddState("downed");
        downedState.motion = downed;

        AnimatorState hitState = rootStateMachine.AddState("hit");
        hitState.motion = hit;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("XIII Animations Setup Complete!");
    }

    private static Sprite GetSprite(Dictionary<string, Sprite> dict, string name)
    {
        if (dict.ContainsKey(name)) return dict[name];
        Debug.LogWarning("Sprite not found: " + name);
        return null;
    }

    private static AnimationClip CreateClip(string path, Sprite[] sprites, float frameRate, bool loop)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = frameRate;

        if (loop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        EditorCurveBinding spriteBinding = new EditorCurveBinding();
        spriteBinding.type = typeof(SpriteRenderer);
        spriteBinding.path = "";
        spriteBinding.propertyName = "m_Sprite";

        // Filter out nulls
        sprites = sprites.Where(s => s != null).ToArray();
        if (sprites.Length == 0) return clip;

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i / frameRate;
            keyframes[i].value = sprites[i];
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }
}
