using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAction", menuName = "Spellcaster/Actions/Melee")]
public class MeleeAction : WeaponAction
{
    [Header("Melee Parameters")]
    public float damage = 15f;
    public float dashSpeed = 15f;
    [Tooltip("Distance d'arrêt avant l'ennemi sur l'axe X")]
    public float stopDistance = 0.5f;
    // characterAnimationClip (hérité de WeaponAction) = animation d'attaque à l'impact
    // dashClip et returnClip sont sur l'arme (Weapon.meleeDashClip / Weapon.meleeReturnClip)

    public override void Execute(Entity caster, Entity target)
    {
        if (target == null) return;
        var dash = caster.GetComponent<MeleeDashEffect>();
        if (dash != null)
            dash.StartDash(target, damage, dashSpeed, stopDistance, characterAnimationClip);
        else
            Debug.LogWarning("MeleeAction: MeleeDashEffect not found on caster.");
    }

    public override void OnPatternComplete(CharacterAnimator animator) { }
}
