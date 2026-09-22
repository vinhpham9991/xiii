using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using FrankenXIII.Combat.Domain;

public class BattleSceneGenerator
{
    [MenuItem("Tools/Franken XIII/1. Generate 2.5D Battle Placeholder")]
    public static void GenerateScene()
    {
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1 & 7: Setup Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        
        // ĐẠI CA CHÚ Ý: Chuyển sang Perspective để có chiều sâu (Depth) y hệt Scene View (ảnh 2)
        // Ortho mặc định sẽ làm mọi thứ phẳng lỳ, khiến Ground xám trông như một bức tường.
        cam.orthographic = false; 
        cam.fieldOfView = 45f; // FOV hẹp để giữ được chất 2.5D (tránh méo góc)
        
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black; 
        
        camObj.transform.position = new Vector3(0, 8, -15);
        camObj.transform.rotation = Quaternion.Euler(20, 0, 0); // Góc chúc xuống 20 độ để thấy rõ mặt phẳng
        camObj.AddComponent<AudioListener>();
        camObj.AddComponent<CameraShake>();

        Shader safeUnlitShader = Shader.Find("Custom/URP_SpriteGlow");
        if (safeUnlitShader == null) safeUnlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (safeUnlitShader == null) safeUnlitShader = Shader.Find("Unlit/Color");
        if (safeUnlitShader == null) safeUnlitShader = Shader.Find("Sprites/Default");

        // 3: Physical Black Background
        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "Background_Black";
        bg.transform.position = camObj.transform.position + camObj.transform.forward * 100f;
        bg.transform.localScale = new Vector3(500f, 500f, 1f);
        bg.transform.rotation = camObj.transform.rotation; 
        Material blackMat = new Material(safeUnlitShader);
        blackMat.color = Color.black;
        bg.GetComponent<Renderer>().sharedMaterial = blackMat;

        // 2: Tạo Ground 3D bằng model Swamp Villa
        string basePath = "Assets/_Project/Art/Environment/abandoned-swamp-villa/";
        string fbxPath = basePath + "Meshy_AI_Abandoned_Swamp_Villa_0909154754_texture.fbx";
        GameObject groundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        
        GameObject ground;
        if (groundPrefab != null)
        {
            ground = (GameObject)PrefabUtility.InstantiatePrefab(groundPrefab);
            ground.name = "Swamp_Villa_Environment";
            
            // Set thông số Transform chuẩn đã được đại ca căn chỉnh
            ground.transform.position = new Vector3(0f, 5.6f, 10.18f);
            ground.transform.rotation = Quaternion.Euler(-90f, 180f, 0f);
            ground.transform.localScale = new Vector3(2600f, 2600f, 2600f);

            // ĐẠI CA CHÚ Ý: Thêm MeshCollider để nhà hoang có thể đỡ được nhân vật rơi xuống
            MeshCollider mc = ground.AddComponent<MeshCollider>();
            // Model FBX có nhiều sub-mesh, nếu muốn chuẩn nhất thì nên tạo BoxCollider thủ công,
            // nhưng tạm thời xài MeshCollider cũng ok.

            // Fix Texture Import Settings
            FixTexture(basePath + "Meshy_AI_Abandoned_Swamp_Villa_0909154754_texture_normal.png", true, true);
            FixTexture(basePath + "Meshy_AI_Abandoned_Swamp_Villa_0909154754_texture_metallic.png", false, true);
            FixTexture(basePath + "Meshy_AI_Abandoned_Swamp_Villa_0909154754_texture_roughness.png", false, true);

            Texture2D albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath + "Meshy_AI_Abandoned_Swamp_Villa_0909154754_texture.png");
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath + "Meshy_AI_Abandoned_Swamp_Villa_0909154754_texture_normal.png");

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            
            Material villaMat = new Material(litShader);
            if (albedo != null) { villaMat.SetTexture("_MainTex", albedo); villaMat.SetTexture("_BaseMap", albedo); }
            if (normal != null) { villaMat.SetTexture("_BumpMap", normal); villaMat.EnableKeyword("_NORMALMAP"); }
            
            if (litShader.name.Contains("Universal"))
            {
                villaMat.SetFloat("_Smoothness", 0.1f);
                villaMat.SetFloat("_Metallic", 0f);
            }
            else
            {
                villaMat.SetFloat("_Glossiness", 0.1f);
                villaMat.SetFloat("_Metallic", 0f);
            }

            Renderer[] renderers = ground.GetComponentsInChildren<Renderer>();
            foreach(var r in renderers)
            {
                r.sharedMaterial = villaMat;
            }
        }
        else
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Battle_Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10f, 1f, 10f); 
            Material grayMat = new Material(safeUnlitShader);
            grayMat.color = new Color(0.3f, 0.3f, 0.3f); 
            ground.GetComponent<Renderer>().sharedMaterial = grayMat;
            ground.AddComponent<MeshCollider>(); // Thêm collider cho ground dự phòng
            Debug.LogWarning("Không tìm thấy FBX Swamp Villa tại: " + fbxPath);
        }

        AssetDatabase.Refresh(); // Cập nhật lại thư mục để Unity nhận diện các file ảnh vừa copy vào

        // Fix Texture Import Settings cho 2 ảnh quái vật Đỏ
        FixSpriteTexture("Assets/_Project/Art/Characters/Boss (1).png");
        FixSpriteTexture("Assets/_Project/Art/Characters/Enemy (1).png");

        // Load hình ảnh nhân vật cho phe Đỏ (Enemy)
        Texture2D texBoss = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Characters/Boss (1).png");
        Material matBoss = new Material(safeUnlitShader);
        matBoss.color = Color.white; 
        if (texBoss != null) {
            matBoss.SetTexture("_BaseMap", texBoss);
            matBoss.SetTexture("_MainTex", texBoss);
            SetMaterialTransparent(matBoss);
        } else Debug.LogError("Không tìm thấy ảnh Boss (1).png!");

        Texture2D texEnemy = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Characters/Enemy (1).png");
        Material matEnemy = new Material(safeUnlitShader);
        matEnemy.color = Color.white;
        if (texEnemy != null) {
            matEnemy.SetTexture("_BaseMap", texEnemy);
            matEnemy.SetTexture("_MainTex", texEnemy);
            SetMaterialTransparent(matEnemy);
        } else Debug.LogError("Không tìm thấy ảnh Enemy (1).png!");

        // Đội hình Enemy (Đỏ) - Bên trái, spawn ngoài màn hình X = -15
        Vector3 enemyOffset = new Vector3(-15f, 0, 0);
        // Boss đỏ xuất hiện trễ 0.2s và phi vào siêu nhanh (speed 40)
        Create2DCharacter(new Vector3(-8f, 10f, 0f), enemyOffset, matBoss, "Enemy_Boss", safeUnlitShader, true, texBoss, 2.168f, 0.2f, 40f, true, false, 1000, "Bách Mệnh Quan", DemoCombatantId.BachMenhQuan);
        // Quái nhỏ 
        Create2DCharacter(new Vector3(-6, 10f, 3f), enemyOffset, matEnemy, "Enemy_Support_Top", safeUnlitShader, true, texEnemy, 1f, 0f, 20f, false, false, 100, "Hộc Tử Thi Trái", DemoCombatantId.LeftCorpseDrawer);
        Create2DCharacter(new Vector3(-6, 10f, -3f), enemyOffset, matEnemy, "Enemy_Support_Bot", safeUnlitShader, true, texEnemy, 1f, 0f, 20f, false, false, 100, "Hộc Tử Thi Phải", DemoCombatantId.RightCorpseDrawer);

        // Đội hình Ally (Xanh / Textures) - Bên phải, spawn ngoài màn hình X = +15
        // Chuyển về đúng chuẩn Universal Render Pipeline / Unlit như phe Đỏ
        
        Texture2D texXIII = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Characters/XIII.png");
        Material matXIII = new Material(safeUnlitShader);
        matXIII.color = Color.white; 
        if (texXIII != null) {
            matXIII.SetTexture("_BaseMap", texXIII);
            matXIII.SetTexture("_MainTex", texXIII);
            SetMaterialTransparent(matXIII);
        }

        Texture2D texAn = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Characters/An.png");
        Material matAn = new Material(safeUnlitShader);
        matAn.color = Color.white;
        if (texAn != null) {
            matAn.SetTexture("_BaseMap", texAn);
            matAn.SetTexture("_MainTex", texAn);
            SetMaterialTransparent(matAn);
        }

        Texture2D texMac = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Characters/Mac.png");
        Material matMac = new Material(safeUnlitShader);
        matMac.color = Color.white;
        if (texMac != null) {
            matMac.SetTexture("_BaseMap", texMac);
            matMac.SetTexture("_MainTex", texMac);
            SetMaterialTransparent(matMac);
        }

        Vector3 allyOffset = new Vector3(15f, 0, 0);
        // Cấp thêm texture vào hàm Create2DCharacter để tính tỷ lệ khung hình
        Create2DCharacter(new Vector3(3, 10f, 0), allyOffset, matXIII, "Ally_XIII", safeUnlitShader, true, texXIII, 1f, 0f, 20f, false, true, 350, "XIII", DemoCombatantId.XIII);
        Create2DCharacter(new Vector3(6, 10f, 3f), allyOffset, matAn, "Ally_An", safeUnlitShader, true, texAn, 1f, 0f, 20f, false, true, 180, "An", DemoCombatantId.An);
        Create2DCharacter(new Vector3(6, 10f, -3f), allyOffset, matMac, "Ally_Mac", safeUnlitShader, true, texMac, 1f, 0f, 20f, false, true, 220, "Mac", DemoCombatantId.Mac);

        // Khởi tạo hệ thống Combat
        GameObject battleSystem = new GameObject("BattleSystem");
        battleSystem.AddComponent<BattleManager>();
        battleSystem.AddComponent<BattleUIManager>();

        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        light.intensity = 1.2f;

        // Gắn script ReplayButton (dùng IMGUI) vào một object bất kỳ trong Scene (ở đây gắn luôn vào Light cho gọn)
        lightObj.AddComponent<ReplayButton>();

        string folderPath = "Assets/_Project/Scenes";
        string scenePath = folderPath + "/BattlePlaceholder.unity";
        if (!AssetDatabase.IsValidFolder(folderPath)) AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log("Đã cập nhật Scene BattlePlaceholder tại: " + scenePath);
    }

    private static void SetMaterialTransparent(Material mat)
    {
        if (mat.shader.name.Contains("URP_SpriteGlow") || mat.shader.name.Contains("Universal Render Pipeline"))
        {
            mat.SetFloat("_Surface", 1); // 1 = Transparent
            mat.SetFloat("_Blend", 0); // 0 = Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            mat.shader = Shader.Find("Unlit/Transparent");
        }
    }

    private static void FixSpriteTexture(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    private static void FixTexture(string path, bool isNormalMap, bool isLinear)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool changed = false;
            if (isNormalMap && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                changed = true;
            }
            if (isLinear && importer.sRGBTexture)
            {
                importer.sRGBTexture = false;
                changed = true;
            }
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
    }

    private static void Create2DCharacter(Vector3 finalPos, Vector3 spawnOffset, Material mat, string name, Shader borderShader, bool hideBorder = false, Texture2D tex = null, float scaleMultiplier = 1f, float delay = 0f, float moveSpeed = 20f, bool shakeScreen = false, bool isAlly = false, int hp = 100, string charName = "", DemoCombatantId combatantId = DemoCombatantId.None)
    {
        // 1. TẠO ROOT (Chịu trách nhiệm Vật Lý)
        GameObject root = new GameObject(name);
        root.transform.position = finalPos + spawnOffset;
        
        Rigidbody rb = root.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        CapsuleCollider cap = root.AddComponent<CapsuleCollider>();
        cap.height = 2.5f * scaleMultiplier;
        cap.radius = 0.5f * scaleMultiplier;

        // Thêm hệ thống tương tác bằng chuột
        CharacterInteraction interaction = root.AddComponent<CharacterInteraction>();
        interaction.isAlly = isAlly;
        interaction.characterName = string.IsNullOrEmpty(charName) ? name : charName;
        interaction.ConfigureCombatantId(combatantId);
        
        // Gán chỉ số chuẩn Demo Funding
        
        // Gắn chỉ số chuẩn Demo Funding
        if (combatantId == DemoCombatantId.XIII)
        {
            interaction.maxHP = 2200; interaction.baseDEF = 15; interaction.baseATK = 40; interaction.baseBreakATK = 2;
            interaction.baseCritRate = 0.15f; interaction.baseCritDMG = 1.5f; interaction.element = "Physical";
            
            interaction.activeSkills.Add(new SkillData(SkillId.XIII_Attack, "Tà Thi Trảm", SkillCategory.ATTACK, 1.4f, 2, 1));
            interaction.activeSkills.Add(new SkillData(SkillId.XIII_HeavySlash, "Huyết Đoạn Kích", SkillCategory.ATTACK, 1.8f, 1, 1));
            
            SkillData bH = new SkillData(SkillId.None, "Liều Mạng Bộc Phá", SkillCategory.ATTACK, 2.5f, 4, 2);
            bH.selfDamage = 150;
            interaction.activeSkills.Add(bH);
        }
        else if (combatantId == DemoCombatantId.Mac)
        {
            interaction.maxHP = 1650; interaction.baseDEF = 10; interaction.baseATK = 28; interaction.baseBreakATK = 1;
            interaction.baseCritRate = 0.25f; interaction.baseCritDMG = 1.6f; interaction.element = "Mixed";

            SkillData m1 = new SkillData(SkillId.Mac_Attack, "Điểm Huyệt Ba-Toong", SkillCategory.ATTACK, 1.1f, 2, 1);
            m1.extraCritRate = 0.30f;
            interaction.activeSkills.Add(m1);
            
            SkillData m2 = new SkillData(SkillId.Mac_SuyNhuoc, "Bột Lân Tinh Bóc Giáp", SkillCategory.DEBUFF, 1.0f, 0, 1);
            m2.defShred = 0.50f;
            interaction.activeSkills.Add(m2);
            
            SkillData m3 = new SkillData(SkillId.None, "Ghi Chép Sát Cơ", SkillCategory.SUPPORT, 0f, 0, 1);
            m3.atkBuff = 0.25f;
            interaction.activeSkills.Add(m3);
        }
        else if (combatantId == DemoCombatantId.An)
        {
            interaction.maxHP = 1200; interaction.baseDEF = 6; interaction.baseATK = 22; interaction.baseBreakATK = 1;
            interaction.baseCritRate = 0.05f; interaction.baseCritDMG = 1.5f; interaction.element = "Mental";

            SkillData a1 = new SkillData(SkillId.An_HoThanPhu, "Hộ Thân Phù", SkillCategory.SUPPORT, 0f, 0, 1);
            a1.shieldAmount = 350;
            interaction.activeSkills.Add(a1);
            
            SkillData a2 = new SkillData(SkillId.An_DanHonThuat, "Dẫn Hồn Thuật", SkillCategory.SUPPORT, 0f, 0, 1);
            a2.healPercent = 0.3f;
            interaction.activeSkills.Add(a2);
            
            SkillData a3 = new SkillData(SkillId.An_Attack, "Trấn Trạch Lôi Bùa", SkillCategory.ATTACK, 1.3f, 2, 1);
            a3.isMental = true;
            interaction.activeSkills.Add(a3);
        }
        else if (combatantId == DemoCombatantId.BachMenhQuan)
        {
            interaction.maxHP = 10000; interaction.baseDEF = 12; interaction.baseATK = 35; interaction.baseBreakATK = 2;
            interaction.baseCritRate = 0.10f; interaction.baseCritDMG = 1.5f; interaction.element = "Physical";
            interaction.maxLimit = 10;
        }
        else if (combatantId == DemoCombatantId.LeftCorpseDrawer)
        {
            interaction.maxHP = BossEncounterRules.DrawerMaxHp; interaction.baseDEF = 8; interaction.baseATK = 20; interaction.baseBreakATK = 1;
            interaction.baseCritRate = 0f; interaction.baseCritDMG = 1.5f; interaction.element = "Physical";
            interaction.maxLimit = BossEncounterRules.DrawerMaxLimit;
        }
        else if (combatantId == DemoCombatantId.RightCorpseDrawer)
        {
            interaction.maxHP = BossEncounterRules.DrawerMaxHp; interaction.baseDEF = 8; interaction.baseATK = 25; interaction.baseBreakATK = 2;
            interaction.baseCritRate = 0f; interaction.baseCritDMG = 1.5f; interaction.element = "Mental";
            interaction.maxLimit = BossEncounterRules.DrawerMaxLimit;
        }
        else // Enemy thường (Ví dụ: Toán Cướp Lưu Vong)
        {
            interaction.maxHP = 450; interaction.baseDEF = 0; interaction.baseATK = 18; interaction.baseBreakATK = 1;
            interaction.baseCritRate = 0f; interaction.baseCritDMG = 1.5f; interaction.element = "Physical";
            interaction.maxLimit = 3;
        }
        
        interaction.currentHP = interaction.maxHP;
        interaction.currentLimit = interaction.maxLimit;

        // Thêm script diễn hoạt lao vào trận
        EntranceAnimation entrance = root.AddComponent<EntranceAnimation>();
        entrance.targetPosition = finalPos; // Đích đến là vị trí chiến đấu cuối cùng
        entrance.speed = moveSpeed; // Tốc độ lao vào cực gắt
        entrance.delayStart = delay; // Set độ trễ xuất hiện
        entrance.causeScreenShake = shakeScreen;

        // Tính tỷ lệ ảnh để không bị méo hình (squished)
        float aspect = 1f;
        if (tex != null)
        {
            aspect = (float)tex.width / tex.height;
        }

        // 2. TẠO VISUAL (Chịu trách nhiệm Hiển Thị 2D)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visual.name = "Visual";
        // Bóp chiều rộng theo tỷ lệ thật của ảnh, và nhân thêm hệ số Scale của Boss
        visual.transform.localScale = new Vector3(2.5f * aspect * scaleMultiplier, 2.5f * scaleMultiplier, 1f * scaleMultiplier); 
        visual.GetComponent<Renderer>().sharedMaterial = mat;
        Object.DestroyImmediate(visual.GetComponent<MeshCollider>());

        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.AddComponent<Billboard>();

        // 3. TẠO BORDER
        if (!hideBorder)
        {
            GameObject border = GameObject.CreatePrimitive(PrimitiveType.Quad);
            border.name = "Border";
            border.transform.SetParent(visual.transform);
            border.transform.localPosition = new Vector3(0, 0, 0.01f); 
            border.transform.localScale = new Vector3(1.1f, 1.08f, 1f); 
            border.transform.localRotation = Quaternion.identity;
            
            Material borderMat = new Material(borderShader);
            borderMat.color = Color.white;
            border.GetComponent<Renderer>().sharedMaterial = borderMat;
            Object.DestroyImmediate(border.GetComponent<MeshCollider>());
        }
    }
}
