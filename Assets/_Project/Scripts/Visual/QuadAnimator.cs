using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(Billboard))]
public class QuadAnimator : MonoBehaviour
{
    public Sprite stanceSprite;     // Fallback 1 frame tĩnh (nếu không có idleSprites)
    public Sprite[] idleSprites;    // Animation Idle lặp vòng (ưu tiên hơn stanceSprite)
    public Sprite[] dashSprites;
    public Sprite[] attackSprites;
    public Sprite[] downedSprites;
    public Sprite[] hitSprites;
    public Sprite[] appearSprites;
    public Sprite[] jumpSprites;
    public Vector3 customOffset = Vector3.zero;

    [SerializeField, Min(1f)] private float idleFramesPerSecond = 8f;
    [SerializeField, Min(1f)] private float actionFramesPerSecond = 12f;

    private Renderer quadRenderer;
    private Coroutine currentAnim;
    private bool isDead = false;

    public float paddingScale = 1.5f;
    [Header("Sửa lỗi ảnh bị bóp méo (x > 1 sẽ làm hình bè ra)")]
    public float aspectFix = 1.0f;

    void Awake()
    {
        quadRenderer = GetComponent<Renderer>();
        // UV sẽ được set động trong SetFrame() dựa theo từng sprite

        if (customOffset == Vector3.zero && transform.parent != null)
        {
            if (transform.parent.name.Contains("XIII"))
            {
                customOffset = new Vector3(0, -1.237113f, 0);
            }
        }
    }

    void Start()
    {
        // Ưu tiên chạy idle animation nếu có, nếu không thì fallback về stance tĩnh
        if (idleSprites != null && idleSprites.Length > 0)
            PlayAnim("idle", idleFramesPerSecond);
        else
            PlayAnim("stance");
    }

    public void PlayAnim(string name, float fps = -1f)
    {
        if (quadRenderer == null || quadRenderer.material == null) return;

        if (fps <= 0f)
        {
            fps = name == "idle" || name == "stance"
                ? idleFramesPerSecond
                : actionFramesPerSecond;
        }
        
        if (currentAnim != null) 
        {
            StopCoroutine(currentAnim);
        }
        
        if (name == "downed")
        {
            isDead = true;
        }
        else if (isDead)
        {
            return;
        }

        Sprite[] frames = null;
        bool loop = false;

        switch (name)
        {
            case "idle":
                frames = idleSprites;
                loop = true;
                break;
            case "attack":
                frames = attackSprites;
                break;
            case "hit":
                frames = hitSprites;
                break;
            case "dash":
                frames = dashSprites;
                loop = true;
                break;
            case "downed":
                frames = downedSprites;
                break;
            case "appear":
                frames = appearSprites;
                break;
            case "jump":
                frames = jumpSprites;
                break;
            case "stance":
                if (idleSprites != null && idleSprites.Length > 0)
                {
                    frames = idleSprites;
                    loop = true;
                    break;
                }
                if (stanceSprite != null)
                {
                    SetFrame(stanceSprite);
                }
                return;
        }

        if (frames == null || frames.Length == 0)
        {
            if (name == "jump" && dashSprites != null && dashSprites.Length > 0)
            {
                frames = dashSprites;
            }
            else if (idleSprites != null && idleSprites.Length > 0)
            {
                frames = idleSprites;
                loop = true; // Fallback to idle should loop
            }
            else if (stanceSprite != null)
            {
                SetFrame(stanceSprite);
                return;
            }
            else return;
        }

        if (frames != null && frames.Length > 0)
        {
            currentAnim = StartCoroutine(PlayRoutine(frames, fps, loop));
        }
    }

    public void ConfigureIdle(Sprite[] frames, float framesPerSecond)
    {
        idleSprites = frames ?? System.Array.Empty<Sprite>();
        stanceSprite = idleSprites.Length > 0 ? idleSprites[0] : null;
        idleFramesPerSecond = Mathf.Max(1f, framesPerSecond);
    }

    public void ClearActionAnimations()
    {
        dashSprites = System.Array.Empty<Sprite>();
        attackSprites = System.Array.Empty<Sprite>();
        downedSprites = System.Array.Empty<Sprite>();
        hitSprites = System.Array.Empty<Sprite>();
        appearSprites = System.Array.Empty<Sprite>();
        jumpSprites = System.Array.Empty<Sprite>();
    }

    private void SetFrame(Sprite sprite)
    {
        if (quadRenderer == null || quadRenderer.material == null || sprite == null) return;

        Texture2D tex = sprite.texture;
        quadRenderer.material.mainTexture = tex;

        // --- UV: Lấy chuẩn xác từ DataUtility ---
        // Giúp loại bỏ lỗi tính sai khi texture bị Unity resize (vd Max Size 2048)
        Vector4 outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
        float uvOffsetX = outerUV.x;
        float uvOffsetY = outerUV.y;
        float uvScaleX  = outerUV.z - outerUV.x;
        float uvScaleY  = outerUV.w - outerUV.y;

        // Áp thêm paddingScale để quad lớn hơn sprite thực tế (tạo viền an toàn)
        // Nên UV window phải RỘNG HƠN sprite (nhân với paddingScale)
        float paddedScaleX = uvScaleX  * paddingScale;
        float paddedScaleY = uvScaleY  * paddingScale;
        float paddedOffX   = uvOffsetX - (paddedScaleX - uvScaleX) / 2f;
        float paddedOffY   = uvOffsetY - (paddedScaleY - uvScaleY) / 2f;

        quadRenderer.material.mainTextureScale  = new Vector2(paddedScaleX, paddedScaleY);
        quadRenderer.material.mainTextureOffset = new Vector2(paddedOffX,   paddedOffY);
        
        // Hỗ trợ URP shader (thường dùng _BaseMap thay vì _MainTex)
        if (quadRenderer.material.HasProperty("_BaseMap"))
        {
            quadRenderer.material.SetTexture("_BaseMap", tex);
            quadRenderer.material.SetTextureScale("_BaseMap", new Vector2(paddedScaleX, paddedScaleY));
            quadRenderer.material.SetTextureOffset("_BaseMap", new Vector2(paddedOffX, paddedOffY));
        }

        // Hỗ trợ shader clip viền (để tránh bị lem frame bên cạnh khi dùng paddingScale)
        quadRenderer.material.SetVector("_SpriteUVRect", new Vector4(uvOffsetX, uvOffsetY, uvOffsetX + uvScaleX, uvOffsetY + uvScaleY));

        // --- Scale quad theo kích thước sprite ---
        float ppu    = sprite.pixelsPerUnit > 0 ? sprite.pixelsPerUnit : 100f;
        float width  = sprite.rect.width  / ppu;
        float height = sprite.rect.height / ppu;

        transform.localScale = new Vector3(width * paddingScale * aspectFix, height * paddingScale, 1f);

        // --- Điều chỉnh vị trí dựa trên Pivot ---
        float normPivotX = sprite.pivot.x / sprite.rect.width;
        float normPivotY = sprite.pivot.y / sprite.rect.height;

        // Lưu ý: KHÔNG nhân paddingScale ở đây, vì padding được thêm đều (đối xứng) vào 2 bên của tâm.
        // Khoảng cách từ tâm đến pivot không bị thay đổi bởi padding.
        float offsetX = (0.5f - normPivotX) * width;
        float offsetY = (0.5f - normPivotY) * height;

        transform.localPosition = new Vector3(offsetX, offsetY, 0f) + customOffset;
    }

    private IEnumerator PlayRoutine(Sprite[] frames, float fps, bool loop)
    {
        float delay = 1f / fps;
        int i = 0;
        while (true)
        {
            if (frames[i] != null)
            {
                SetFrame(frames[i]);
            }
            yield return new WaitForSeconds(delay);
            
            i++;
            if (i >= frames.Length)
            {
                if (loop) 
                {
                    i = 0;
                }
                else 
                {
                    break; // Keep the last frame
                }
            }
        }
    }
}
