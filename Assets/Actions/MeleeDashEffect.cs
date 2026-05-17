using UnityEngine;
using System.Collections;

public class MeleeDashEffect : MonoBehaviour
{
    private bool isDashing;
    public bool IsDashing => isDashing;

    private Weapon currentWeapon;

    public void SetWeapon(Weapon weapon) => currentWeapon = weapon;

    public void StartDash(Entity target, float damage, float dashSpeed, float stopDistance, AnimationClip attackClip, float impactDelay = 0f)
    {
        if (isDashing || target == null) return;
        StartCoroutine(DashRoutine(target, damage, dashSpeed, stopDistance, attackClip, impactDelay));
    }

    IEnumerator DashRoutine(Entity target, float damage, float dashSpeed, float stopDistance, AnimationClip attackClip, float impactDelay)
    {
        isDashing = true;
        var anim = GetComponent<CharacterAnimator>();
        Vector3 origin = transform.position;

        float dir = Mathf.Sign(target.transform.position.x - origin.x);
        float scaledStop = stopDistance * Mathf.Abs(transform.localScale.x);
        Vector3 destination = new Vector3(
            target.transform.position.x - dir * scaledStop,
            origin.y,
            origin.z
        );

        // Phase 1 — approche
        var dashClip = currentWeapon?.meleeDashClip;
        if (dashClip != null) { anim?.SetAttackAnimation(dashClip); anim?.PlayAttack(""); }
        while (Vector3.Distance(transform.position, destination) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, dashSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = destination;

        // Phase 2 — impact : animation immédiate, dégâts après impactDelay
        if (attackClip != null) { anim?.SetAttackAnimation(attackClip); anim?.PlayAttack(""); }
        if (impactDelay > 0f) yield return new WaitForSeconds(impactDelay);
        target.TakeDamage(damage);
        float postImpactWait = Mathf.Max(0f, (attackClip != null ? attackClip.length : 0.3f) - impactDelay);
        if (postImpactWait > 0f) yield return new WaitForSeconds(postImpactWait);

        // Phase 3 — retour
        var returnClip = currentWeapon?.meleeReturnClip;
        if (returnClip != null) { anim?.SetAttackAnimation(returnClip); anim?.PlayAttack(""); }
        while (Vector3.Distance(transform.position, origin) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, origin, dashSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = origin;

        // Phase 4 — idle arme
        anim?.PlayStandStill();
        isDashing = false;
    }
}
