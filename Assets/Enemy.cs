using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Spawn")]
    [Tooltip("Décalage appliqué par rapport au point de spawn (compense la taille du sprite)")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    public Vector3 SpawnOffset => spawnOffset;

    private PlayerEntity player;
    private EnemyAnimator enemyAnimator;
    private float lastAttackTime;

    protected override void Awake()
    {
        base.Awake();
        player = FindFirstObjectByType<PlayerEntity>();
        enemyAnimator = GetComponent<EnemyAnimator>();
    }

    void Update()
    {
        if (player == null || !player.IsAlive) return;
        TryAttack();
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;
        player.TakeDamage(attackDamage);
        enemyAnimator?.PlayAttack();
        Debug.LogFormat("Enemy '{0}' dealt {1} damage to player", gameObject.name, attackDamage);
    }

    protected override void OnHealthChanged()
    {
        if (IsAlive)
            enemyAnimator?.PlayHurt();
    }

    protected override void Die()
    {
        enabled = false; // stoppe Update et TryAttack immédiatement
        Debug.LogFormat("Enemy '{0}' died", gameObject.name);

        if (enemyAnimator != null)
            enemyAnimator.PlayDeath(() => Destroy(gameObject));
        else
            Destroy(gameObject);
    }
}
