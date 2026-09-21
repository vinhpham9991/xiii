using UnityEngine;

public enum ItemType
{
    HEAL,
    BUFF_STATS,
    RESTORE_SOUL,
    CURE_POISON
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Combat/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public ItemType itemType;
    
    [Header("Heal Effect")]
    public int healAmount;
    
    [Header("Buff Effect")]
    public float atkBuff; // Ví dụ: 0.2 cho 20%

    [Header("Soul Effect")]
    public int soulRestoreAmount; // +1 Soul
}
