using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlannedAction
{
    public CharacterInteraction actor;
    public CharacterInteraction target;
    public ActionType type;
    public SkillData skill;
    public ItemData item;
    
    // Thuộc tính phục vụ Resolution
    public bool isResolved = false;
    public bool isCancelled = false; // Bị hủy do mục tiêu chết trước khi ra đòn
}

[System.Serializable]
public class BeatPlan
{
    // Danh sách các hành động diễn ra CÙNG LÚC trong một Beat
    public List<PlannedAction> actions = new List<PlannedAction>();

    public void AddAction(PlannedAction action)
    {
        actions.Add(action);
    }
}
