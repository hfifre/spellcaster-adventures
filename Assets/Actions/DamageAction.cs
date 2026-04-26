using UnityEngine;

[CreateAssetMenu(fileName = "NewDamageAction", menuName = "Spellcaster/Actions/Damage")]
public class DamageAction : WeaponAction
{
    public float damage = 10f;

    public override void Execute(Entity caster, Entity target)
    {
        target?.TakeDamage(damage);
        SpawnEffect(caster, target);
    }
}
