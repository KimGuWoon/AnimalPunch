using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private int maxHealth;

    public void Setup(int max)
    {
        maxHealth = max;
        UpdateHealth(max); // ���� �� full��
    }

    public void UpdateHealth(int current)
    {
        if (fillImage == null || maxHealth <= 0) return;
        fillImage.fillAmount = (float)current / maxHealth;
    }
}
