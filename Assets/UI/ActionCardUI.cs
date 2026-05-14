using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Transform arrowsContainer;
    [SerializeField] private GameObject cooldownOverlay;
    [SerializeField] private TMP_Text cooldownText;

    private static readonly Color ColorDefault  = new Color(1f, 1f, 1f, 0.35f);
    private static readonly Color ColorNext     = Color.white;
    private static readonly Color ColorDone     = new Color(0.4f, 0.9f, 0.4f, 1f);

    private Image[] arrowImages;

    public void Setup(string label, KeyCode[] pattern, ArrowIconsConfig config)
    {
        if (labelText) labelText.text = label;

        foreach (Transform child in arrowsContainer)
            Destroy(child.gameObject);

        arrowImages = new Image[pattern.Length];
        for (int i = 0; i < pattern.Length; i++)
        {
            var go = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(arrowsContainer, false);
            var img = go.GetComponent<Image>();
            img.sprite = config.Get(pattern[i]);
            img.color  = ColorDefault;
            arrowImages[i] = img;
        }

        SetCooldown(0f);
    }

    public void UpdateProgress(int nextIndex)
    {
        if (arrowImages == null) return;
        for (int i = 0; i < arrowImages.Length; i++)
        {
            if (i < nextIndex)       arrowImages[i].color = ColorDone;
            else if (i == nextIndex) arrowImages[i].color = ColorNext;
            else                     arrowImages[i].color = ColorDefault;
        }
    }

    public void SetCooldown(float remaining)
    {
        bool onCooldown = remaining > 0f;
        if (cooldownOverlay) cooldownOverlay.SetActive(onCooldown);
        if (cooldownText)    cooldownText.text = onCooldown ? remaining.ToString("F1") + "s" : "";
    }
}
