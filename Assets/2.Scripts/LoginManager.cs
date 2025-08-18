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
    // 로그인 관련 UI인 loginPanel(로그인패널)을 선언함
    // 그리고, 캐릭터 선택 패널인 selectPanel도 선언함
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

        // 엔터로 제출되게 단일라인 + onSubmit에 로그인 연결
        idInput.lineType = TMP_InputField.LineType.SingleLine;
        pwInput.lineType = TMP_InputField.LineType.SingleLine;

        idInput.onSubmit.AddListener(_ => ButtonLogin());
        pwInput.onSubmit.AddListener(_ => ButtonLogin());
    }

    private void Update()
    {
        // ── 로그인 패널에서 Tab 순환 ──
        if (loginPanel.activeSelf && Input.GetKeyDown(KeyCode.Tab))
        {
            if (idInput.isFocused) pwInput.Select();
            else if (pwInput.isFocused) idInput.Select();
        }

        // ── 로그인 패널에서 Enter = 로그인 ──
        if (loginPanel.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ButtonLogin();
        }

        // ── 셀렉트 패널에서 키보드 조작 ──
        if (selectPanel.activeSelf)
        {
            // 닉네임 입력 중엔 캐릭터 이동 키 잠금
            if (!nicknameInput.isFocused)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                    MoveCharacter(-1);
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                    MoveCharacter(+1);
            }            

            // Tab → 닉네임 입력 칸
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                nicknameInput.Select();
                nicknameInput.ActivateInputField();
            }

            // Enter → Start 버튼
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ButtonStart();
            }
        }
    }

    public void ButtonLogin()
    {
        if(UserData.userID == idInput.text && UserData.userPW == pwInput.text) // 03.28토글 색상 유지 추가
        {
            loginPanel.SetActive(false);
            selectPanel.SetActive(true);

            SetDefaultCharacterToZero();
        }

        //loginPanel.SetActive(false); //위로 옮김. 되돌릴 시 주석 제거
        //selectPanel.SetActive(true);
    }

    private void MoveCharacter(int delta)
    {
        if (selectToggles == null || selectToggles.Length == 0) return;

        // 현재 on인 인덱스를 동기화(혹시 마우스로 바꿨을 때 캐시 보정)
        for (int i = 0; i < selectToggles.Length; i++)
        {
            if (selectToggles[i].GetComponent<Toggle>().isOn)
            {
                currentSelectIdx = i;
                break;
            }
        }

        int next = (currentSelectIdx + delta + selectToggles.Length) % selectToggles.Length;

        // 토글 on → ToggleGroup이 하나만 켜지게 정리해줌
        selectToggles[next].GetComponent<Toggle>().isOn = true;
        currentSelectIdx = next;
    }

    private void SetDefaultCharacterToZero()
    {
        if (selectToggles == null || selectToggles.Length == 0) return;

        // CharacterIndex==0인 항목을 찾아 On
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
