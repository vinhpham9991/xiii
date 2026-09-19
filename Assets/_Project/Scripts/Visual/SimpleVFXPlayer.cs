using UnityEngine;

public class SimpleVFXPlayer : MonoBehaviour
{
    public Sprite[] frames;
    public float frameRate = 24f;
    private SpriteRenderer sr;
    private int currentFrame = 0;
    private float timer = 0f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                Destroy(gameObject); // Chạy xong tự hủy
            }
            else
            {
                sr.sprite = frames[currentFrame];
            }
        }
    }
}
