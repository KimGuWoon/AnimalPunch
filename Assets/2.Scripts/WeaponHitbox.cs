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

    private bool isActive = false;
    private Collider myCol;

    private void Awake()
    {
        myCol = GetComponent<Collider>();
        if (myCol) myCol.isTrigger = true;

        // �ݶ��̴��� �׻� �ѵд�.
        // (���� Ÿ�ָ̹� ������ �ֱ�: isActive�� ����)
        if (myCol) myCol.enabled = true;
        isActive = false;
    }

    // �ܺο��� ���� Ÿ�ֿ̹� �Ѱ�/����
    public void SetActive(bool active)
    {
        isActive = active;
    }

    private void Reset()
    {
        // ��Ʈ�ڽ� �ݶ��̴��� �ݵ�� Trigger
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);  //  Enter �õ�
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);  //  �̹� ��ģ �� ����â�� ���� ��츦 ���� Stay������ �õ�
    }

    private void TryHit(Collider other)
    {
        if (!isActive) return;      //  ���� â�� �ƴ� �� ����
        if (!canDealDamage) return; // ��ٿ� ���̸� ����

        if (owner == OwnerType.Player)
        {
            // AI ã��(�����ϰ�)
            var ai = other.GetComponentInParent<AIController>()
                  ?? other.GetComponent<AIController>()
                  ?? other.GetComponentInChildren<AIController>();
            if (ai == null) return;

            if (ai.IsBlocking())
            {
                GameEvents.EvadeOccurred(false, ai.transform.position);
                ai.PlayBlockReaction();
                StartCoroutine(Cooldown());  // ��� ���� ��ٿ� ���� (��Ÿ ����)
                return;
            }

            ai.TakeHit(damage);
            StartCoroutine(Cooldown());
        }
        else // OwnerType.AI
        {
            var player = other.GetComponentInParent<PlayerController>()
                      ?? other.GetComponent<PlayerController>()
                      ?? other.GetComponentInChildren<PlayerController>();
            if (player == null) return;

            if (player.IsBlocking())
            {
                GameEvents.EvadeOccurred(true, player.transform.position);
                player.PlayBlockReaction();
                StartCoroutine(Cooldown());  // ��� ���� ��ٿ�
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
