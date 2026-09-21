using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DebugConfigUI : MonoBehaviour
{
    private bool showConfig = false;
    private int tabIndex = 0;
    private string[] tabs = { "Character", "Enemy" };

    private Dictionary<CharacterInteraction, CharacterConfigData> configDataMap = new Dictionary<CharacterInteraction, CharacterConfigData>();

    public class CharacterConfigData
    {
        public string HP;
        public string ATK;
        public string DEF;
        public string Limit;
        public string Break;
        
        public int defaultHP;
        public int defaultATK;
        public int defaultDEF;
        public int defaultLimit;
        public int defaultBreak;
        
        public CharacterConfigData(CharacterInteraction c)
        {
            defaultHP = c.maxHP;
            defaultATK = c.baseATK;
            defaultDEF = c.baseDEF;
            defaultLimit = c.maxLimit;
            defaultBreak = c.baseBreakATK;
            
            ResetToDefault();
        }
        
        public void ResetToDefault()
        {
            HP = defaultHP.ToString();
            ATK = defaultATK.ToString();
            DEF = defaultDEF.ToString();
            Limit = defaultLimit.ToString();
            Break = defaultBreak.ToString();
        }
        
        public void ApplyTo(CharacterInteraction c)
        {
            if (int.TryParse(HP, out int hp)) { c.maxHP = hp; c.currentHP = Mathf.Min(c.currentHP, c.maxHP); }
            if (int.TryParse(ATK, out int atk)) c.baseATK = atk;
            if (int.TryParse(DEF, out int def)) c.baseDEF = def;
            if (int.TryParse(Limit, out int limit)) { c.maxLimit = limit; c.currentLimit = Mathf.Min(c.currentLimit, c.maxLimit); }
            if (int.TryParse(Break, out int brk)) c.baseBreakATK = brk;
        }
    }

    private Texture2D darkBackground;

    void Start()
    {
        darkBackground = new Texture2D(1, 1);
        darkBackground.SetPixel(0, 0, new Color(0, 0, 0, 0.9f));
        darkBackground.Apply();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            showConfig = !showConfig;
            if (showConfig)
            {
                Time.timeScale = 0; // Pause
                RefreshConfigData();
            }
            else
            {
                Time.timeScale = 1; // Resume
            }
        }
    }

    void RefreshConfigData()
    {
        var battleManager = FindObjectOfType<BattleManager>();
        if (battleManager == null) return;
        
        configDataMap.Clear();
        foreach (var c in battleManager.allies)
        {
            configDataMap[c] = new CharacterConfigData(c);
        }
        foreach (var c in battleManager.enemies)
        {
            configDataMap[c] = new CharacterConfigData(c);
        }
    }

    void OnGUI()
    {
        if (!showConfig) return;

        // Draw dark full-screen background
        if (darkBackground != null)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), darkBackground);
        }

        // Apply global scale (zoom in by 1.5x)
        float scaleFactor = 1.5f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleFactor, scaleFactor, 1));

        float scaledWidth = Screen.width / scaleFactor;
        float scaledHeight = Screen.height / scaleFactor;

        float boxWidth = 800;
        float boxHeight = 350;
        float startX = (scaledWidth - boxWidth) / 2;
        float startY = (scaledHeight - boxHeight) / 2;

        GUI.BeginGroup(new Rect(startX, startY, boxWidth, boxHeight));
        GUI.Box(new Rect(0, 0, boxWidth, boxHeight), "Debug Config UI");

        if (GUI.Button(new Rect(boxWidth - 40, 10, 30, 20), "X"))
        {
            showConfig = false;
            Time.timeScale = 1;
        }

        tabIndex = GUI.Toolbar(new Rect(20, 30, 200, 30), tabIndex, tabs);

        var battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null)
        {
            List<CharacterInteraction> targetList = tabIndex == 0 ? battleManager.allies : battleManager.enemies;

            float yOffset = 80;
            foreach (var c in targetList)
            {
                if (!configDataMap.ContainsKey(c)) continue;
                var data = configDataMap[c];

                GUI.Label(new Rect(20, yOffset, 150, 20), c.characterName);
                
                GUI.Label(new Rect(180, yOffset, 30, 20), "HP:");
                data.HP = GUI.TextField(new Rect(210, yOffset, 60, 20), data.HP);

                GUI.Label(new Rect(280, yOffset, 35, 20), "ATK:");
                data.ATK = GUI.TextField(new Rect(315, yOffset, 60, 20), data.ATK);

                GUI.Label(new Rect(385, yOffset, 35, 20), "DEF:");
                data.DEF = GUI.TextField(new Rect(420, yOffset, 60, 20), data.DEF);
                
                GUI.Label(new Rect(490, yOffset, 40, 20), "Limit:");
                data.Limit = GUI.TextField(new Rect(530, yOffset, 60, 20), data.Limit);
                
                GUI.Label(new Rect(600, yOffset, 45, 20), "Break:");
                data.Break = GUI.TextField(new Rect(645, yOffset, 60, 20), data.Break);

                yOffset += 40;
            }
        }

        if (GUI.Button(new Rect(20, boxHeight - 50, 120, 30), "Apply & Restart"))
        {
            foreach (var kvp in configDataMap)
            {
                kvp.Value.ApplyTo(kvp.Key);
            }
            ApplyStaticOverrides();
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (GUI.Button(new Rect(150, boxHeight - 50, 100, 30), "Default"))
        {
            foreach (var kvp in configDataMap)
            {
                kvp.Value.ResetToDefault();
            }
            ClearStaticOverrides();
        }

        GUI.EndGroup();
    }

    public static Dictionary<string, CharacterConfigData> Overrides = new Dictionary<string, CharacterConfigData>();

    private void ApplyStaticOverrides()
    {
        Overrides.Clear();
        foreach (var kvp in configDataMap)
        {
            Overrides[kvp.Key.characterName] = kvp.Value;
        }
    }

    private void ClearStaticOverrides()
    {
        Overrides.Clear();
    }
}
