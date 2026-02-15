using UnityEngine;

public class TriggerAnimation : MonoBehaviour
{
    // Static helper to spawn a spell prefab and schedule destruction.
    // If the prefab has a SpellBeheviour component, calls Trigger(); otherwise just destroys after duration.
    public static GameObject Spawn(GameObject prefab, Vector3 position, float duration)
    {
        if (prefab == null)
        {
            Debug.LogWarning("TriggerAnimation.Spawn called with null prefab.");
            return null;
        }

        var instance = Instantiate(prefab, position, Quaternion.identity);
        Debug.LogFormat("TriggerAnimation.Spawn: instantiated prefab '{0}' at {1}", prefab.name, position);

        // Try to find and trigger SpellBeheviour if it exists (optional dependency)
        var animator = instance.GetComponent<Animator>();
        if (animator != null)
        {
            // If prefab has Animator, play its first animation clip
            var clips = animator.runtimeAnimatorController?.animationClips;
            if (clips != null && clips.Length > 0)
            {
                animator.Play(clips[0].name, 0, 0f);
                animator.Update(0f);
                Debug.LogFormat("TriggerAnimation.Spawn: playing animation '{0}'", clips[0].name);
            }
        }

        // Schedule destruction after duration
        Destroy(instance, duration);

        return instance;
    }

    // Instance wrapper (optional) so you can call from inspector-linked components
    public GameObject SpawnInstance(GameObject prefab, Vector3 position, float duration)
    {
        return Spawn(prefab, position, duration);
    }
}
