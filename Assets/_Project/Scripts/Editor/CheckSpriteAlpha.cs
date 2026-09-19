using UnityEngine;
using UnityEditor;

public class CheckSpriteAlpha
{
    [MenuItem("Tools/Check Sprite Alpha")]
    public static void Run()
    {
        CheckSprite("Assets/_Project/Art/Sprites/An/An_idle.png", "An_idle_0");
        CheckSprite("Assets/_Project/Art/Sprites/XIII/XIII_idle.png", "XIII_idle_0");
    }

    private static void CheckSprite(string path, string frameName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        Sprite targetSprite = null;
        foreach (var asset in assets)
        {
            if (asset is Sprite s && s.name == frameName)
            {
                targetSprite = s;
                break;
            }
        }

        if (targetSprite == null)
        {
            Debug.Log($"Sprite {frameName} not found at {path}");
            return;
        }

        Texture2D tex = targetSprite.texture;
        
        // Cần Texture được đánh dấu là Readable
        string assetPath = AssetDatabase.GetAssetPath(tex);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        bool wasReadable = importer.isReadable;
        if (!wasReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        Rect r = targetSprite.rect;
        int minX = (int)r.width, maxX = 0, minY = (int)r.height, maxY = 0;
        
        Color[] pixels = tex.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
        int w = (int)r.width;
        int h = (int)r.height;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a > 0.1f)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (!wasReadable)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        float aspect = (float)width / height;
        
        Debug.Log($"{frameName} non-transparent rect: width={width}, height={height}, aspect={aspect}. Overall Sprite rect: {r.width}x{r.height}");
    }
}
