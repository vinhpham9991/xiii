using UnityEngine;
using UnityEditor;
using System.Linq;

public class VFXSetup : EditorWindow
{
    [MenuItem("Tools/Franken XIII/Setup VFX Prefabs")]
    public static void CreateVFX()
    {
        // 1. Tạo thư mục nếu chưa có
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs/VFX"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "VFX");
        }
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Sprites/VFX"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Sprites"))
                AssetDatabase.CreateFolder("Assets/_Project", "Sprites");
            AssetDatabase.CreateFolder("Assets/_Project/Sprites", "VFX");
        }

        // 2. Copy ảnh từ ngoài vào Project (do AssetDatabase chỉ đọc file trong Project)
        string extSlash = @"D:/Work/UNity/Assets/2D/VFX/pixel-art-slashes/128x128/Slash 1/color1/sprite-sheet.png";
        string extHit = @"D:/Work/UNity/Assets/2D/VFX/effect-and-fx-pixel-part-6/Free/273.png";
        
        string localSlash = "Assets/_Project/Sprites/VFX/slash_sprite-sheet.png";
        string localHit = "Assets/_Project/Sprites/VFX/hit_273.png";

        if (System.IO.File.Exists(extSlash))
            System.IO.File.Copy(extSlash, localSlash, true);
        else
            Debug.LogWarning("Không tìm thấy ảnh tại: " + extSlash);

        if (System.IO.File.Exists(extHit))
            System.IO.File.Copy(extHit, localHit, true);
        else
            Debug.LogWarning("Không tìm thấy ảnh tại: " + extHit);

        AssetDatabase.Refresh();

        // 3. Load Sprites (yêu cầu người dùng setup Multiple nếu chưa)
        Object[] slashAssets = AssetDatabase.LoadAllAssetsAtPath(localSlash);
        Sprite[] slashSprites = slashAssets.OfType<Sprite>().OrderBy(s => s.name).ToArray();

        Object[] hitAssets = AssetDatabase.LoadAllAssetsAtPath(localHit);
        Sprite[] hitSprites = hitAssets.OfType<Sprite>().OrderBy(s => s.name).ToArray();

        if (slashSprites.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy Slash Sprites! Đảm bảo ảnh đã được set thành Multiple Sprite.");
        }

        // 4. Tạo Slash Prefab
        GameObject slashObj = new GameObject("VFX_Slash");
        slashObj.transform.localScale = new Vector3(3f, 3f, 3f); // Phóng to Slash
        SimpleVFXPlayer slashPlayer = slashObj.AddComponent<SimpleVFXPlayer>();
        slashPlayer.frames = slashSprites;
        slashPlayer.frameRate = 90f; // Tăng tốc độ phát
        GameObject slashPrefab = PrefabUtility.SaveAsPrefabAsset(slashObj, "Assets/_Project/Prefabs/VFX/VFX_Slash.prefab");
        DestroyImmediate(slashObj);

        // 5. Tạo Hit Prefab
        GameObject hitObj = new GameObject("VFX_Hit");
        hitObj.transform.localScale = new Vector3(3f, 3f, 3f); // Phóng to Hit
        SimpleVFXPlayer hitPlayer = hitObj.AddComponent<SimpleVFXPlayer>();
        hitPlayer.frames = hitSprites;
        hitPlayer.frameRate = 90f; // Tăng tốc độ phát
        GameObject hitPrefab = PrefabUtility.SaveAsPrefabAsset(hitObj, "Assets/_Project/Prefabs/VFX/VFX_Hit.prefab");
        DestroyImmediate(hitObj);

        // 6. Gán vào BattleManager
        BattleManager bm = FindObjectOfType<BattleManager>();
        if (bm != null)
        {
            bm.slashVFXPrefab = slashPrefab;
            bm.hitVFXPrefab = hitPrefab;
            EditorUtility.SetDirty(bm);
            Debug.Log("Đã setup thành công VFX_Slash và VFX_Hit vào BattleManager!");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy BattleManager trên Scene.");
        }
    }
}
