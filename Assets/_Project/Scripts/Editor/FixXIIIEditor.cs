using UnityEngine;
using UnityEditor;

public class FixXIIIEditor : EditorWindow
{
    [MenuItem("Tools/Fix XIII Prefab")]
    public static void FixPrefab()
    {
        // Fix Sprites Pivot to Bottom Center
        string spritesFolder = "Assets/_Project/Sprites/XIII-2";
        string[] guids = AssetDatabase.FindAssets("t:texture2D", new[] { spritesFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                
                if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || importer.wrapMode != TextureWrapMode.Clamp || importer.spritePixelsPerUnit != 400f)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 400f;
                    
                    TextureImporterSettings newSettings = new TextureImporterSettings();
                    importer.ReadTextureSettings(newSettings);
                    importer.SetTextureSettings(newSettings);
                    
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.SaveAndReimport();
                }
            }
        }
        
        // Load the new sprite (we use stance)
        Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-stance.png");
        if (newSprite == null)
        {
            Debug.LogError("Could not find XIII stance.png");
            return;
        }

        bool changed = false;

        CharacterInteraction[] chars = FindObjectsOfType<CharacterInteraction>();
        foreach (var c in chars)
        {
            if (c.characterName.Contains("XIII"))
            {
                Transform visual = c.transform.Find("Visual");
                if (visual != null)
                {
                    // Revert from SpriteRenderer to MeshRenderer
                    SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
                    Material oldMat = null;
                    if (sr != null)
                    {
                        oldMat = sr.sharedMaterial;
                        DestroyImmediate(sr);
                    }
                    
                    // Also destroy Unity Animator since we use QuadAnimator now
                    Animator unityAnim = visual.GetComponent<Animator>();
                    if (unityAnim != null) DestroyImmediate(unityAnim);

                    MeshFilter mf = visual.GetComponent<MeshFilter>();
                    if (mf == null) mf = visual.gameObject.AddComponent<MeshFilter>();
                    
                    // Create Quad Mesh
                    GameObject tempQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    mf.sharedMesh = tempQuad.GetComponent<MeshFilter>().sharedMesh;
                    DestroyImmediate(tempQuad);

                    MeshRenderer mr = visual.GetComponent<MeshRenderer>();
                    if (mr == null) mr = visual.gameObject.AddComponent<MeshRenderer>();
                    
                    if (oldMat != null) mr.sharedMaterial = oldMat;
                    else
                    {
                        Material glowMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/XIII_Glow.mat");
                        if (glowMat == null) glowMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/CharacterGlow.mat"); // Try another common name
                        if (glowMat != null) mr.sharedMaterial = glowMat;
                    }
                    
                    // Put the texture into material
                    if (mr.sharedMaterial != null && newSprite != null)
                    {
                        mr.sharedMaterial.mainTexture = newSprite.texture;
                        EditorUtility.SetDirty(mr.sharedMaterial);
                    }

                    // Auto scale based on sprite size and padding
                    float paddingScale = 1.5f;
                    float ppu = newSprite.pixelsPerUnit > 0 ? newSprite.pixelsPerUnit : 100f;
                    float width = newSprite.rect.width / ppu;
                    float height = newSprite.rect.height / ppu;
                    
                    visual.localScale = new Vector3(width * paddingScale, height * paddingScale, 1f);

                    // Setup QuadAnimator
                    QuadAnimator qAnim = visual.GetComponent<QuadAnimator>();
                    if (qAnim == null) qAnim = visual.gameObject.AddComponent<QuadAnimator>();
                    
                    qAnim.stanceSprite = newSprite;
                    qAnim.attackSprites = new Sprite[] {
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-attack-1.png"),
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-attack-2.png"),
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-attack-3.png")
                    };
                    qAnim.hitSprites = new Sprite[] {
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-hit.png")
                    };
                    qAnim.downedSprites = new Sprite[] {
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-downed-1.png"),
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-downed-2.png")
                    };
                    qAnim.dashSprites = new Sprite[] {
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-dash.png")
                    };
                    qAnim.appearSprites = new Sprite[] {
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-appear-1.png"),
                        AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Sprites/XIII-2/XIII-appear-2.png")
                    };

                    EditorUtility.SetDirty(c.gameObject);
                    changed = true;
                    Debug.Log("Successfully Reverted and Fixed " + c.gameObject.name);
                }
            }
        }

        if (changed)
        {
            Debug.Log("Successfully fixed XIII in the scene! Press Play to test.");
            EditorApplication.RepaintHierarchyWindow();
        }
        else
        {
            Debug.LogWarning("Could not find XIII in the scene.");
        }
    }
}
