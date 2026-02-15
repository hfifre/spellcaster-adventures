using UnityEngine;
using System.Collections.Generic;

public class SpellManager : MonoBehaviour
{
    [System.Serializable]
    public class SpellEntry
    {
        public string spellName;
        public KeyCode[] sequence = new KeyCode[0];
        public GameObject spellPrefab;
        public float spellDuration = 2f;

        [Header("Optional effects")]
        public float cooldownSeconds = 0f;

        // runtime state
        [System.NonSerialized] public float lastExecutedTime = -1000f;
    }

    [Tooltip("List of available spells with their sequences and prefabs")]
    public SpellEntry[] spells = new SpellEntry[0];

    [Tooltip("Position where spells are spawned")]
    public Vector3 spawnPosition = new Vector3(4.5f, -0.2f, 0f);

    void Awake()
    {
        // Optional: validate spell entries
        if (spells != null)
        {
            foreach (var spell in spells)
            {
                if (spell == null)
                {
                    Debug.LogWarning("SpellManager: found null entry in spells array.");
                    continue;
                }
                if (string.IsNullOrEmpty(spell.spellName))
                    Debug.LogWarning("SpellManager: found spell entry with empty name.");
                if (spell.spellPrefab == null)
                    Debug.LogWarning("SpellManager: found spell entry '" + spell.spellName + "' with null prefab.");
            }
        }
    }

    // Try to execute a spell by name; check cooldown and spawn
    public bool TryExecuteSpell(string spellName)
    {
        // find spell entry
        var entry = FindSpellByName(spellName);
        if (entry == null)
        {
            Debug.LogWarning("SpellManager: no spell named '" + spellName + "'");
            return false;
        }

        // check cooldown
        if (Time.time - entry.lastExecutedTime < entry.cooldownSeconds)
        {
            Debug.LogWarning("SpellManager: spell '" + spellName + "' on cooldown for " + 
                (entry.cooldownSeconds - (Time.time - entry.lastExecutedTime)) + "s more");
            return false;
        }

        // execute
        entry.lastExecutedTime = Time.time;
        TriggerAnimation.Spawn(entry.spellPrefab, spawnPosition, entry.spellDuration);
        Debug.LogFormat("SpellManager: executed spell '{0}'", spellName);
        return true;
    }

    // Check if a sequence matches any spell; return spell name if match, null otherwise
    public string GetSpellNameForSequence(KeyCode[] pressedSequence)
    {
        if (spells == null)
            return null;

        for (int i = 0; i < spells.Length; ++i)
        {
            var spell = spells[i];
            if (spell != null && SequencesMatch(spell.sequence, pressedSequence))
                return spell.spellName;
        }

        return null;
    }

    // Helper to check if two sequences are identical
    bool SequencesMatch(KeyCode[] a, KeyCode[] b)
    {
        if (a == null || b == null)
            return false;
        if (a.Length != b.Length)
            return false;
        for (int i = 0; i < a.Length; ++i)
            if (a[i] != b[i])
                return false;
        return true;
    }

    // Find spell entry by name
    SpellEntry FindSpellByName(string spellName)
    {
        if (spells == null || string.IsNullOrEmpty(spellName))
            return null;
        for (int i = 0; i < spells.Length; ++i)
            if (spells[i] != null && spells[i].spellName == spellName)
                return spells[i];
        return null;
    }
}
