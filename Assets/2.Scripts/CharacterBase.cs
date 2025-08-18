using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    public int maxHealth = 50;
    protected int currentHealth;

    protected Animator animator;
    protected Rigidbody rb;
    protected bool isBlocking = false;
    protected GameObject currentWeapon;

    [SerializeField] private string baseIdleStateName = "Idle"; // ← 네 컨트롤러의 Idle 스테이트명
    [SerializeField] private int baseLayerIndex = 0;
    private Coroutine dieFreezeCo;
    public bool IsBlocking() => isBlocking;
    public bool IsDead => currentHealth <= 0;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
    }

    public virtual void TakeHit(int damage)
    {
        if (isBlocking || IsDead) return;

        int before = currentHealth;
        currentHealth -= damage;
        //Debug.Log($"{gameObject.name} 피격! 현재 체력: {currentHealth}");
        Debug.Log($"[HIT] {name} dmg={damage}, HP {before}->{currentHealth} (frame {Time.frameCount})");
        
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }       
    }

    protected abstract void UpdateHealthUI();

    public abstract void PlayAttackAnimation();

    public virtual void PlayBlockReaction()
    {
        if (animator != null) animator.SetTrigger("isBlock");
    }

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} 사망");
        animator.SetTrigger("Die");
        animator.SetBool("isDead", true);
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        DestroyWeapon();
        if (dieFreezeCo != null) StopCoroutine(dieFreezeCo);
        dieFreezeCo = StartCoroutine(FreezeAfterDieAnimation(animator, 2.1f));

        GameManager.Instance.currentState = GameManager.GameState.RoundEnd;
    }

    protected IEnumerator FreezeAfterDieAnimation(Animator anim, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (this == null || anim == null || !isActiveAndEnabled) yield break;
        anim.enabled = false;
    }

    public virtual void DestroyWeapon()
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon);
            currentWeapon = null;
        }
    }

    public void ResetHealthToFull()
    {
        if (IsDead) return;

        currentHealth = maxHealth;
        UpdateHealthUI();
        if (animator)
        {
            animator.enabled = true;            //혹시 꺼져 있던 애니메이터 복구
            animator.SetBool("isDead", false);
        }
        if (rb) rb.isKinematic = false;

        // idle 강제
        ResetAnimatorToIdle();
    }

    public virtual void ReviveCommon()
    {
        if (dieFreezeCo != null) { StopCoroutine(dieFreezeCo); dieFreezeCo = null; }
        StopAllCoroutines();
        currentHealth = maxHealth;
        isBlocking = false;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (animator != null)
        {
            animator.enabled = true;
            animator.ResetTrigger("Die");
            animator.SetBool("isDead", false);
            animator.SetBool("isBlock", false);
            animator.Rebind();
            animator.Update(0f);
        }

        DestroyWeapon();
        UpdateHealthUI();

        ResetAnimatorToIdle();
    }

    protected void ResetAnimatorToIdle()
    {
        if (animator == null) return;

        // 1) 모든 트리거/불리언 초기화
        for (int i = 0; i < animator.parameterCount; i++)
        {
            var p = animator.GetParameter(i);
            if (p.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(p.name);
            else if (p.type == AnimatorControllerParameterType.Bool)
                animator.SetBool(p.name, false);
        }

        // 필수 플래그는 명시적으로
        animator.SetBool("isDead", false);
        animator.SetBool("isBlock", false);
        animator.SetBool("isWalk", false);

        // 2) 속도/루트모션 정리(필요 시)
        animator.speed = 1f;
        if (TryGetComponent<Animator>(out var anim)) anim.applyRootMotion = false;

        // 3) 리바인드 후 1프레임 강제 갱신
        animator.Rebind();
        animator.Update(0f);

        // 4) Idle 스테이트로 강제 재생 (베이스 레이어에서, 처음 프레임부터)
        if (!string.IsNullOrEmpty(baseIdleStateName))
        {
            animator.Play(baseIdleStateName, baseLayerIndex, 0f);
            animator.Update(0f);
        }
    }
}
