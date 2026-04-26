using UnityEngine;

public class PlayerEntity : Entity
{
    [SerializeField] private HUDManager hudManager;

    protected override void Awake()
    {
        base.Awake();
        if (hudManager == null)
            hudManager = FindFirstObjectByType<HUDManager>();
    }

    protected override void OnHealthChanged()
    {
        if (hudManager != null)
            hudManager.UpdateHealthBar(currentHealth, maxHealth);
    }

    protected override void Die()
    {
        Debug.Log("Player died!");
        // TODO: déclencher l'écran de game over
    }
}
