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

        // 콜라이더는 항상 켜둔다.
        // (공격 타이밍만 데미지 주기: isActive로 제어)
        if (myCol) myCol.enabled = true;
        isActive = false;
    }

    // 외부에서 공격 타이밍에 켜고/끄기
    public void SetActive(bool active)
    {
        isActive = active;
    }

    private void Reset()
    {
        // 히트박스 콜라이더는 반드시 Trigger
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);  //  Enter 시도
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);  //  이미 겹친 뒤 공격창이 켜진 경우를 위해 Stay에서도 시도
    }

    private void TryHit(Collider other)
    {
        if (!isActive) return;      //  공격 창이 아닐 땐 무시
        if (!canDealDamage) return; // 쿨다운 중이면 무시

        if (owner == OwnerType.Player)
        {
            // AI 찾기(안전하게)
            var ai = other.GetComponentInParent<AIController>()
                  ?? other.GetComponent<AIController>()
                  ?? other.GetComponentInChildren<AIController>();
            if (ai == null) return;

            if (ai.IsBlocking())
            {
                GameEvents.EvadeOccurred(false, ai.transform.position);
                ai.PlayBlockReaction();
                StartCoroutine(Cooldown());  // 방어 때도 쿨다운 적용 (연타 방지)
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
                StartCoroutine(Cooldown());  // 방어 때도 쿨다운
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
