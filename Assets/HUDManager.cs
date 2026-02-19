using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBarFill;
    
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float healthPercent = currentHealth / maxHealth;
        healthSlider.value = currentHealth;  // Pas de pourcentage si Max Value = 100
        healthText.text = $"{currentHealth}/{maxHealth}";
        
        // Changer la couleur selon la santé
        healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
    }
}