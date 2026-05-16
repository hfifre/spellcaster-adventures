using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Spellcaster/Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName = "Sword";
    [TextArea(1, 3)]
    public string description = "";

    [Header("Invocation")]
    [Tooltip("Pattern to invoke this weapon")]
    public KeyCode[] invocationPattern = new KeyCode[0];

    [Header("Actions")]
    [Tooltip("List of actions available with this weapon (damage, heal, buff...)")]
    public WeaponAction[] actions = new WeaponAction[0];

    [Header("Animations")]
    [Tooltip("Animation clip for equipping this weapon")]
    public AnimationClip invocationEndClip;

    [Tooltip("Animation clip for idle with this weapon")]
    public AnimationClip standStillClip;

    [Tooltip("Animation pendant le dash d'approche (corps à corps)")]
    public AnimationClip meleeDashClip;

    [Tooltip("Animation pendant le retour après l'attaque (corps à corps)")]
    public AnimationClip meleeReturnClip;
}
