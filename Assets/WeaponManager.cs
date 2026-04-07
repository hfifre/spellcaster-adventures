using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Tooltip("List of all available weapons")]
    [SerializeField] private Weapon[] availableWeapons = new Weapon[0];
    
    private Weapon currentWeapon = null;
    
    void Awake()
    {
        if (availableWeapons == null || availableWeapons.Length == 0)
        {
            Debug.LogWarning("WeaponManager: No weapons configured!");
        }
    }
    
    /// <summary>
    /// Equip a weapon by name
    /// </summary>
    public bool EquipWeapon(string weaponName)
    {
        if (availableWeapons == null)
            return false;
        
        for (int i = 0; i < availableWeapons.Length; ++i)
        {
            if (availableWeapons[i] != null && availableWeapons[i].weaponName == weaponName)
            {
                currentWeapon = availableWeapons[i];
                Debug.LogFormat("WeaponManager: Equipped weapon '{0}'", weaponName);
                return true;
            }
        }
        
        Debug.LogWarning("WeaponManager: Weapon '" + weaponName + "' not found!");
        return false;
    }
    
    /// <summary>
    /// Get the currently equipped weapon
    /// </summary>
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    
    /// <summary>
    /// Check if a weapon is equipped
    /// </summary>
    public bool IsWeaponEquipped()
    {
        return currentWeapon != null;
    }
    
    /// <summary>
    /// Get a weapon by name without equipping it
    /// </summary>
    public Weapon GetWeaponByName(string weaponName)
    {
        if (availableWeapons == null)
            return null;
        
        for (int i = 0; i < availableWeapons.Length; ++i)
        {
            if (availableWeapons[i] != null && availableWeapons[i].weaponName == weaponName)
                return availableWeapons[i];
        }
        
        return null;
    }
    
    /// <summary>
    /// Get all available weapons
    /// </summary>
    public Weapon[] GetAvailableWeapons()
    {
        return availableWeapons;
    }
}
