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

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} 피격! 현재 체력: {currentHealth}");

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

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} 사망");
        animator.SetTrigger("Die");
        animator.SetBool("isDead", true);
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        DestroyWeapon();
        StartCoroutine(FreezeAfterDieAnimation(animator, 2.1f));
   
        GameManager.Instance.currentState = GameManager.GameState.RoundEnd;
    }

    protected IEnumerator FreezeAfterDieAnimation(Animator anim, float delay)
    {
        yield return new WaitForSeconds(delay);
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
}
