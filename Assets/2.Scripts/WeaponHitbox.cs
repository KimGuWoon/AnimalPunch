using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    public enum OwnerType { Player, AI }
    public OwnerType owner;
  

    public int damage = 1; // 기본 데미지
    private bool canDealDamage = true;

    private Transform playerAnchor;
    private Transform aiAnchor;

    private void OnEnable()
    {
        GameEvents.OnSetPlayerAnchor += SetPlayerAnchor;
        GameEvents.OnSetAIAnchor += SetAIAnchor;
    }

    private void OnDisable()
    {
        GameEvents.OnSetPlayerAnchor -= SetPlayerAnchor;
        GameEvents.OnSetAIAnchor -= SetAIAnchor;
    }

    private void SetPlayerAnchor(Transform anchor)
    {
        playerAnchor = anchor;
    }

    private void SetAIAnchor(Transform anchor)
    {
        aiAnchor = anchor;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;

        if (owner == OwnerType.Player && other.CompareTag("AI"))
        {
            AIController ai = other.GetComponent<AIController>();
            if (ai != null)
            {
                if (ai.IsBlocking())
                {
                    Debug.Log("AI가 회피(방어)에 성공함!");
                    if (aiAnchor != null)
                    {
                        GameEvents.EvadeOccurred(false, aiAnchor.position);
                    }
                }
                else
                {
                    ai.TakeHit(damage);
                }
                StartCoroutine(DisableTemporarily());
            }
        }
        else if (owner == OwnerType.AI && other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                if (player.IsBlocking())
                {
                    Debug.Log("플레이어가 회피(방어)에 성공함!");
                    if (playerAnchor != null)
                    {
                        GameEvents.EvadeOccurred(true, playerAnchor.position);
                    }
                }
                else
                {
                    player.TakeHit(damage);
                }
                StartCoroutine(DisableTemporarily());
            }
        }
    }



    private System.Collections.IEnumerator DisableTemporarily()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(1f); // 연속 타격 방지 시간
        canDealDamage = true;
    }

    
}
