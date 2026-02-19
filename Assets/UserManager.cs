using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;

public class UserManager : MonoBehaviour
{
    [Tooltip("Reference to the HUDManager to update health bar and other UI elements.")]
    [SerializeField] private HUDManager hudManager;
    private float currentHealth = 100f;
    private float maxHealth = 100f;

    [Tooltip("Seconds allowed between key presses before progress resets")]
    public float inputTimeout = 1f;

    [Tooltip("Assign your InputActionAsset (UserActions.inputactions) here. The script will use the 'combat' map and its Up/Down/Left/Right actions.")]
    public InputActionAsset inputActions;

    [Tooltip("Assign the SpellManager instance that manages all spells and their sequences.")]
    public SpellManager spellManager;

    // InputAction references (populated when asset is assigned)
    InputActionMap combatMap;
    System.Collections.Generic.List<InputAction> combatActions = new System.Collections.Generic.List<InputAction>();

    [Header("UI")]
    [Tooltip("Optional TextMeshPro UI element to display configured sequences and progress.")]
    public TMP_Text sequenceText;

    // runtime state for each configured sequence
    class SequenceState { public int index; public float lastInputTime; }
    SequenceState[] sequenceStates = new SequenceState[0];

    void Awake()
    {
        if (spellManager == null)
        {
            spellManager = FindFirstObjectByType<SpellManager>();
            if (spellManager == null)
                Debug.LogError("UserManager: no SpellManager found. Please assign one in the inspector or ensure it exists in the scene.");
        }

        // initialize sequence states
        EnsureSequenceStates();
    }

    void EnsureSequenceStates()
    {
        if (spellManager == null || spellManager.spells == null)
        {
            sequenceStates = new SequenceState[0];
            return;
        }

        if (sequenceStates == null || sequenceStates.Length != spellManager.spells.Length)
        {
            sequenceStates = new SequenceState[spellManager.spells.Length];
            for (int i = 0; i < sequenceStates.Length; ++i)
                sequenceStates[i] = new SequenceState() { index = 0, lastInputTime = 0f };
            UpdateSequenceDisplay();
        }
    }

    void OnEnable()
    {
        EnsureSequenceStates();
        if (inputActions != null)
        {
            combatMap = inputActions.FindActionMap("combat", true);
            combatActions.Clear();
            foreach (var a in combatMap.actions)
            {
                combatActions.Add(a);
                a.performed += OnActionPerformed;
            }

            combatMap.Enable();
            UpdateSequenceDisplay();
        } 
        else 
        {
            Debug.LogError("UserManager: No InputActionAsset assigned. Please assign one in the inspector.");
        }
    }

    void OnDisable()
    {
        if (combatMap != null)
        {
            foreach (var a in combatActions)
                a.performed -= OnActionPerformed;

            combatMap.Disable();
            combatActions.Clear();
            combatMap = null;
        } 
        else
        {
            Debug.LogError("UserManager: No InputActionAsset assigned. Please assign one in the inspector.");
        }
    }

    void Update()
    {
        // update timeouts for configured sequence mappings
        if (sequenceStates != null && sequenceStates.Length > 0)
        {
            for (int i = 0; i < sequenceStates.Length; ++i)
            {
                var st = sequenceStates[i];
                if (st.index > 0 && Time.time - st.lastInputTime > inputTimeout)
                    st.index = 0;
            }
        }

        // If no InputActionAsset assigned, display error
        if (inputActions == null)
        {
            Debug.LogError("UserManager: No InputActionAsset assigned. Please assign one in the inspector.");
        }
    }

    public void UpdateHealth(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;
        if (hudManager != null)
        {
            hudManager.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    void HandlePressed(KeyCode pressed)
    {
        Debug.Log("HandlePressed: " + pressed);

        if (spellManager == null || spellManager.spells == null || spellManager.spells.Length == 0)
            return;

        EnsureSequenceStates();
        for (int i = 0; i < spellManager.spells.Length; ++i)
        {
            var spell = spellManager.spells[i];
            if (spell == null || spell.sequence == null || spell.sequence.Length == 0)
                continue;

            var st = sequenceStates[i];

            // timeout check
            if (st.index > 0 && Time.time - st.lastInputTime > inputTimeout)
                st.index = 0;

            if (pressed == spell.sequence[st.index])
            {
                st.index++;
                st.lastInputTime = Time.time;

                if (st.index >= spell.sequence.Length)
                {
                    Debug.LogFormat("Sequence for spell '{0}' complete", spell.spellName);
                    spellManager.TryExecuteSpell(spell.spellName, this.gameObject);
                    st.index = 0;
                }
            }
            else
            {
                if (pressed == spell.sequence[0])
                    st.index = 1;
                else
                    st.index = 0;

                st.lastInputTime = Time.time;
            }
        }

        UpdateSequenceDisplay();
    }

    void OnActionPerformed(InputAction.CallbackContext ctx)
    {
        Debug.LogFormat("Action performed: {0} control={1}", ctx.action?.name, ctx.control?.path);

        // Prefer mapping from action name (works for keyboard + gamepad bindings)
        var actionName = ctx.action?.name;
        if (!string.IsNullOrEmpty(actionName))
        {
            switch (actionName)
            {
                case "Up":
                    Debug.Log("Mapped action 'Up' -> UpArrow");
                    HandlePressed(KeyCode.UpArrow);
                    return;
                case "Down":
                    Debug.Log("Mapped action 'Down' -> DownArrow");
                    HandlePressed(KeyCode.DownArrow);
                    return;
                case "Left":
                    Debug.Log("Mapped action 'Left' -> LeftArrow");
                    HandlePressed(KeyCode.LeftArrow);
                    return;
                case "Right":
                    Debug.Log("Mapped action 'Right' -> RightArrow");
                    HandlePressed(KeyCode.RightArrow);
                    return;
            }
        }
    }


    void UpdateSequenceDisplay()
    {
        if (sequenceText == null || spellManager == null)
            return;

        if (spellManager.spells == null || spellManager.spells.Length == 0)
        {
            sequenceText.text = "(no spells configured in SpellManager)";
            return;
        }

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < spellManager.spells.Length; ++i)
        {
            var spell = spellManager.spells[i];
            if (spell == null || spell.sequence == null || spell.sequence.Length == 0)
                continue;

            // display spell name
            if (!string.IsNullOrEmpty(spell.spellName))
            {
                sb.Append(spell.spellName);
                sb.Append(": ");
            }

            // show sequence
            for (int j = 0; j < spell.sequence.Length; ++j)
            {
                bool isNext = (sequenceStates != null && i < sequenceStates.Length && sequenceStates[i] != null && sequenceStates[i].index == j);
                if (isNext)
                    sb.Append("<color=#FFFF00><b>");

                sb.Append(spell.sequence[j].ToString());

                if (isNext)
                    sb.Append("</b></color>");

                if (j < spell.sequence.Length - 1)
                    sb.Append(" → ");
            }

            // cooldown indicator
            if (spell.cooldownSeconds > 0f)
            {
                float timeLeft = spell.cooldownSeconds - (Time.time - spell.lastExecutedTime);
                if (timeLeft > 0f)
                    sb.Append("  <color=#FF0000>[" + timeLeft.ToString("F1") + "s]</color>");
            }

            if (i < spellManager.spells.Length - 1)
                sb.AppendLine();
        }

        sequenceText.text = sb.ToString();
    }
}
