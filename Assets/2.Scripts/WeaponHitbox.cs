using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    public enum OwnerType { Player, AI }
    public OwnerType owner;

    [Header("Damage")]
    public int damage = 1;

    [Header("Hit Cooldown (sec)")]
    [SerializeField] private float hitCooldown = 0.4f;

    private bool canDealDamage = true;

    private void Reset()
    {
        // ��Ʈ�ڽ� �ݶ��̴��� �ݵ�� Trigger
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;
       
        if (owner == OwnerType.Player)
        {
            var ai = other.GetComponentInParent<AIController>();
            if (ai == null) return;

            if (ai.IsBlocking())
            {
                GameEvents.EvadeOccurred(false, ai.transform.position); // AI ��� �� ����
                ai.PlayBlockReaction();                                  // ��� ��� ����
                return;
            }

            ai.TakeHit(damage);
            StartCoroutine(Cooldown());
        }
        else if (owner == OwnerType.AI)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            if (player.IsBlocking())
            {
                GameEvents.EvadeOccurred(true, player.transform.position); // �÷��̾� ��� �� ���
                player.PlayBlockReaction();
                return;
            }

            player.TakeHit(damage);
            StartCoroutine(Cooldown());
        }
    }

    private IEnumerator Cooldown()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(hitCooldown);
        canDealDamage = true;
    }
}
