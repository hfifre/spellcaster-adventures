using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Weapon
{
    [Header("Weapon Info")]
    public string weaponName = "Sword";
    
    [Header("Invocation")]
    [Tooltip("Pattern to invoke this weapon")]
    public KeyCode[] invocationPattern = new KeyCode[0];
    
    [System.Serializable]
    public class Attack
    {
        public string attackName = "Attack1";
        public KeyCode[] pattern = new KeyCode[0];
        public AnimationClip animationClip;
        public GameObject effectPrefab;
        public float damageAmount = 10f;
        public float cooldownSeconds = 0f;
        
        // Runtime state
        [System.NonSerialized] public float lastExecutedTime = -1000f;
    }
    
    [Header("Attacks")]
    [Tooltip("List of attacks for this weapon")]
    public Attack[] attacks = new Attack[0];
    
    [Header("Animations")]
    [Tooltip("Animation clip for equipping this weapon (relève)")]
    public AnimationClip invocationEndClip;
    
    [Tooltip("Animation clip for idle with this weapon")]
    public AnimationClip standStillClip;
}
