using UnityEngine;

public class Enemy : Entity
{
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 2f;

    private PlayerEntity player;
    private float lastAttackTime;

    protected override void Awake()
    {
        base.Awake();
        player = FindFirstObjectByType<PlayerEntity>();
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
        Debug.LogFormat("Enemy '{0}' dealt {1} damage to player", gameObject.name, attackDamage);
    }

    protected override void Die()
    {
        Debug.LogFormat("Enemy '{0}' died", gameObject.name);
        // TODO: drop loot
        Destroy(gameObject);
    }
}
