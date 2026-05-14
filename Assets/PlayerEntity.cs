using UnityEngine;

public class PlayerEntity : Entity
{
    [SerializeField] private CombatHUD combatHUD;

    protected override void Awake()
    {
        base.Awake();
        if (combatHUD == null)
            combatHUD = FindFirstObjectByType<CombatHUD>();
    }

    void Start()
    {
        combatHUD?.UpdateHealth(currentHealth, maxHealth);
    }

    protected override void OnHealthChanged()
    {
        combatHUD?.UpdateHealth(currentHealth, maxHealth);
    }

    protected override void Die()
    {
        Debug.Log("Player died!");
        // TODO: déclencher l'écran de game over
    }
}
