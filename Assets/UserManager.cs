using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using TMPro;

public class UserManager : MonoBehaviour
{
    [Tooltip("Reference to the CharacterAnimator for animation control.")]
    [SerializeField] private CharacterAnimator characterAnimator;
    
    [Tooltip("Reference to the WeaponManager for weapon management.")]
    [SerializeField] private WeaponManager weaponManager;

    // Weapon state
    private bool weaponInvoked = false;
    private bool combatStarted = false;
    private Weapon currentWeapon = null;
    private PlayerEntity playerEntity;

    [Tooltip("Seconds allowed between key presses before progress resets")]
    public float inputTimeout = 1f;

    [Tooltip("Assign your InputActionAsset (UserActions.inputactions) here. The script will use the 'combat' map and its Up/Down/Left/Right actions.")]
    public InputActionAsset inputActions;

    // Helper function to convert KeyCode to arrow symbol
    private string KeyCodeToArrowSymbol(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow:
                return "↑";
            case KeyCode.DownArrow:
                return "↓";
            case KeyCode.LeftArrow:
                return "←";
            case KeyCode.RightArrow:
                return "→";
            default:
                return key.ToString();
        }
    }

    // InputAction references (populated when asset is assigned)
    InputActionMap combatMap;
    System.Collections.Generic.List<InputAction> combatActions = new System.Collections.Generic.List<InputAction>();

    [Header("UI")]
    [Tooltip("Optional TextMeshPro UI element to display configured sequences and progress.")]
    public TMP_Text sequenceText;

    // Invocation pattern state
    private int invocationIndex = 0;
    private float invocationLastInputTime = 0f;
    private int currentInvocationWeaponIndex = -1; // Track which weapon pattern is being followed (-1 = none)

    // Attack patterns state (one per weapon attack)
    class AttackPatternState { public int index; public float lastInputTime; public float lastExecutedTime = -1000f; }
    AttackPatternState[] attackPatternStates = new AttackPatternState[0];

    void Awake()
    {
        if (characterAnimator == null)
        {
            characterAnimator = GetComponent<CharacterAnimator>();
            if (characterAnimator == null)
                Debug.LogWarning("UserManager: no CharacterAnimator found. Animations will not play.");
        }

        playerEntity = GetComponent<PlayerEntity>();
        if (playerEntity == null)
            Debug.LogWarning("UserManager: no PlayerEntity found on this GameObject.");

        if (weaponManager == null)
        {
            weaponManager = FindFirstObjectByType<WeaponManager>();
            if (weaponManager == null)
                Debug.LogWarning("UserManager: no WeaponManager found. Weapon system disabled.");
        }

        // Initialize attack pattern states
        EnsureAttackPatternStates();
    }

    void EnsureAttackPatternStates()
    {
        if (currentWeapon == null || currentWeapon.actions == null)
        {
            attackPatternStates = new AttackPatternState[0];
            return;
        }

        if (attackPatternStates == null || attackPatternStates.Length != currentWeapon.actions.Length)
        {
            attackPatternStates = new AttackPatternState[currentWeapon.actions.Length];
            for (int i = 0; i < attackPatternStates.Length; ++i)
                attackPatternStates[i] = new AttackPatternState() { index = 0, lastInputTime = 0f };
            UpdateSequenceDisplay();
        }
    }

    void OnEnable()
    {
        EnsureAttackPatternStates();
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
            
            // Play stand still at combat start (only once)
            if (!combatStarted && !weaponInvoked && characterAnimator != null)
            {
                characterAnimator.PlayStandStillNoWeapon();
                combatStarted = true;
                Debug.Log("Combat started: playing stand still without weapon");
            }
            
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

        // Reset combat state when disabling (for potential restart)
        combatStarted = false;
        invocationIndex = 0;
        currentInvocationWeaponIndex = -1;
    }

    void Update()
    {
        // Update timeout for invocation pattern
        if (!weaponInvoked && invocationIndex > 0 && Time.time - invocationLastInputTime > inputTimeout)
        {
            if (characterAnimator != null)
                characterAnimator.StopInvocationLoop();
            invocationIndex = 0;
            currentInvocationWeaponIndex = -1;
        }

        // Update timeouts for attack patterns
        if (weaponInvoked && attackPatternStates != null && attackPatternStates.Length > 0)
        {
            for (int i = 0; i < attackPatternStates.Length; ++i)
            {
                var st = attackPatternStates[i];
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

    void HandlePressed(KeyCode pressed)
    {
        Debug.Log("HandlePressed: " + pressed);

        if (!weaponInvoked)
        {
            // PHASE 1: INVOCATION PATTERN
            HandleInvocationPattern(pressed);
        }
        else
        {
            // PHASE 2: ATTACK PATTERNS
            HandleAttackPatterns(pressed);
        }

        UpdateSequenceDisplay();
    }

    void HandleInvocationPattern(KeyCode pressed)
    {
        if (weaponManager == null)
            return;

        Weapon[] weapons = weaponManager.GetAvailableWeapons();
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogWarning("HandleInvocationPattern: No weapons available");
            return;
        }

        // If we're already following a specific weapon pattern, stay with it
        if (currentInvocationWeaponIndex >= 0 && currentInvocationWeaponIndex < weapons.Length)
        {
            Weapon weapon = weapons[currentInvocationWeaponIndex];
            if (weapon != null && weapon.invocationPattern != null && weapon.invocationPattern.Length > invocationIndex)
            {
                if (pressed == weapon.invocationPattern[invocationIndex])
                {
                    // Correct next key for current weapon
                    invocationIndex++;
                    invocationLastInputTime = Time.time;

                    // Animation feedback
                    if (invocationIndex == 1 && characterAnimator != null)
                    {
                        characterAnimator.PlayStartInvocation();
                    }
                    else if (invocationIndex > 1 && characterAnimator != null)
                    {
                        characterAnimator.PlayInvocationLoop();
                    }

                    // Check if pattern complete
                    if (invocationIndex >= weapon.invocationPattern.Length)
                    {
                        Debug.LogFormat("Invocation pattern for '{0}' complete!", weapon.weaponName);
                        
                        if (characterAnimator != null)
                        {
                            characterAnimator.PlayInvocationEnd();
                        }

                        EquipWeapon(weapon);
                        weaponInvoked = true;
                        invocationIndex = 0;
                        currentInvocationWeaponIndex = -1;
                    }
                    return;
                }
                else
                {
                    // Wrong key for current weapon, check if it starts a new pattern
                    for (int w = 0; w < weapons.Length; ++w)
                    {
                        Weapon otherWeapon = weapons[w];
                        if (otherWeapon == null || otherWeapon.invocationPattern == null || otherWeapon.invocationPattern.Length == 0)
                            continue;
                        
                        if (pressed == otherWeapon.invocationPattern[0])
                        {
                            // Switch to new weapon pattern
                            if (characterAnimator != null)
                                characterAnimator.StopInvocationLoop();
                            invocationIndex = 1;
                            invocationLastInputTime = Time.time;
                            currentInvocationWeaponIndex = w;
                            
                            if (characterAnimator != null)
                                characterAnimator.PlayStartInvocation();
                            return;
                        }
                    }
                    
                    // No weapon starts with this key, reset
                    if (characterAnimator != null)
                        characterAnimator.StopInvocationLoop();
                    invocationIndex = 0;
                    currentInvocationWeaponIndex = -1;
                    return;
                }
            }
        }

        // Try each weapon's invocation pattern (no weapon currently being followed)
        for (int w = 0; w < weapons.Length; ++w)
        {
            Weapon weapon = weapons[w];
            if (weapon == null || weapon.invocationPattern == null || weapon.invocationPattern.Length == 0)
                continue;

            if (pressed == weapon.invocationPattern[0])
            {
                invocationIndex = 1;
                invocationLastInputTime = Time.time;
                currentInvocationWeaponIndex = w;

                // Animation feedback
                if (characterAnimator != null)
                {
                    characterAnimator.PlayStartInvocation();
                }

                return;
            }
        }

        // No weapon pattern matched
        if (invocationIndex > 0 && characterAnimator != null)
            characterAnimator.StopInvocationLoop();
        invocationIndex = 0;
        currentInvocationWeaponIndex = -1;
    }

    void HandleAttackPatterns(KeyCode pressed)
    {
        if (currentWeapon == null || currentWeapon.actions == null || currentWeapon.actions.Length == 0)
            return;

        EnsureAttackPatternStates();

        for (int i = 0; i < currentWeapon.actions.Length; ++i)
        {
            WeaponAction action = currentWeapon.actions[i];
            if (action == null || action.pattern == null || action.pattern.Length == 0)
                continue;

            var st = attackPatternStates[i];

            if (st.index > 0 && Time.time - st.lastInputTime > inputTimeout)
                st.index = 0;

            if (pressed == action.pattern[st.index])
            {
                st.index++;
                st.lastInputTime = Time.time;

                if (st.index >= action.pattern.Length)
                {
                    Debug.LogFormat("Action pattern '{0}' complete!", action.actionName);

                    float timeSinceLastAction = Time.time - st.lastExecutedTime;
                    if (timeSinceLastAction < action.cooldownSeconds)
                    {
                        Debug.LogWarning("Action '" + action.actionName + "' on cooldown for " +
                            (action.cooldownSeconds - timeSinceLastAction) + "s more");
                        st.index = 0;
                        return;
                    }

                    st.lastExecutedTime = Time.time;

                    if (characterAnimator != null)
                        characterAnimator.PlayAttack(action.actionName);

                    if (action.effectPrefab != null)
                        TriggerAnimation.Spawn(action.effectPrefab, transform.position, 1.5f);

                    Enemy target = FindFirstObjectByType<Enemy>();
                    action.Execute(playerEntity, target != null ? (Entity)target : null);

                    st.index = 0;
                    return;
                }
            }
            else
            {
                st.index = pressed == action.pattern[0] ? 1 : 0;
                st.lastInputTime = Time.time;
            }
        }
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
        if (sequenceText == null || weaponManager == null)
            return;

        var sb = new System.Text.StringBuilder();

        if (!weaponInvoked)
        {
            // PHASE 1: Show invocation patterns
            sb.AppendLine("<color=#FF6600><b>WEAPON NOT INVOKED</b></color>");
            sb.AppendLine("Complete an invocation sequence to proceed:");
            sb.AppendLine();

            Weapon[] weapons = weaponManager.GetAvailableWeapons();
            if (weapons == null || weapons.Length == 0)
            {
                sequenceText.text = "(no weapons available)";
                return;
            }

            for (int w = 0; w < weapons.Length; ++w)
            {
                Weapon weapon = weapons[w];
                if (weapon == null || weapon.invocationPattern == null || weapon.invocationPattern.Length == 0)
                    continue;

                sb.Append(weapon.weaponName);
                sb.Append(" <i>(Invocation)</i>: ");

                for (int j = 0; j < weapon.invocationPattern.Length; ++j)
                {
                    // Only highlight if this is the weapon currently being followed AND this is the next key
                    bool isNext = (currentInvocationWeaponIndex == w && invocationIndex == j);
                    if (isNext)
                        sb.Append("<color=#FFFF00><b>");

                    sb.Append(KeyCodeToArrowSymbol(weapon.invocationPattern[j]));

                    if (isNext)
                        sb.Append("</b></color>");
                }

                if (w < weapons.Length - 1)
                    sb.AppendLine();
            }
        }
        else
        {
            // PHASE 2: Show attack patterns
            sb.AppendLine("<color=#00FF00><b>WEAPON INVOKED</b></color>");
            if (currentWeapon != null)
                sb.AppendLine("Weapon: <b>" + currentWeapon.weaponName + "</b>");
            sb.AppendLine("Cast attacks/spells using sequences:");
            sb.AppendLine();

            if (currentWeapon == null || currentWeapon.actions == null || currentWeapon.actions.Length == 0)
            {
                sequenceText.text = "(no actions configured for this weapon)";
                return;
            }

            for (int i = 0; i < currentWeapon.actions.Length; ++i)
            {
                WeaponAction action = currentWeapon.actions[i];
                if (action == null || action.pattern == null || action.pattern.Length == 0)
                    continue;

                sb.Append(action.actionName);
                sb.Append(": ");

                for (int j = 0; j < action.pattern.Length; ++j)
                {
                    bool isNext = (attackPatternStates != null && i < attackPatternStates.Length &&
                                   attackPatternStates[i] != null && attackPatternStates[i].index == j);
                    if (isNext)
                        sb.Append("<color=#FFFF00><b>");

                    sb.Append(KeyCodeToArrowSymbol(action.pattern[j]));

                    if (isNext)
                        sb.Append("</b></color>");
                }

                if (action.cooldownSeconds > 0f)
                {
                    float timeLeft = action.cooldownSeconds - (Time.time - attackPatternStates[i].lastExecutedTime);
                    if (timeLeft > 0f)
                        sb.Append("  <color=#FF0000>[" + timeLeft.ToString("F1") + "s]</color>");
                }

                if (i < currentWeapon.actions.Length - 1)
                    sb.AppendLine();
            }
        }

        sequenceText.text = sb.ToString();
    }

    /// <summary>
    /// Equip a weapon and load its attacks/animations
    /// </summary>
    private void EquipWeapon(Weapon weapon)
    {
        if (weapon == null)
        {
            Debug.LogWarning("UserManager: Cannot equip null weapon");
            return;
        }

        currentWeapon = weapon;

        // Setup character animations for this weapon
        if (characterAnimator != null)
        {
            characterAnimator.SetupWeaponAnimations(weapon);
        }

        // Initialize attack pattern states for this weapon
        EnsureAttackPatternStates();

        Debug.LogFormat("UserManager: Weapon '{0}' equipped with {1} actions",
            weapon.weaponName, weapon.actions != null ? weapon.actions.Length : 0);
    }

}
