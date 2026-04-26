using UnityEngine;

public abstract class WeaponAction : ScriptableObject
{
    [Header("Action Info")]
    public string actionName = "Action";

    [Header("Input")]
    public KeyCode[] pattern = new KeyCode[0];

    [Header("Animation & Visual")]
    public AnimationClip animationClip;
    public GameObject effectPrefab;

    [Header("Cooldown")]
    public float cooldownSeconds = 0f;

    public abstract void Execute(Entity caster, Entity target);
}
