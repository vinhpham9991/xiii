using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayButton : MonoBehaviour
{
    private Rect buttonRect;
    private bool isPressed = false;
    private bool isReloading = false;

    void Start()
    {
        // Căn góc dưới bên trái. IMGUI có gốc (0,0) ở góc trên bên trái.
        buttonRect = new Rect(20, 20, 120, 50);
    }

    void OnGUI()
    {
        // Cập nhật lại vị trí nếu đổi kích thước màn hình
        buttonRect = new Rect(20, 20, 120, 50);

        Rect displayRect = buttonRect;
        if (isPressed)
        {
            // Scale down 80%
            float width = buttonRect.width * 0.8f;
            float height = buttonRect.height * 0.8f;
            float x = buttonRect.x + (buttonRect.width - width) / 2f;
            float y = buttonRect.y + (buttonRect.height - height) / 2f;
            displayRect = new Rect(x, y, width, height);
        }

        GUIStyle style = new GUIStyle(GUI.skin.button);
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        Event e = Event.current;
        if (e.isMouse && e.button == 0)
        {
            if (e.type == EventType.MouseDown && buttonRect.Contains(e.mousePosition))
            {
                isPressed = true;
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                if (isPressed && buttonRect.Contains(e.mousePosition) && !isReloading)
                {
                    isReloading = true;
                    ReplayScene();
                }
                isPressed = false;
            }
        }

        // Vẽ nút
        GUI.Button(displayRect, "REPLAY", style);
    }

    void ReplayScene()
    {
        Debug.Log("Đang tải lại Scene bằng IMGUI...");
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(SceneManager.GetActiveScene().path, new LoadSceneParameters(LoadSceneMode.Single));
#else
        SceneManager.LoadScene(SceneManager.GetActiveScene().path);
#endif
    }
}
