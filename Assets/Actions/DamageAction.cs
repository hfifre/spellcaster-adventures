using UnityEngine;

[CreateAssetMenu(fileName = "NewDamageAction", menuName = "Spellcaster/Actions/Damage")]
public class DamageAction : WeaponAction
{
    public float damage = 10f;

    public override void Execute(Entity caster, Entity target)
    {
        SpawnEffect(caster, target);
    }

    public override void OnImpact(Entity caster, Entity target)
    {
        target?.TakeDamage(damage);
    }
}
