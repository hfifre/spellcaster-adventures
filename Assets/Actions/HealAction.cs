using UnityEngine;

[CreateAssetMenu(fileName = "NewHealAction", menuName = "Spellcaster/Actions/Heal")]
public class HealAction : WeaponAction
{
    public float healAmount = 20f;

    public override void Execute(Entity caster, Entity target)
    {
        caster?.Heal(healAmount);
        SpawnEffect(caster, target);
    }
}
