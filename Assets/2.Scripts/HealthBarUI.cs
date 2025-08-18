using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{

    [SerializeField] private Image fillImage;   // (기존) 노랑
    [SerializeField] private Image lossImage;   // (추가) 빨강(항상 가득)
    private int maxHealth;
    public void Setup(int max)
    {
        maxHealth = Mathf.Max(1, max);

        // 빨강이 뒤(먼저), 노랑이 위(나중)로 고정
        if (lossImage != null) lossImage.transform.SetAsFirstSibling();
        if (fillImage != null) fillImage.transform.SetAsLastSibling();

        // 빨강은 항상 가득
        if (lossImage != null)
        {
            lossImage.enabled = true;
            lossImage.fillAmount = 1f;
        }

        UpdateHealth(maxHealth); // 시작은 풀 체력
    }

    public void UpdateHealth(int current)
    {
        if (fillImage == null || maxHealth <= 0) return;
        current = Mathf.Clamp(current, 0, maxHealth);
        fillImage.fillAmount = (float)current / maxHealth;
    }
}


