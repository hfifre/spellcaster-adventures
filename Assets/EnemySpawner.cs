using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private ZoneConfig zoneConfig;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    [SerializeField] private bool debugMode = false;

    void Start()
    {
        if (zoneConfig == null)
        {
            Debug.LogWarning("EnemySpawner: aucune ZoneConfig assignée.");
            return;
        }
        SpawnEnemy();
    }

    /// <summary>
    /// Spawne un ennemi aléatoire de la zone à la position du spawner.
    /// </summary>
    [ContextMenu("Spawn Enemy")]
    public void SpawnEnemy()
    {
        if (zoneConfig == null)
        {
            Debug.LogError("EnemySpawner: impossible de spawner — aucune ZoneConfig assignée.");
            return;
        }

        GameObject prefab = zoneConfig.GetRandomEnemy();
        if (prefab == null)
            return;

        Vector3 spawnPos = transform.position + spawnOffset;
        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);

        var enemy = instance.GetComponent<Enemy>();
        if (enemy != null)
            instance.transform.position = spawnPos + enemy.SpawnOffset;

        if (debugMode)
            Debug.LogFormat("EnemySpawner: spawned '{0}' from zone '{1}'", prefab.name, zoneConfig.zoneName);
    }

    /// <summary>
    /// Spawne N ennemis.
    /// </summary>
    public void SpawnMultiple(int count)
    {
        for (int i = 0; i < count; i++)
            SpawnEnemy();
    }
}
