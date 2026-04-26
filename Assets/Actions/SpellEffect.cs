using UnityEngine;

public abstract class SpellEffect : MonoBehaviour
{
    public abstract void Init(Entity caster, Entity target);
}
