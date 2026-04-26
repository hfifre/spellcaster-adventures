using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Tooltip("Weapons available to invoke at the start of a run")]
    [SerializeField] private List<Weapon> availableWeapons = new List<Weapon>();

    void Awake()
    {
        if (availableWeapons == null || availableWeapons.Count == 0)
            Debug.LogWarning("WeaponManager: No weapons configured!");
    }

    public Weapon[] GetAvailableWeapons()
    {
        return availableWeapons.ToArray();
    }

    /// <summary>
    /// Adds a weapon to the available pool (called when loot is picked up during a run).
    /// </summary>
    public void AddWeapon(Weapon weapon)
    {
        if (weapon == null || availableWeapons.Contains(weapon)) return;
        availableWeapons.Add(weapon);
        Debug.LogFormat("WeaponManager: '{0}' added to available weapons", weapon.weaponName);
    }

    /// <summary>
    /// Resets the weapon pool to a given starting set (call this at the start of a new run).
    /// </summary>
    public void InitRun(List<Weapon> startingWeapons)
    {
        availableWeapons = new List<Weapon>(startingWeapons);
    }
}
