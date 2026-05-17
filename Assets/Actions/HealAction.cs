using UnityEngine;

[CreateAssetMenu(fileName = "NewHealAction", menuName = "Spellcaster/Actions/Heal")]
public class HealAction : WeaponAction
{
    public float healAmount = 20f;

    public override void Execute(Entity caster, Entity target)
    {
        SpawnEffect(caster, target);
    }

    public override void OnImpact(Entity caster, Entity target)
    {
        caster?.Heal(healAmount);
    }
}
