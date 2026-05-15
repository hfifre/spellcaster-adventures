using UnityEngine;

public abstract class WeaponAction : ScriptableObject
{
    [Header("Action Info")]
    public string actionName = "Action";
    [TextArea(1, 3)]
    public string description = "";

    [Header("Input")]
    public KeyCode[] pattern = new KeyCode[0];

    [Header("Animation & Visual")]
    [Tooltip("Animation du personnage pendant le cast (optionnel — si vide, le personnage reste en idle)")]
    public AnimationClip characterAnimationClip;
    [Tooltip("Prefab GameObject de l'effet visuel (SpriteRenderer + Animator)")]
    public GameObject effectPrefab;
    [Tooltip("Décalage en world units par rapport à la position du caster")]
    public Vector2 spawnOffset = Vector2.zero;
    [Tooltip("Durée avant destruction de l'effet s'il n'a pas de SpellEffect")]
    public float effectDuration = 2f;

    [Header("Cooldown")]
    public float cooldownSeconds = 0f;

    public abstract void Execute(Entity caster, Entity target);

    /// <summary>
    /// Appelé par UserManager quand le paterne est complété.
    /// Par défaut joue l'animation d'attaque. Les sous-classes peuvent override
    /// pour gérer leur propre timing (ex. MeleeAction gère l'anim dans MeleeDashEffect).
    /// </summary>
    public virtual void OnPatternComplete(CharacterAnimator animator)
    {
        if (characterAnimationClip != null)
        {
            animator?.SetAttackAnimation(characterAnimationClip);
            animator?.PlayAttack(actionName);
        }
        // Si pas de clip assigné, le personnage reste en idle
    }

    protected void SpawnEffect(Entity caster, Entity target)
    {
        if (effectPrefab == null) return;
        Vector3 spawnPos = caster.transform.position + (Vector3)spawnOffset;
        var go = Object.Instantiate(effectPrefab, spawnPos, Quaternion.identity);
        var effect = go.GetComponent<SpellEffect>();
        if (effect != null)
            effect.Init(caster, target);
        else
            Object.Destroy(go, effectDuration);
    }
}
