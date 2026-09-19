using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class AddFrameEditor : EditorWindow
{
    [MenuItem("Tools/Add Battle Frame")]
    public static void AddFrame()
    {
        // Force import Frame.png as Sprite
        string path = "Assets/_Project/Sprites/Frame.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (frameSprite == null)
        {
            Debug.LogError("Could not find Frame.png at " + path);
            return;
        }

        // Find Canvas
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null) canvas = canvasObj.GetComponent<Canvas>();
            
            // If still null, try finding any Canvas including inactive
            if (canvas == null)
            {
                Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (Canvas c in canvases)
                {
                    if (c.gameObject.scene.isLoaded) // Ensure it's in the scene, not a prefab
                    {
                        canvas = c;
                        break;
                    }
                }
            }
        }

        if (canvas == null)
        {
            Debug.Log("No Canvas found. Creating a new one for the Battle Frame!");
            GameObject newCanvasObj = new GameObject("Canvas");
            canvas = newCanvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = newCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            newCanvasObj.AddComponent<GraphicRaycaster>();
        }

        // Check if Frame already exists
        Transform existingFrame = canvas.transform.Find("BattleFrame");
        if (existingFrame != null)
        {
            DestroyImmediate(existingFrame.gameObject);
        }

        // Create UI Image
        GameObject frameObj = new GameObject("BattleFrame");
        frameObj.transform.SetParent(canvas.transform, false);
        frameObj.transform.SetAsLastSibling(); // Put it on top of everything

        Image img = frameObj.AddComponent<Image>();
        img.sprite = frameSprite;
        img.raycastTarget = false; // So it doesn't block button clicks!

        // Stretch to fill
        RectTransform rt = frameObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        EditorUtility.SetDirty(canvas.gameObject);
        Debug.Log("Successfully added Battle Frame to Canvas!");
        EditorApplication.RepaintHierarchyWindow();
    }
}
