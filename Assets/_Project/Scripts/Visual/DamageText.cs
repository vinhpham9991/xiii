using UnityEngine;

public class DamageText : MonoBehaviour
{
    private TextMesh textMesh;
    private Color textColor;
    
    private float moveSpeed = 2f;
    private float disappearTimer;
    private float disappearTimerMax = 1f;

    public void Setup(int damageAmount, bool isCritical, bool isHeal, Color color)
    {
        textMesh = gameObject.AddComponent<TextMesh>();
        
        // Sử dụng font có sẵn trong máy để không lỗi font
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            textMesh.font = font;
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.sharedMaterial = font.material;
        }

        textMesh.text = damageAmount.ToString();
        if (isCritical) textMesh.text += "!";
        
        textMesh.characterSize = 0.05f; // Thu nhỏ 50% nữa
        textMesh.fontSize = 60;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        
        textColor = color;
        textMesh.color = textColor;
        
        disappearTimerMax = 1.0f;
        disappearTimer = disappearTimerMax;
        
        // Hiệu ứng tốc độ trôi lên (giảm còn 1/3 để bay vừa phải)
        moveSpeed = 0.8f;
    }

    private void Update()
    {
        // Bay từ từ lên trên
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if (disappearTimer < 0)
        {
            // Mờ dần rồi biến mất
            float fadeSpeed = 3f;
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            if (textColor.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
