using UnityEngine;

public enum ItemId
{
    None = 0,
    HealthPotion,
    PowerElixir,
    SoulPlusOne,
    Antidote
}

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
    public ItemId id;
    public string itemName;
    public string description;
    public ItemType itemType;
    
    [Header("Heal Effect")]
    [Tooltip("Phần trăm máu tối đa được hồi (ví dụ: 0.3 = 30%)")]
    public float healPercent;
    
    [Header("Buff Effect")]
    public float atkBuff; // Ví dụ: 0.2 cho 20%

    [Header("Soul Effect")]
    public int soulRestoreAmount; // +1 Soul
}
