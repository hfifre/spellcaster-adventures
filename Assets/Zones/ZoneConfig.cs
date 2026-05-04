using UnityEngine;

[CreateAssetMenu(fileName = "ZoneConfig_Difficulty1", menuName = "Spellcaster/Zone Config")]
public class ZoneConfig : ScriptableObject
{
    [Header("Zone Info")]
    public string zoneName = "Zone 1";
    public int difficulty = 1;

    [Header("Enemy Pool")]
    [Tooltip("Liste des prefabs d'ennemis disponibles dans cette zone (sélection aléatoire avec chances égales)")]
    public GameObject[] enemyPrefabs = new GameObject[0];

    /// <summary>
    /// Retourne un prefab d'ennemi aléatoire de cette zone.
    /// </summary>
    public GameObject GetRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("ZoneConfig '" + zoneName + "': aucun ennemi configuré.");
            return null;
        }
        return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
    }
}
