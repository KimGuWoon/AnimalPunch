using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : CharacterBase
{
    public enum MovementMode { Locked, Battle, Free }
    public MovementMode movementMode = MovementMode.Free;

    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float jumpPower = 80f;

    public GameObject hammerPrefab;
    public GameObject shieldPrefab;
    public Transform handTransform;
    public HealthBarUI healthBarUI;

    private bool isJumping = false;

    protected override void Awake()
    {
        base.Awake();
        GameObject bar = GameObject.Find("PlayerHealthBar");
        if (bar != null)
        {
            healthBarUI = bar.GetComponent<HealthBarUI>();
            healthBarUI.Setup(maxHealth);
        }
    }

    void Start()
    {
        Transform anchor = FindDeepChild(transform, "HeadAnchor");
        if (anchor != null)
        {
            GameEvents.SetPlayerAnchor(anchor);
        }

        Transform weaponPos = FindDeepChild(transform, "WeaponPos");
        if (weaponPos != null)
            handTransform = weaponPos;

        transform.rotation = Quaternion.Euler(0, 90f, 0);
    }

    void Update()
    {
        Jump();
        HandleInput();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 input = Vector3.zero;

        switch (movementMode)
        {
            case MovementMode.Locked:
                input = Vector3.zero;
                animator.SetBool("isWalk", false);
                break;
            case MovementMode.Battle:
                input = new Vector3(h, 0, 0);
                break;
            case MovementMode.Free:
                input = new Vector3(h, 0, v);
                break;
        }

        if (input.magnitude < 0.01f)
        {
            animator.SetBool("isWalk", false);
            return;
        }

        animator.SetBool("isWalk", true);

        if (movementMode != MovementMode.Locked && input != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(input);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
        }

        Vector3 move = input.normalized * moveSpeed * Time.deltaTime;
        rb.MovePosition(transform.position + move);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            animator.SetTrigger("isJump");
            isJumping = true;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isJumping = false;
    }

    void HandleInput()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Battle) return;

        if (Input.GetMouseButtonDown(0) && !isBlocking)
        {
            PlayAttackAnimation();
        }

        if (Input.GetMouseButtonDown(1) && !isBlocking)
        {
            StartCoroutine(BlockRoutine());
        }
    }

    IEnumerator BlockRoutine()
    {
        isBlocking = true;
        animator.SetBool("isBlock", true);

        yield return new WaitForSeconds(1f); // 1.5초 동안 방어 유지

        isBlocking = false;
        animator.SetBool("isBlock", false);
    }

    public void EquipWeapon(bool isHammer)
    {
        if (handTransform == null) return;

        DestroyWeapon();

        GameObject prefab = isHammer ? hammerPrefab : shieldPrefab;
        currentWeapon = Instantiate(prefab, handTransform.position, handTransform.rotation, handTransform);

        var hitbox = currentWeapon.GetComponentInChildren<WeaponHitbox>();
        if (hitbox != null)
            hitbox.owner = WeaponHitbox.OwnerType.Player;
    }

    protected override void UpdateHealthUI()
    {
        if (healthBarUI != null)
            healthBarUI.UpdateHealth(currentHealth);
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name) return t;
        }
        return null;
    }

    public override void PlayAttackAnimation()
    {
        int random = Random.Range(0, 2);
        if (random == 0)
            animator.SetTrigger("Attack1");
        else
            animator.SetTrigger("Attack2");
    }

    protected override void Die()
    {
        base.Die(); // 부모 Die() 호출

        GameManager.Instance.currentState = GameManager.GameState.RoundEnd;
        GameManager.Instance.ShowGameOver();
    }



    
}
