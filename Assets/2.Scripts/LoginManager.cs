using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    // �α��� ���� UI�� loginPanel(�α����г�)�� ������
    // �׸���, ĳ���� ���� �г��� selectPanel�� ������
    public GameObject loginPanel, selectPanel;
    public TMP_InputField idInput, pwInput, nicknameInput;
    private SelectToggle[] selectToggles;
    private int currentSelectIdx = 0;

    private void Awake()
    {
        loginPanel.SetActive(true);
        selectPanel.SetActive(false);

        pwInput.contentType = TMP_InputField.ContentType.Password;
        pwInput.ForceLabelUpdate();

        selectToggles = selectPanel.GetComponentsInChildren<SelectToggle>(true)
                             .OrderBy(t => t.CharacterIndex)
                             .ToArray();

        // ���ͷ� ����ǰ� ���϶��� + onSubmit�� �α��� ����
        idInput.lineType = TMP_InputField.LineType.SingleLine;
        pwInput.lineType = TMP_InputField.LineType.SingleLine;

        idInput.onSubmit.AddListener(_ => ButtonLogin());
        pwInput.onSubmit.AddListener(_ => ButtonLogin());
    }

    private void Update()
    {
        // ���� �α��� �гο��� Tab ��ȯ ����
        if (loginPanel.activeSelf && Input.GetKeyDown(KeyCode.Tab))
        {
            if (idInput.isFocused) pwInput.Select();
            else if (pwInput.isFocused) idInput.Select();
        }

        // ���� �α��� �гο��� Enter = �α��� ����
        if (loginPanel.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ButtonLogin();
        }

        // ���� ����Ʈ �гο��� Ű���� ���� ����
        if (selectPanel.activeSelf)
        {
            // �г��� �Է� �߿� ĳ���� �̵� Ű ���
            if (!nicknameInput.isFocused)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                    MoveCharacter(-1);
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                    MoveCharacter(+1);
            }            

            // Tab �� �г��� �Է� ĭ
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                nicknameInput.Select();
                nicknameInput.ActivateInputField();
            }

            // Enter �� Start ��ư
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ButtonStart();
            }
        }
    }

    public void ButtonLogin()
    {
        if(UserData.userID == idInput.text && UserData.userPW == pwInput.text) // 03.28��� ���� ���� �߰�
        {
            loginPanel.SetActive(false);
            selectPanel.SetActive(true);

            SetDefaultCharacterToZero();
        }

        //loginPanel.SetActive(false); //���� �ű�. �ǵ��� �� �ּ� ����
        //selectPanel.SetActive(true);
    }

    private void MoveCharacter(int delta)
    {
        if (selectToggles == null || selectToggles.Length == 0) return;

        // ���� on�� �ε����� ����ȭ(Ȥ�� ���콺�� �ٲ��� �� ĳ�� ����)
        for (int i = 0; i < selectToggles.Length; i++)
        {
            if (selectToggles[i].GetComponent<Toggle>().isOn)
            {
                currentSelectIdx = i;
                break;
            }
        }

        int next = (currentSelectIdx + delta + selectToggles.Length) % selectToggles.Length;

        // ��� on �� ToggleGroup�� �ϳ��� ������ ��������
        selectToggles[next].GetComponent<Toggle>().isOn = true;
        currentSelectIdx = next;
    }

    private void SetDefaultCharacterToZero()
    {
        if (selectToggles == null || selectToggles.Length == 0) return;

        // CharacterIndex==0�� �׸��� ã�� On
        int idx = 0;
        for (int i = 0; i < selectToggles.Length; i++)
        {
            bool isZero = (selectToggles[i].CharacterIndex == 0);
            var t = selectToggles[i].GetComponent<Toggle>();
            t.isOn = isZero;
            if (isZero) idx = i;
        }
        currentSelectIdx = idx;
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
