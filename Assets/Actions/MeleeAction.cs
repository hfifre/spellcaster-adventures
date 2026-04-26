using UnityEngine;

[CreateAssetMenu(fileName = "NewMeleeAction", menuName = "Spellcaster/Actions/Melee")]
public class MeleeAction : WeaponAction
{
    public float damage = 15f;
    public float dashSpeed = 15f;

    public override void Execute(Entity caster, Entity target)
    {
        if (target == null) return;
        var dash = caster.GetComponent<MeleeDashEffect>();
        if (dash != null)
            dash.StartDash(target, damage, dashSpeed);
        else
            Debug.LogWarning("MeleeAction: MeleeDashEffect not found on caster.");
    }

    // MeleeDashEffect gère l'animation au moment de l'impact
    public override void OnPatternComplete(CharacterAnimator animator) { }
}
