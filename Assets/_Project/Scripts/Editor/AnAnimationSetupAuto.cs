using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AnAnimationSetupAuto : EditorWindow
{
    public const string IdleSpritesheetPath = "Assets/_Project/Art/Sprites/An/An_idle.png";
    public const int IdleFrameCount = 4;

    [MenuItem("Tools/An/Setup Animations")]
    public static void SetupAnAnimations()
    {
        SetupIdleAnimation();
    }

    private static void SetupIdleAnimation()
    {
        const string spriteName = "An_idle";
        const string spritesheetPath = IdleSpritesheetPath;

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(spritesheetPath);
        if (importer == null)
        {
            Debug.LogError("[An Setup] Không tìm thấy file " + spriteName + " tại: " + spritesheetPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp; // Prevent edge bleeding
        importer.spritePixelsPerUnit = 300; 

        int totalWidth = 2048;
        int totalHeight = 512;
        
        string absolutePath = Application.dataPath + spritesheetPath.Substring(6);
        if (File.Exists(absolutePath))
        {
            byte[] fileData = File.ReadAllBytes(absolutePath);
            Texture2D tex = new Texture2D(2, 2);
            if (tex.LoadImage(fileData))
            {
                totalWidth = tex.width;
                totalHeight = tex.height;
                DestroyImmediate(tex);
            }
        }

        int frameCount = IdleFrameCount;
        int frameWidth = totalWidth / frameCount;

        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        for (int i = 0; i < frameCount; i++)
        {
            SpriteMetaData smd = new SpriteMetaData
            {
                name = spriteName + "_" + i,
                rect = new Rect(i * frameWidth, 0, frameWidth, totalHeight),
                alignment = (int)SpriteAlignment.Custom,
                // Giữ chân nhân vật trên cùng một baseline như XIII.
                pivot = new Vector2(0.5f, 0f)
            };
            sprites.Add(smd);
        }

        importer.spritesheet = sprites.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        
        Debug.Log($"[An Setup] Đã cắt {spriteName} thành {frameCount} frames ({frameWidth}x{totalHeight})!");
    }
}
