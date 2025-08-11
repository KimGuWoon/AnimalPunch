using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RPSHandler : MonoBehaviour
{
    [Header("UI")]
    public GameObject rpsPanel;
    public TMP_Text timerText;
    public Button scissorsBtn, rockBtn, paperBtn;

    [Header("RPS Images")]
    public RectTransform playerRPSImageUI;
    public RectTransform aiRPSImageUI;
    public Sprite scissorsSprite, rockSprite, paperSprite;

    public enum RPS { None, Scissors = 1, Rock = 2, Paper = 3 }

    public RPS playerChoice = RPS.None;
    public RPS aiChoice = RPS.None;

    private Sprite[] rpsSprites;
    private Transform playerHeadTransform;
    private Transform aiHeadTransform;
    private Coroutine timerCoroutine;

    // 중복 실행 방지
    private Coroutine animCoroutine;
    private bool isAnimating = false;
    private bool isChoiceLocked = false;

    public Canvas canvas;

    private void Awake()
    {
        // 인덱스 1~3만 사용 (1:가위, 2:바위, 3:보)
        rpsSprites = new Sprite[] { null, scissorsSprite, rockSprite, paperSprite };

        rpsPanel.SetActive(false);
        playerRPSImageUI.gameObject.SetActive(false);
        aiRPSImageUI.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnShowRPS += ShowRPS;
        GameEvents.OnSetPlayerAnchor += SetPlayerAnchor;
        GameEvents.OnSetAIAnchor += SetAIAnchor;
    }

    private void OnDisable()
    {
        GameEvents.OnShowRPS -= ShowRPS;
        GameEvents.OnSetPlayerAnchor -= SetPlayerAnchor; // 중요: -=
        GameEvents.OnSetAIAnchor -= SetAIAnchor;
    }

    public void SetPlayerAnchor(Transform anchor) => playerHeadTransform = anchor;
    public void SetAIAnchor(Transform anchor) => aiHeadTransform = anchor;
    public Transform GetPlayerAnchor() => playerHeadTransform;
    public Transform GetAIAnchor() => aiHeadTransform;

    public void ShowRPS()
    {
        // 상태 초기화 + 중복 코루틴 정리
        isChoiceLocked = false;
        isAnimating = false;

        if (animCoroutine != null) { StopCoroutine(animCoroutine); animCoroutine = null; }
        if (timerCoroutine != null) { StopCoroutine(timerCoroutine); timerCoroutine = null; }

        playerChoice = RPS.None;

        rpsPanel.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(null);

        timerCoroutine = StartCoroutine(CountdownTimer());
    }

    IEnumerator CountdownTimer()
    {
        float timeLeft = 3f;
        while (timeLeft > 0f)
        {
            timerText.text = $"{timeLeft:F0}";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        if (playerChoice == RPS.None)
            playerChoice = (RPS)Random.Range(1, 4);

        aiChoice = (RPS)Random.Range(1, 4);
        isChoiceLocked = true;
        rpsPanel.SetActive(false);

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(RPSShowAnimation(playerChoice, aiChoice));
    }

    public void OnSelectScissors() { Debug.Log("[RPS] Button: Scissors"); ConfirmPlayerChoice(RPS.Scissors); }
    public void OnSelectRock() { Debug.Log("[RPS] Button: Rock"); ConfirmPlayerChoice(RPS.Rock); }
    public void OnSelectPaper() { Debug.Log("[RPS] Button: Paper"); ConfirmPlayerChoice(RPS.Paper); }

    private void ConfirmPlayerChoice(RPS choice)
    {
        if (isAnimating) return;
        playerChoice = choice;
    }

    IEnumerator RPSShowAnimation(RPS finalPlayer, RPS finalAI)
    {
        isAnimating = true;

        float duration = 1.5f;
        float elapsed = 0f;

        playerRPSImageUI.gameObject.SetActive(true);
        aiRPSImageUI.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            var r1 = (RPS)Random.Range(1, 4);
            var r2 = (RPS)Random.Range(1, 4);

            playerRPSImageUI.GetComponent<Image>().sprite = GetSprite(r1);
            aiRPSImageUI.GetComponent<Image>().sprite = GetSprite(r2);

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        playerRPSImageUI.GetComponent<Image>().sprite = GetSprite(finalPlayer);
        aiRPSImageUI.GetComponent<Image>().sprite = GetSprite(finalAI);

        Debug.Log($"[RPS] Result  Player={finalPlayer}  AI={finalAI}");

        yield return new WaitForSeconds(1.2f);

        playerRPSImageUI.gameObject.SetActive(false);
        aiRPSImageUI.gameObject.SetActive(false);

        isAnimating = false;
        animCoroutine = null;

        GameEvents.RPSFinished(finalPlayer, finalAI);
    }

    Sprite GetSprite(RPS choice)
    {
        switch (choice)
        {
            case RPS.Scissors: return scissorsSprite;
            case RPS.Rock: return rockSprite;
            case RPS.Paper: return paperSprite;
            default: return null;
        }
    }

    private void Update()
    {
        UpdateUIPosition(playerRPSImageUI, playerHeadTransform);
        UpdateUIPosition(aiRPSImageUI, aiHeadTransform);
    }

    void UpdateUIPosition(RectTransform ui, Transform target)
    {
        if (ui == null || target == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 0.3f);
        ui.position = screenPos;
    }
}