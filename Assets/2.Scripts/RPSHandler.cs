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

    public Canvas canvas;

    private void Awake()
    {
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
        GameEvents.OnSetPlayerAnchor += SetPlayerAnchor;
        GameEvents.OnSetAIAnchor -= SetAIAnchor;
    }
    public void SetPlayerAnchor(Transform anchor) => playerHeadTransform = anchor;
    public void SetAIAnchor(Transform anchor) => aiHeadTransform = anchor;
    public Transform GetPlayerAnchor() => playerHeadTransform;
    public Transform GetAIAnchor() => aiHeadTransform;

    public void ShowRPS()
    {
        playerChoice = RPS.None;
        rpsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        timerCoroutine = StartCoroutine(CountdownTimer());
    }

    IEnumerator CountdownTimer()
    {
        float timeLeft = 5f;
        while (timeLeft > 0)
        {
            timerText.text = $"{timeLeft:F0}";
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        if (playerChoice == RPS.None)
            playerChoice = (RPS)Random.Range(1, 4);

        aiChoice = (RPS)Random.Range(1, 4);
        rpsPanel.SetActive(false);

        StartCoroutine(RPSShowAnimation(playerChoice, aiChoice));
    }

    public void OnSelectScissors()
    {
        ConfirmPlayerChoice(RPS.Scissors);
    }

    public void OnSelectRock()
    {
        ConfirmPlayerChoice(RPS.Rock);
    }

    public void OnSelectPaper()
    {
        ConfirmPlayerChoice(RPS.Paper);
    }

    IEnumerator RPSShowAnimation(RPS finalPlayer, RPS finalAI)
    {
       
        float duration = 1.5f;
        float elapsed = 0f;

        playerRPSImageUI.gameObject.SetActive(true);
        aiRPSImageUI.gameObject.SetActive(true);

        Sprite[] animSprites = new Sprite[] {
        null,            // 인덱스 0 (안 씀)
        scissorsSprite,  // 1: Scissors
        rockSprite,      // 2: Rock
        paperSprite      // 3: Paper
    };

        while (elapsed < duration)
        {
            int rand1 = Random.Range(1, 4); // 1~3
            int rand2 = Random.Range(1, 4);

            playerRPSImageUI.GetComponent<Image>().sprite = animSprites[rand1];
            aiRPSImageUI.GetComponent<Image>().sprite = animSprites[rand2];

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        playerRPSImageUI.GetComponent<Image>().sprite = GetSprite(finalPlayer);
        aiRPSImageUI.GetComponent<Image>().sprite = GetSprite(finalAI);

        yield return new WaitForSeconds(1.2f);

        playerRPSImageUI.gameObject.SetActive(false);
        aiRPSImageUI.gameObject.SetActive(false);

       
        GameEvents.RPSFinished(finalPlayer, finalAI);
    }

    private void ConfirmPlayerChoice(RPS choice)
    {
        if (playerChoice != RPS.None) return; // 이미 선택했으면 무시

        playerChoice = choice;

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);  // 타이머 중단

        aiChoice = (RPS)Random.Range(1, 4);

        rpsPanel.SetActive(false);

        StartCoroutine(RPSShowAnimation(playerChoice, aiChoice));  // 애니메이션 즉시 시작
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
