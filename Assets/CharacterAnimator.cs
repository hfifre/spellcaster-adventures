using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    private Animator animator;
    private RuntimeAnimatorController baseController;
    private AnimatorOverrideController overrideController;

    // Animation parameter hashes (optimization)
    private int hashStandStillNoWeapon = Animator.StringToHash("StandStillNoWeapon");
    private int hashInvocationStart = Animator.StringToHash("InvocationStart");
    private int hashInvocationLoop = Animator.StringToHash("InvocationLoop");
    private int hashInvocationEnd = Animator.StringToHash("InvocationEnd");
    private int hashStandStill = Animator.StringToHash("StandStill");
    private int hashAttack = Animator.StringToHash("Attack");

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("CharacterAnimator: Animator component not found on " + gameObject.name);
        else
            baseController = animator.runtimeAnimatorController;
    }

    // Check if currently in StandStillNoWeapon state
    public bool IsInStandStillNoWeapon()
    {
        if (animator == null) return false;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.shortNameHash == hashStandStillNoWeapon;
    }

    // Check if currently in Attack state
    public bool IsInAttackState()
    {
        if (animator == null) return false;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.shortNameHash == hashAttack;
    }

    // Play stand still without weapon animation (idle, no weapon)
    public void PlayStandStillNoWeapon()
    {
        if (animator == null) return;
        animator.SetTrigger(hashStandStillNoWeapon);
        Debug.Log("CharacterAnimator: Playing StandStillNoWeapon");
    }

    // Play stand still without weapon, then transition to invocation start
    public void PlayStartInvocation()
    {
        if (animator == null) return;
        if(IsInStandStillNoWeapon())
            animator.CrossFadeInFixedTime(hashInvocationStart, 0f, 0); // 0 duration = instant transition
        else
            animator.SetTrigger(hashInvocationStart);
        Debug.Log("CharacterAnimator: Playing StandStillNoWeapon -> InvocationStart sequence");
    }

    // Play the invocation loop animation (character stays crouched, idle loop)
    public void PlayInvocationLoop()
    {
        if (animator == null) return;
        animator.SetBool(hashInvocationLoop, true);
        Debug.Log("CharacterAnimator: Playing InvocationLoop");
    }

    // Stop the invocation loop animation
    public void StopInvocationLoop()
    {
        if (animator == null) return;
        animator.SetBool(hashInvocationLoop, false);
        Debug.Log("CharacterAnimator: Stopped InvocationLoop");
    }

    // Play the invocation end animation (character stands up with weapon)
    public void PlayInvocationEnd()
    {
        if (animator == null) return;
        animator.CrossFadeInFixedTime(hashInvocationEnd, 0f, 0); // 0 duration = instant transition
        Debug.Log("CharacterAnimator: Playing InvocationEnd");
    }

    // Play the stand still animation (idle with weapon)
    public void PlayStandStill()
    {
        if (animator == null) return;
        
        // Use SetBool to allow interruption by other animations
        animator.SetBool(hashStandStill, true);
        Debug.Log("CharacterAnimator: Playing StandStill");
    }

    // Play an attack/spell animation. Optional attackName parameter for future use
    public void PlayAttack(string attackName = "")
    {
        if (animator == null) return;
        animator.CrossFadeInFixedTime(hashAttack, 0f, 0);
        Debug.Log("CharacterAnimator: Playing Attack (" + (string.IsNullOrEmpty(attackName) ? "default" : attackName) + ")");
    }

    // Override the Attack clip with a weapon-action-specific animation
    public void SetAttackAnimation(AnimationClip clip)
    {
        if (overrideController == null || clip == null) return;
        foreach (var baseClip in baseController.animationClips)
        {
            if (baseClip.name.Contains("Attack"))
            {
                overrideController[baseClip] = clip;
                return;
            }
        }
        Debug.LogWarning("CharacterAnimator: no 'Attack' clip found in base controller to override.");
    }

    /// <summary>
    /// Setup weapon-specific animations using AnimatorOverrideController
    /// </summary>
    public void SetupWeaponAnimations(Weapon weapon)
    {
        if (animator == null || weapon == null)
        {
            Debug.LogWarning("CharacterAnimator: Cannot setup weapon animations - missing animator or weapon");
            return;
        }

        if (baseController == null)
        {
            Debug.LogError("CharacterAnimator: baseController is null! Make sure Animator has a controller assigned.");
            return;
        }

        Debug.Log("SetupWeaponAnimations: Starting setup for weapon " + weapon.weaponName);

        // Create override controller if needed
        if (overrideController == null)
        {
            Debug.Log("Creating new AnimatorOverrideController");
            overrideController = new AnimatorOverrideController(baseController);
            animator.runtimeAnimatorController = overrideController;
            Debug.Log("Assigned override controller to animator");
        }

        // Get all animation clips from the base controller
        AnimationClip[] baseClips = baseController.animationClips;
        Debug.LogFormat("Found {0} clips in base controller", baseClips.Length);
        
        foreach (var clip in baseClips)
        {
            Debug.Log("  Base clip: " + (clip != null ? clip.name : "null"));
        }

        // Override with weapon-specific animations
        int overrideCount = 0;
        for (int i = 0; i < baseClips.Length; i++)
        {
            AnimationClip clip = baseClips[i];
            
            if (clip == null) continue;
            
            Debug.LogFormat("  Checking clip: '{0}'", clip.name);
            Debug.LogFormat("    Contains 'InvocationEnd'? {0}, weapon.invocationEndClip assigned? {1}", 
                clip.name.Contains("InvocationEnd"), weapon.invocationEndClip != null);
            Debug.LogFormat("    Contains 'StandStill'? {0}, weapon.standStillClip assigned? {1}", 
                clip.name.Contains("StandStill"), weapon.standStillClip != null);
            
            if (clip.name.Contains("InvocationEnd") && weapon.invocationEndClip != null)
            {
                overrideController[clip] = weapon.invocationEndClip;
                Debug.LogFormat("✓ Overriding '{0}' with {1}", clip.name, weapon.invocationEndClip.name);
                overrideCount++;
            }
            else if (clip.name.Contains("StandStill") && weapon.standStillClip != null)
            {
                overrideController[clip] = weapon.standStillClip;
                Debug.LogFormat("✓ Overriding '{0}' with {1}", clip.name, weapon.standStillClip.name);
                overrideCount++;
            }
        }
        
        if (overrideCount == 0)
        {
            Debug.LogWarning("CharacterAnimator: No clips were overridden! Check weapon animation clips.");
        }

        Debug.LogFormat("CharacterAnimator: Weapon '{0}' animations setup complete ({1} overrides)", weapon.weaponName, overrideCount);
    }

    /// <summary>
    /// Reset to original animator controller
    /// </summary>
    public void ResetAnimations()
    {
        if (animator == null)
            return;
        
        animator.runtimeAnimatorController = baseController;
        overrideController = null;
        Debug.Log("CharacterAnimator: Animations reset to default");
    }
}
