using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatHUD : MonoBehaviour
{
    [Header("Top Bar — Santé")]
    [SerializeField] private TMP_Text healthText;

    [Header("Barre du bas — Cartes d'actions")]
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private GameObject actionCardPrefab;

    [Header("Config")]
    [SerializeField] private ArrowIconsConfig arrowConfig;

    private ActionCardUI[] cards = new ActionCardUI[0];

    // ── Santé ─────────────────────────────────────────────────────────

    public void UpdateHealth(float current, float max)
    {
        if (healthText)
            healthText.text = Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
    }

    // ── Phase invocation — une carte par arme ─────────────────────────

    public void BuildInvocationCards(Weapon[] weapons)
    {
        ClearCards();
        if (weapons == null) return;

        cards = new ActionCardUI[weapons.Length];
        for (int i = 0; i < weapons.Length; i++)
        {
            var card = CreateCard();
            card.Setup(weapons[i].weaponName, weapons[i].description, weapons[i].invocationPattern, arrowConfig);
            cards[i] = card;
        }
    }

    public void UpdateInvocationCards(int followedWeaponIndex, int progress)
    {
        for (int i = 0; i < cards.Length; i++)
            cards[i].UpdateProgress(i == followedWeaponIndex ? progress : 0);
    }

    // ── Phase combat — une carte par action ───────────────────────────

    public void BuildCombatCards(WeaponAction[] actions)
    {
        ClearCards();
        if (actions == null) return;

        cards = new ActionCardUI[actions.Length];
        for (int i = 0; i < actions.Length; i++)
        {
            var card = CreateCard();
            card.Setup(actions[i].actionName, actions[i].description, actions[i].pattern, arrowConfig);
            cards[i] = card;
        }
    }

    public void UpdateCombatCards(int[] patternIndices, float[] cooldowns)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].UpdateProgress(patternIndices[i]);
            cards[i].SetCooldown(cooldowns[i]);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────

    void ClearCards()
    {
        foreach (Transform child in cardsContainer)
            Destroy(child.gameObject);
        cards = new ActionCardUI[0];
    }

    ActionCardUI CreateCard()
    {
        var go = Instantiate(actionCardPrefab, cardsContainer);
        return go.GetComponent<ActionCardUI>();
    }
}
