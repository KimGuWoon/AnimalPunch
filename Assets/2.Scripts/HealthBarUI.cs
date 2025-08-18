using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{

    [SerializeField] private Image fillImage;   // (����) ���
    [SerializeField] private Image lossImage;   // (�߰�) ����(�׻� ����)
    private int maxHealth;
    public void Setup(int max)
    {
        maxHealth = Mathf.Max(1, max);

        // ������ ��(����), ����� ��(����)�� ����
        if (lossImage != null) lossImage.transform.SetAsFirstSibling();
        if (fillImage != null) fillImage.transform.SetAsLastSibling();

        // ������ �׻� ����
        if (lossImage != null)
        {
            lossImage.enabled = true;
            lossImage.fillAmount = 1f;
        }

        UpdateHealth(maxHealth); // ������ Ǯ ü��
    }

    public void UpdateHealth(int current)
    {
        if (fillImage == null || maxHealth <= 0) return;
        current = Mathf.Clamp(current, 0, maxHealth);
        fillImage.fillAmount = (float)current / maxHealth;
    }
}


