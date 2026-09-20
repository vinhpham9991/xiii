import re

bm_path = r"Assets\_Project\Scripts\Combat\BattleManager.cs"
with open(bm_path, "r", encoding="utf-8") as f:
    bm_content = f.read()

# Pattern to find the if (state == BattleState.PLAYER_TURN && currentActor != null && !isExecuting) block
search_block = """        if (state == BattleState.PLAYER_TURN && currentActor != null && !isExecuting)
        {
            // Action Menu is open
            if (BattleUIManager.Instance.IsActionMenuOpen())
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    BattleUIManager.Instance.ChangeMenuSelection(-1);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    BattleUIManager.Instance.ChangeMenuSelection(1);
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    BattleUIManager.Instance.ConfirmMenuSelection();
                }
                else if (Input.GetKeyDown(KeyCode.Backspace) || (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame))
                {
                    if (!BattleUIManager.Instance.TryGoBack())
                    {
                        CancelActorSelection();
                    }
                }
            }"""
            
replace_block = """        if (state == BattleState.PLAYER_TURN && currentActor != null && !isExecuting)
        {
            if (Input.GetKeyDown(KeyCode.Backspace) || (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame))
            {
                if (!BattleUIManager.Instance.TryGoBack())
                {
                    CancelActorSelection();
                }
            }
            // Action Menu is open
            else if (BattleUIManager.Instance.IsActionMenuOpen())
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                {
                    BattleUIManager.Instance.ChangeMenuSelection(-1);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                {
                    BattleUIManager.Instance.ChangeMenuSelection(1);
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    BattleUIManager.Instance.ConfirmMenuSelection();
                }
            }"""

if search_block in bm_content:
    bm_content = bm_content.replace(search_block, replace_block)
else:
    print("Search block not found. Trying regex...")
    pattern = re.compile(r"        if \(state == BattleState\.PLAYER_TURN && currentActor != null && !isExecuting\)\s*\{\s*// Action Menu is open\s*if \(BattleUIManager\.Instance\.IsActionMenuOpen\(\)\)\s*\{\s*if \(Input\.GetKeyDown\(KeyCode\.UpArrow\).*?\}\s*\}\s*\}", re.DOTALL)
    match = pattern.search(bm_content)
    if match:
        print("Found with regex, but didn't replace because complex.")

with open(bm_path, "w", encoding="utf-8") as f:
    f.write(bm_content)

print("done")
