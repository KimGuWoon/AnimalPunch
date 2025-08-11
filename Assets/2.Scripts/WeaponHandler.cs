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

        // 버튼은 항상 둘 다 보이게
        btnHammer.SetActive(true);
        btnBak.SetActive(true);

        // 내부적으로만 제한(승자=해머, 패자=박)
        allowedForPlayer = isPlayerWinner ? AllowedWeapon.Hammer : AllowedWeapon.Bak;
    }

    public void OnClick_Hammer()
    {
        if (allowedForPlayer != AllowedWeapon.Hammer)
        {
            Debug.Log("You chose the wrong weapon");
            GameEvents.ShowWarning(); // UIHandler가 TriggerWarning 구독 중
            return;                   // 패널 유지
        }

        Debug.Log("Hammer 선택");
        panel.SetActive(false);
        
        GameEvents.WeaponSelected(true); // GameManager/Orchestrator에 알림
    }

    public void OnClick_Bak()
    {
        if (allowedForPlayer != AllowedWeapon.Bak)
        {
            Debug.Log("You chose the wrong weapon");
            GameEvents.ShowWarning();
            return;
        }

        Debug.Log("Bak 선택");
        panel.SetActive(false);
     
        GameEvents.WeaponSelected(false);
    }
}

