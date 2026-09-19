using UnityEngine;
using UnityEditor;

public class FixAn_SceneAuto
{
    [MenuItem("Tools/An/Fix Scene Auto")]
    public static void FixScene()
    {
        AnAnimationSetupAuto.SetupAnAnimations();
        AssetDatabase.Refresh();

        string spritesheetPath = "Assets/_Project/Art/Sprites/An/An_idle.png";
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spritesheetPath);
        
        Sprite[] idleFrames = new Sprite[4];
        int loadedCount = 0;

        for (int i = 0; i < 4; i++)
        {
            string targetName = "An_idle_" + i;
            foreach (var asset in allAssets)
            {
                if (asset is Sprite sp && sp.name == targetName)
                {
                    idleFrames[i] = sp;
                    loadedCount++;
                    break;
                }
            }
        }

        if (loadedCount < 4)
        {
            Debug.LogError("[Fix Scene] Chưa có đủ 4 frame cho An_idle. Vui lòng chạy lại lệnh.");
            return;
        }

        QuadAnimator[] allQuads = Object.FindObjectsByType<QuadAnimator>(FindObjectsSortMode.None);
        foreach (var quad in allQuads)
        {
            if (quad.transform.parent != null && IsAnCharacter(quad.transform.parent.name))
            {
                UpdateQuad(quad, idleFrames);
            }
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (IsAnCharacter(fileName))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    QuadAnimator[] prefQuads = prefab.GetComponentsInChildren<QuadAnimator>();
                    foreach (var quad in prefQuads)
                    {
                        if (quad.transform.parent == null || IsAnCharacter(quad.transform.parent.name) || IsAnCharacter(prefab.name))
                        {
                            UpdateQuad(quad, idleFrames);
                            PrefabUtility.SavePrefabAsset(prefab);
                        }
                    }
                }
            }
        }

        Debug.Log("[Fix Scene] Hoàn tất gắn An_idle!");
    }

    private static bool IsAnCharacter(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        name = name.Trim().ToLower();
        return name == "an" || name.StartsWith("an ") || name.StartsWith("an_") || name.StartsWith("an(") || name.Contains("ally_an") || name.Contains("an_ally");
    }

    private static void UpdateQuad(QuadAnimator quad, Sprite[] idleFrames)
    {
        if (quad.gameObject.name.Contains("Shadow")) return;

        Debug.Log($"[Fix Scene] Đã cập nhật QuadAnimator của {quad.transform.parent?.name ?? quad.name}");
        
        if (idleFrames.Length > 0)
        {
            quad.stanceSprite = idleFrames[0];
        }
        quad.idleSprites = idleFrames;
        
        quad.dashSprites = new Sprite[0];
        quad.attackSprites = new Sprite[0];
        quad.downedSprites = new Sprite[0];
        quad.hitSprites = new Sprite[0];
        quad.appearSprites = new Sprite[0];
        quad.jumpSprites = new Sprite[0];
        
        quad.customOffset = new Vector3(0, -1.195876f, 0);

        Renderer renderer = quad.GetComponent<Renderer>();
        if (quad.GetComponent<Billboard>() == null)
        {
            Undo.AddComponent<Billboard>(quad.gameObject);
        }
        if (renderer != null && idleFrames.Length > 0 && idleFrames[0] != null)
        {
            Texture2D tex = idleFrames[0].texture;
            Sprite sprite = idleFrames[0];
            
            float ppu = sprite.pixelsPerUnit > 0 ? sprite.pixelsPerUnit : 100f;
            float width = sprite.rect.width / ppu;
            float height = sprite.rect.height / ppu;
            float paddingScale = quad.paddingScale > 0 ? quad.paddingScale : 1.1f;
            quad.transform.localScale = new Vector3(width * paddingScale, height * paddingScale, 1f);

            if (renderer.sharedMaterial != null)
            {
                renderer.sharedMaterial.mainTexture = tex;
                if (renderer.sharedMaterial.HasProperty("_BaseMap"))
                {
                    renderer.sharedMaterial.SetTexture("_BaseMap", tex);
                }

                Vector4 outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(idleFrames[0]);
                float uvOffsetX = outerUV.x;
                float uvOffsetY = outerUV.y;
                float uvScaleX = outerUV.z - outerUV.x;
                float uvScaleY = outerUV.w - outerUV.y;

                float paddedScaleX = uvScaleX * paddingScale;
                float paddedScaleY = uvScaleY * paddingScale;
                float paddedOffX = uvOffsetX - (paddedScaleX - uvScaleX) / 2f;
                float paddedOffY = uvOffsetY - (paddedScaleY - uvScaleY) / 2f;

                renderer.sharedMaterial.mainTextureScale = new Vector2(paddedScaleX, paddedScaleY);
                renderer.sharedMaterial.mainTextureOffset = new Vector2(paddedOffX, paddedOffY);

                if (renderer.sharedMaterial.HasProperty("_BaseMap"))
                {
                    renderer.sharedMaterial.SetTextureScale("_BaseMap", new Vector2(paddedScaleX, paddedScaleY));
                    renderer.sharedMaterial.SetTextureOffset("_BaseMap", new Vector2(paddedOffX, paddedOffY));
                }
                
                renderer.sharedMaterial.SetVector("_SpriteUVRect", new Vector4(uvOffsetX, uvOffsetY, uvOffsetX + uvScaleX, uvOffsetY + uvScaleY));
            }
        }

        EditorUtility.SetDirty(quad);

        if (quad.transform.parent != null)
        {
            CharacterInteraction charInt = quad.transform.parent.GetComponent<CharacterInteraction>();
            if (charInt != null)
            {
                charInt.quadAnimator = quad;
                EditorUtility.SetDirty(charInt);
            }
        }
    }
}
