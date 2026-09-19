using UnityEngine;
using UnityEditor;
using System.Linq;

public class FixXIII_SceneAuto : EditorWindow
{
    [MenuItem("Tools/XIII/Fix Scene Auto")]
    public static void FixScene()
    {
        // Chạy lại setup để đảm bảo PPU = 300 (nhân vật thu nhỏ lại)
        XIIIAnimationSetupAuto.SetupIdleAnimation();
        XIIIAnimationSetupAuto.SetupJumpAnimation();
        XIIIAnimationSetupAuto.SetupAttackAnimation();
        XIIIAnimationSetupAuto.SetupHitAnimation();

        // 1. Tải spritesheet và trích xuất 8 frame
        string spritesheetPath = "Assets/_Project/Art/Sprites/XIII/XIII_idle.png";
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(spritesheetPath);
        Sprite[] idleFrames = new Sprite[8];
        int loadedCount = 0;

        for (int i = 0; i < 8; i++)
        {
            string targetName = "XIII_idle_" + i;
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

        if (loadedCount < 8)
        {
            Debug.LogError("[Fix Scene] Chưa có đủ 8 frame. Đại ca vui lòng chạy Tools -> XIII -> Setup Idle Animation trước nhé!");
            return;
        }

        // Tải jump frames
        string jumpSpritesheetPath = "Assets/_Project/Art/Sprites/XIII/XIII_jump.png";
        Object[] allJumpAssets = AssetDatabase.LoadAllAssetsAtPath(jumpSpritesheetPath);
        Sprite[] jumpFrames = new Sprite[8];
        int loadedJumpCount = 0;

        for (int i = 0; i < 8; i++)
        {
            string targetName = "XIII_jump_" + i;
            foreach (var asset in allJumpAssets)
            {
                if (asset is Sprite sp && sp.name == targetName)
                {
                    jumpFrames[i] = sp;
                    loadedJumpCount++;
                    break;
                }
            }
        }

        // Tải attack frames
        string attackSpritesheetPath = "Assets/_Project/Art/Sprites/XIII/XIII_attack.png";
        Object[] allAttackAssets = AssetDatabase.LoadAllAssetsAtPath(attackSpritesheetPath);
        Sprite[] attackFrames = new Sprite[8];
        int loadedAttackCount = 0;

        for (int i = 0; i < 8; i++)
        {
            string targetName = "XIII_attack_" + i;
            foreach (var asset in allAttackAssets)
            {
                if (asset is Sprite sp && sp.name == targetName)
                {
                    attackFrames[i] = sp;
                    loadedAttackCount++;
                    break;
                }
            }
        }

        // Tải hit frames
        string hitSpritesheetPath = "Assets/_Project/Art/Sprites/XIII/XIII_hit.png";
        Object[] allHitAssets = AssetDatabase.LoadAllAssetsAtPath(hitSpritesheetPath);
        Sprite[] hitFrames = new Sprite[4];
        int loadedHitCount = 0;

        for (int i = 0; i < 4; i++)
        {
            string targetName = "XIII_hit_" + i;
            foreach (var asset in allHitAssets)
            {
                if (asset is Sprite sp && sp.name == targetName)
                {
                    hitFrames[i] = sp;
                    loadedHitCount++;
                    break;
                }
            }
        }

        // 2. Tìm nhân vật XIII trong scene
        QuadAnimator[] allQuads = FindObjectsByType<QuadAnimator>(FindObjectsSortMode.None);
        bool found = false;

        foreach (var quad in allQuads)
        {
            if (quad.transform.parent != null && quad.transform.parent.name.Contains("XIII"))
            {
                if (quad.gameObject.name.Contains("Shadow")) continue;

                Debug.Log($"[Fix Scene] Đã tìm thấy QuadAnimator của {quad.transform.parent.name}");
                
                // Gán frames
                quad.idleSprites = idleFrames;
                if (loadedJumpCount == 8) quad.jumpSprites = jumpFrames;
                if (loadedAttackCount == 8) quad.attackSprites = attackFrames;
                if (loadedHitCount == 4) quad.hitSprites = hitFrames;
                
                EditorUtility.SetDirty(quad);

                // Tìm và gán quadAnimator cho script ở GameObject cha
                var entrance = quad.transform.parent.GetComponent<EntranceAnimation>();
                if (entrance != null)
                {
                    var serializedObject = new SerializedObject(entrance);
                    var prop = serializedObject.FindProperty("quadAnimator");
                    if (prop != null)
                    {
                        prop.objectReferenceValue = quad;
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                found = true;
            }
        }

        if (!found)
        {
            Debug.LogError("[Fix Scene] Không tìm thấy QuadAnimator nào là con của XIII trong Scene!");
            return;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[Fix Scene] HOÀN TẤT! Đã gán idle animation, jump animation và kết nối reference thành công.");
    }
}
