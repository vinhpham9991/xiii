using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;
    private float shakeDuration = 0f;
    private float shakeAmount = 0.2f;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            // Lắc màn hình ngẫu nhiên trong bán kính shakeAmount
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.deltaTime;
            
            if (shakeDuration <= 0f)
            {
                shakeDuration = 0f;
                // Trả về vị trí gốc
                transform.localPosition = originalPos;
            }
        }
    }

    public void TriggerShake(float duration, float amount)
    {
        // Lưu lại vị trí gốc đề phòng camera bị lệch trước khi rung
        if (shakeDuration <= 0) originalPos = transform.localPosition;
        shakeDuration = duration;
        shakeAmount = amount;
    }
}
