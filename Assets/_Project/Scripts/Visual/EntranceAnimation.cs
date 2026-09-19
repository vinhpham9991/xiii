using UnityEngine;

public class EntranceAnimation : MonoBehaviour
{
    public Vector3 targetPosition;
    public float speed = 15f;
    private Rigidbody rb;

    private Vector3 currentVelocity = Vector3.zero;
    public float smoothTime = 0.1f; // Giảm thời gian trượt để nhân vật phanh gấp hơn
    public float delayStart = 0f; // Thời gian chờ trước khi xuất hiện
    public bool causeScreenShake = false; // Có gây rung màn hình khi đáp đất không?

    private bool hasStartedFalling = false;
    private bool hasReachedTargetXZ = false;
    private bool hasShaken = false;
    private bool hasPlayedSFX = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        if (delayStart > 0)
        {
            rb.isKinematic = true;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            if (delayStart > 0)
            {
                delayStart -= Time.fixedDeltaTime;
                if (delayStart <= 0)
                {
                    rb.isKinematic = false;
                }
                else
                {
                    return;
                }
            }

            if (causeScreenShake && !hasPlayedSFX)
            {
                hasPlayedSFX = true;
                if (BattleManager.Instance != null)
                    BattleManager.Instance.PlaySFX("sfx_boss_appear", 1f);
            }

            rb.AddForce(Vector3.down * 40f, ForceMode.Acceleration);

            if (rb.linearVelocity.y < -5f)
            {
                hasStartedFalling = true;
            }

            if (!hasReachedTargetXZ)
            {
                Vector3 currentPos = rb.position;
                Vector3 targetXZ = new Vector3(targetPosition.x, currentPos.y, targetPosition.z);
                
                Vector3 newPos = Vector3.SmoothDamp(currentPos, targetXZ, ref currentVelocity, smoothTime, speed, Time.fixedDeltaTime);
                Vector3 neededVelocity = (newPos - currentPos) / Time.fixedDeltaTime;
                
                rb.linearVelocity = new Vector3(neededVelocity.x, rb.linearVelocity.y, neededVelocity.z);

                float distance = Vector2.Distance(new Vector2(currentPos.x, currentPos.z), new Vector2(targetPosition.x, targetPosition.z));
                if (distance < 0.5f)
                {
                    hasReachedTargetXZ = true;
                    rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                    rb.position = new Vector3(targetPosition.x, rb.position.y, targetPosition.z);
                }
            }

            if (hasReachedTargetXZ && hasStartedFalling && Mathf.Abs(rb.linearVelocity.y) < 0.5f && !hasShaken)
            {
                if (causeScreenShake)
                {
                    if (CameraShake.Instance != null)
                        CameraShake.Instance.TriggerShake(0.5f, 0.8f);
                }
                hasShaken = true;
                rb.isKinematic = true; // Khóa vật lý sau khi đáp đất để khi chém nhau không bị văng
                
                // Cập nhật lại tọa độ gốc cho CharacterInteraction để lúc bị đánh không bị dịch chuyển về vị trí sinh ra (tít ngoài viền)
                CharacterInteraction ci = GetComponent<CharacterInteraction>();
                if (ci != null)
                {
                    ci.SetOriginalPosition(transform.position);
                }
                
                this.enabled = false;
            }
        }
    }
}
