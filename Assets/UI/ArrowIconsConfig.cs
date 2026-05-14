using UnityEngine;

[CreateAssetMenu(fileName = "ArrowIconsConfig", menuName = "Spellcaster/Arrow Icons Config")]
public class ArrowIconsConfig : ScriptableObject
{
    public Sprite up;
    public Sprite down;
    public Sprite left;
    public Sprite right;

    public Sprite Get(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.UpArrow:    return up;
            case KeyCode.DownArrow:  return down;
            case KeyCode.LeftArrow:  return left;
            case KeyCode.RightArrow: return right;
            default:                 return null;
        }
    }
}
