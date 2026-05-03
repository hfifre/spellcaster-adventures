using UnityEngine;
using System.Collections;

public class EnemyAnimator : MonoBehaviour
{
    [Tooltip("Durée de l'animation de mort avant destruction du GameObject")]
    [SerializeField] private float deathAnimationDuration = 1f;

    private Animator animator;

    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashHurt   = Animator.StringToHash("Hurt");
    private static readonly int HashDeath  = Animator.StringToHash("Death");

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("EnemyAnimator: aucun Animator trouvé sur " + gameObject.name);
    }

    public void PlayAttack() => animator?.SetTrigger(HashAttack);
    public void PlayHurt()   => animator?.SetTrigger(HashHurt);

    public void PlayDeath(System.Action onComplete)
    {
        animator?.SetTrigger(HashDeath);
        StartCoroutine(DeathRoutine(onComplete));
    }

    IEnumerator DeathRoutine(System.Action onComplete)
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        onComplete?.Invoke();
    }
}
