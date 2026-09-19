using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class XIIIAnimationSetupAuto : EditorWindow
{
    [MenuItem("Tools/XIII/Setup Idle Animation")]
    public static void SetupIdleAnimation()
    {
        string spritesheetPath = "Assets/_Project/Art/Sprites/XIII/XIII_idle.png";
        string animClipPath = "Assets/_Project/Art/Sprites/XIII/XIII_idle.anim";
        string animControllerPath = "Assets/_Project/Art/Sprites/XIII/XIII.controller";

        // --- 1. SET UP TEXTURE IMPORT SETTINGS ---
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spritesheetPath);
        if (importer == null)
        {
            Debug.LogError("[XIII Setup] Không tìm thấy file XIII_idle.png tại: " + spritesheetPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.spritePixelsPerUnit = 300; // Điều chỉnh PPU để XIII vừa vặn với scene (tăng = nhỏ lại)

        // Slice 8 frames manually (spritesheet 2896x543, 8 frames)
        int frameCount = 8;
        int totalWidth = 2896;
        int frameHeight = 543;
        int frameWidth = totalWidth / frameCount; // 362

        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        for (int i = 0; i < frameCount; i++)
        {
            SpriteMetaData meta = new SpriteMetaData();
            meta.name = "XIII_idle_" + i;
            meta.rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight);
            meta.pivot = new Vector2(0.5f, 0f); // Pivot bottom-center
            meta.alignment = (int)SpriteAlignment.Custom;
            sprites.Add(meta);
        }

        importer.spritesheet = sprites.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("[XIII Setup] Đã slice spritesheet thành " + frameCount + " frames.");

        // --- 2. LOAD SPRITES ---
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spritesheetPath);
        List<Sprite> loadedSprites = new List<Sprite>();
        for (int i = 0; i < frameCount; i++)
        {
            string targetName = "XIII_idle_" + i;
            foreach (var asset in allAssets)
            {
                if (asset is Sprite sp && sp.name == targetName)
                {
                    loadedSprites.Add(sp);
                    break;
                }
            }
        }

        if (loadedSprites.Count != frameCount)
        {
            Debug.LogError($"[XIII Setup] Tải Sprite thất bại! Chỉ tải được {loadedSprites.Count}/{frameCount} sprites.");
            return;
        }

        // --- 3. CREATE ANIMATION CLIP ---
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 8; // 8 FPS cho animation idle mượt

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = ""; // applies to the root GameObject
        binding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frameCount + 1];
        float frameDuration = 1f / clip.frameRate;

        for (int i = 0; i < frameCount; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = loadedSprites[i]
            };
        }

        // Repeat: last keyframe = first frame
        keyframes[frameCount] = new ObjectReferenceKeyframe
        {
            time = frameCount * frameDuration,
            value = loadedSprites[0]
        };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Set loop
        SerializedObject clipSO = new SerializedObject(clip);
        clipSO.FindProperty("m_AnimationClipSettings.m_LoopTime").boolValue = true;
        clipSO.ApplyModifiedProperties();

        // Save clip
        AssetDatabase.CreateAsset(clip, animClipPath);
        Debug.Log("[XIII Setup] Đã tạo AnimationClip: " + animClipPath);

        // --- 4. CREATE ANIMATOR CONTROLLER ---
        var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(animControllerPath);
        var rootSM = controller.layers[0].stateMachine;

        var idleState = rootSM.AddState("Idle");
        idleState.motion = clip;
        rootSM.defaultState = idleState;

        Debug.Log("[XIII Setup] Đã tạo AnimatorController: " + animControllerPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("XIII Setup", "✅ Hoàn tất!\n\nĐã slice spritesheet, tạo AnimationClip Idle (loop) và AnimatorController cho nhân vật XIII.\n\nGán AnimatorController vào nhân vật XIII trong Scene là xong!", "OK");
    }

    [MenuItem("Tools/XIII/Setup Jump Animation")]
    public static void SetupJumpAnimation()
    {
        string spritesheetPath = "Assets/_Project/Art/Sprites/XIII/XIII_jump.png";

        // --- 1. SET UP TEXTURE IMPORT SETTINGS ---
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spritesheetPath);
        if (importer == null)
        {
            Debug.LogError("[XIII Setup] Không tìm thấy file XIII_jump.png tại: " + spritesheetPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.spritePixelsPerUnit = 300; 

        int totalWidth = 2832;
        int frameHeight = 433;

        int frameCount = 8;
        int frameWidth = totalWidth / frameCount;

        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        for (int i = 0; i < frameCount; i++)
        {
            SpriteMetaData meta = new SpriteMetaData();
            meta.name = "XIII_jump_" + i;
            meta.rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight);
            meta.pivot = new Vector2(0.5f, 0f); // Pivot bottom-center
            meta.alignment = (int)SpriteAlignment.Custom;
            sprites.Add(meta);
        }

        importer.spritesheet = sprites.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("[XIII Setup] Đã slice XIII_jump.png thành " + frameCount + " frames.");
    }

    [MenuItem("Tools/XIII/Setup Attack Animation")]
    public static void SetupAttackAnimation()
    {
        string spritesheetPath = "Assets/_Project/Art/Sprites/XIII/XIII_attack.png";

        // --- 1. SET UP TEXTURE IMPORT SETTINGS ---
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spritesheetPath);
        if (importer == null)
        {
            Debug.LogError("[XIII Setup] Không tìm thấy file XIII_attack.png tại: " + spritesheetPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.spritePixelsPerUnit = 300; 

        int totalWidth = 2864;
        int frameHeight = 526;

        int frameCount = 8;
        int frameWidth = totalWidth / frameCount;

        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        for (int i = 0; i < frameCount; i++)
        {
            SpriteMetaData meta = new SpriteMetaData();
            meta.name = "XIII_attack_" + i;
            meta.rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight);
            meta.pivot = new Vector2(0.5f, 0f); // Pivot bottom-center
            meta.alignment = (int)SpriteAlignment.Custom;
            sprites.Add(meta);
        }

        importer.spritesheet = sprites.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("[XIII Setup] Đã slice XIII_attack.png thành " + frameCount + " frames.");
    }

    [MenuItem("Tools/XIII/Setup Hit Animation")]
    public static void SetupHitAnimation()
    {
        string spritesheetPath = "Assets/_Project/Art/Sprites/XIII/XIII_hit.png";

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spritesheetPath);
        if (importer == null)
        {
            Debug.LogError("[XIII Setup] Không tìm thấy file XIII_hit.png tại: " + spritesheetPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.spritePixelsPerUnit = 300; 

        int totalWidth = 2122;
        int frameHeight = 497;
        
        // Dynamically read from file to avoid squishing
        if (System.IO.File.Exists(spritesheetPath)) {
            byte[] fileData = System.IO.File.ReadAllBytes(spritesheetPath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData)) {
                totalWidth = tex.width;
                frameHeight = tex.height;
            }
        }

        int frameCount = 4; // Hit now has 4 frames
        int frameWidth = totalWidth / frameCount;

        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        for (int i = 0; i < frameCount; i++)
        {
            SpriteMetaData meta = new SpriteMetaData();
            meta.name = "XIII_hit_" + i;
            meta.rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight);
            meta.pivot = new Vector2(0.5f, 0f); // Pivot bottom-center
            meta.alignment = (int)SpriteAlignment.Custom;
            sprites.Add(meta);
        }

        importer.spritesheet = sprites.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();

        Debug.Log("[XIII Setup] Đã slice XIII_hit.png thành " + frameCount + " frames.");
    }
}
