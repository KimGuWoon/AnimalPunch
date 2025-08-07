using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    // 로그인 관련 UI인 loginPanel(로그인패널)을 선언함
    // 그리고, 캐릭터 선택 패널인 selectPanel도 선언함
    public GameObject loginPanel, selectPanel;
    public TMP_InputField idInput, pwInput, nicknameInput;

    private void Awake()
    {
        loginPanel.SetActive(true);
        selectPanel.SetActive(false);

        pwInput.contentType = TMP_InputField.ContentType.Password;
        pwInput.ForceLabelUpdate();
    }

    private void Update()
    {
        // Tab 키를 눌렀을 때 입력 필드 간 순환
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if(idInput.isFocused)
            {
                pwInput.Select(); // id에서 pw로 이동
            }
            else if(pwInput.isFocused)
            {
                idInput.Select(); // pw에서id으로 이동(순환)
            }
        }
    }

    public void ButtonLogin()
    {
        if(UserData.userID == idInput.text && UserData.userPW == pwInput.text) // 03.28토글 색상 유지 추가
        {
            loginPanel.SetActive(false);
            selectPanel.SetActive(true);
        }

        //loginPanel.SetActive(false); //위로 옮김. 되돌릴 시 주석 제거
        //selectPanel.SetActive(true);
    }
    public void ButtonStart()
    {
        if(!string.IsNullOrEmpty(nicknameInput.text))
        {
            UserData.nickName = nicknameInput.text;
            SceneManager.LoadScene(1);
            print(UserData.nickName);
        }
    }
}
