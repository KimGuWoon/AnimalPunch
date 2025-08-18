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

    [SerializeField] private string baseIdleStateName = "Idle"; // �� �� ��Ʈ�ѷ��� Idle ������Ʈ��
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
        //Debug.Log($"{gameObject.name} �ǰ�! ���� ü��: {currentHealth}");
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
        Debug.Log($"{gameObject.name} ���");
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
            animator.enabled = true;            //Ȥ�� ���� �ִ� �ִϸ����� ����
            animator.SetBool("isDead", false);
        }
        if (rb) rb.isKinematic = false;

        // idle ����
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

        // 1) ��� Ʈ����/�Ҹ��� �ʱ�ȭ
        for (int i = 0; i < animator.parameterCount; i++)
        {
            var p = animator.GetParameter(i);
            if (p.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(p.name);
            else if (p.type == AnimatorControllerParameterType.Bool)
                animator.SetBool(p.name, false);
        }

        // �ʼ� �÷��״� ���������
        animator.SetBool("isDead", false);
        animator.SetBool("isBlock", false);
        animator.SetBool("isWalk", false);

        // 2) �ӵ�/��Ʈ��� ����(�ʿ� ��)
        animator.speed = 1f;
        if (TryGetComponent<Animator>(out var anim)) anim.applyRootMotion = false;

        // 3) �����ε� �� 1������ ���� ����
        animator.Rebind();
        animator.Update(0f);

        // 4) Idle ������Ʈ�� ���� ��� (���̽� ���̾��, ó�� �����Ӻ���)
        if (!string.IsNullOrEmpty(baseIdleStateName))
        {
            animator.Play(baseIdleStateName, baseLayerIndex, 0f);
            animator.Update(0f);
        }
    }
}
