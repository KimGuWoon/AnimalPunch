using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    // �α��� ���� UI�� loginPanel(�α����г�)�� ������
    // �׸���, ĳ���� ���� �г��� selectPanel�� ������
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
        // Tab Ű�� ������ �� �Է� �ʵ� �� ��ȯ
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if(idInput.isFocused)
            {
                pwInput.Select(); // id���� pw�� �̵�
            }
            else if(pwInput.isFocused)
            {
                idInput.Select(); // pw����id���� �̵�(��ȯ)
            }
        }
    }

    public void ButtonLogin()
    {
        if(UserData.userID == idInput.text && UserData.userPW == pwInput.text) // 03.28��� ���� ���� �߰�
        {
            loginPanel.SetActive(false);
            selectPanel.SetActive(true);
        }

        //loginPanel.SetActive(false); //���� �ű�. �ǵ��� �� �ּ� ����
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
