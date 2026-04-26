using UnityEngine;

public class TriggerAnimation : MonoBehaviour
{
    public static GameObject Spawn(GameObject prefab, Vector3 position, float duration)
    {
        if (prefab == null)
        {
            Debug.LogWarning("TriggerAnimation.Spawn called with null prefab.");
            return null;
        }

        var instance = Instantiate(prefab, position, Quaternion.identity);

        var animator = instance.GetComponent<Animator>();
        if (animator != null)
        {
            var clips = animator.runtimeAnimatorController?.animationClips;
            if (clips != null && clips.Length > 0)
                animator.Play(clips[0].name, 0, 0f);
        }

        Destroy(instance, duration);
        return instance;
    }

    public GameObject SpawnInstance(GameObject prefab, Vector3 position, float duration)
    {
        return Spawn(prefab, position, duration);
    }
}
