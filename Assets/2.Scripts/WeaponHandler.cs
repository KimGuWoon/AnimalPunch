using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHandler : MonoBehaviour
{
    public GameObject panel;
    public GameObject btnHammer;
    public GameObject btnBak;   

    private enum AllowedWeapon { Hammer, Bak }
    private AllowedWeapon allowedForPlayer;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnShowWeaponPanel += ShowWeaponPanel;
    }

    private void OnDisable()
    {
        GameEvents.OnShowWeaponPanel -= ShowWeaponPanel;
    }

    public void ShowWeaponPanel(bool isPlayerWinner)
    {
        panel.SetActive(true);

        // ��ư�� �׻� �� �� ���̰�
        btnHammer.SetActive(true);
        btnBak.SetActive(true);

        // ���������θ� ����(����=�ظ�, ����=��)
        allowedForPlayer = isPlayerWinner ? AllowedWeapon.Hammer : AllowedWeapon.Bak;
    }

    public void OnClick_Hammer()
    {
        if (allowedForPlayer != AllowedWeapon.Hammer)
        {
            Debug.Log("You chose the wrong weapon");
            GameEvents.ShowWarning(); // UIHandler�� TriggerWarning ���� ��
            return;                   // �г� ����
        }

        Debug.Log("Hammer ����");
        panel.SetActive(false);
        
        GameEvents.WeaponSelected(true); // GameManager/Orchestrator�� �˸�
    }

    public void OnClick_Bak()
    {
        if (allowedForPlayer != AllowedWeapon.Bak)
        {
            Debug.Log("You chose the wrong weapon");
            GameEvents.ShowWarning();
            return;
        }

        Debug.Log("Bak ����");
        panel.SetActive(false);
     
        GameEvents.WeaponSelected(false);
    }
}

