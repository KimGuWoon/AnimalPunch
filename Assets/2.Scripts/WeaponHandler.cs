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

    public static event Action OnWeaponSelected;

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

        btnHammer.SetActive(isPlayerWinner);
        btnBak.SetActive(!isPlayerWinner);
    }

    public void OnClick_Hammer()
    {
        if (!btnHammer.activeSelf)
        {
            Debug.Log("You chose the wrong weapon");
            GameEvents.ShowWarning(); 
            return;
        }
        
        
        Debug.Log("Hammer 선택");
        panel.SetActive(false);
        OnWeaponSelected?.Invoke(); // GameManager에 전달
              
    }

    public void OnClick_Bak()
    {
        if (!btnBak.activeSelf)
        {
            Debug.Log("You chose the wrong weapon");
            GameEvents.ShowWarning();
            return;
        }

        Debug.Log("Bak 선택");
        panel.SetActive(false);
        OnWeaponSelected?.Invoke(); // GameManager에 전달
    }
}
