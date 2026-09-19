using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AnAnimationSetupAuto : EditorWindow
{
    [MenuItem("Tools/An/Setup Animations")]
    public static void SetupAnAnimations()
    {
        SetupIdleAnimation();
    }

    private static void SetupIdleAnimation()
    {
        string spriteName = "An_idle";
        string spritesheetPath = "Assets/_Project/Art/Sprites/An/" + spriteName + ".png";

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

        int frameCount = 4;
        int frameWidth = totalWidth / frameCount;

        List<SpriteMetaData> sprites = new List<SpriteMetaData>();
        for (int i = 0; i < frameCount; i++)
        {
            SpriteMetaData smd = new SpriteMetaData
            {
                name = spriteName + "_" + i,
                rect = new Rect(i * frameWidth, 0, frameWidth, totalHeight),
                alignment = 9, 
                pivot = new Vector2(0.5f, 0.5f)
            };
            sprites.Add(smd);
        }

        importer.spritesheet = sprites.ToArray();
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
        
        Debug.Log($"[An Setup] Đã cắt {spriteName} thành {frameCount} frames ({frameWidth}x{totalHeight})!");
    }
}
