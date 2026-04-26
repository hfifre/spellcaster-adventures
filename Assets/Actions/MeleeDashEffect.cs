using UnityEngine;
using System.Collections;

public class MeleeDashEffect : MonoBehaviour
{
    private bool isDashing;
    public bool IsDashing => isDashing;

    public void StartDash(Entity target, float damage, float dashSpeed)
    {
        if (isDashing || target == null) return;
        StartCoroutine(DashRoutine(target, damage, dashSpeed));
    }

    IEnumerator DashRoutine(Entity target, float damage, float dashSpeed)
    {
        isDashing = true;
        Vector3 origin = transform.position;
        Vector3 destination = target.transform.position;

        // Dash vers l'ennemi
        while (Vector3.Distance(transform.position, destination) > 0.15f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, dashSpeed * Time.deltaTime);
            yield return null;
        }

        // Impact : dégâts + animation d'attaque
        target.TakeDamage(damage);
        GetComponent<CharacterAnimator>()?.PlayAttack("");

        yield return new WaitForSeconds(0.1f);

        // Retour à la position d'origine
        while (Vector3.Distance(transform.position, origin) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, origin, dashSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = origin;
        isDashing = false;
    }
}
