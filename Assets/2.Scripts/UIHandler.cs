using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHandler : MonoBehaviour
{
    [Header("Stage Text")]
    public TextMeshProUGUI stageText;
    public float fadeDuration = 1f;
    public float holdDuration = 2f;
 
    public EvadeTextUI evadeTextUI;
    public GameObject warningText;

    [Header("Battle Timer")]
    [SerializeField] private TextMeshProUGUI battleTimerText;
    [SerializeField] private bool blinkLast3Sec = true;

    private Coroutine battleTimerCo;

    private void OnEnable()
    {
        GameEvents.OnEvadeOccurred += ShowEvadeText;
        GameEvents.TriggerWarning += ShowWarningText;

        GameEvents.OnBattleTimerStart += StartBattleTimer;
        GameEvents.OnBattleTimerStop += StopBattleTimer;
    }

    private void OnDisable()
    {
        GameEvents.OnEvadeOccurred -= ShowEvadeText;
        GameEvents.TriggerWarning -= ShowWarningText;

        GameEvents.OnBattleTimerStart -= StartBattleTimer;
        GameEvents.OnBattleTimerStop -= StopBattleTimer;
    }

    void Start()
    {
        warningText.SetActive(false);

        if (battleTimerText != null)
        {
            battleTimerText.text = "07"; 
        }

        StartCoroutine(StageTextRoutine());
    }

    private void StartBattleTimer(float seconds)
    {
        if (battleTimerText == null) return;

        if (battleTimerCo != null) StopCoroutine(battleTimerCo);
        battleTimerCo = StartCoroutine(BattleTimerRoutine(seconds));
    }

    // [ADD] 전투 타이머 정지/숨김
    private void StopBattleTimer()
    {
        if (battleTimerCo != null) StopCoroutine(battleTimerCo);
        battleTimerCo = null;

        if (battleTimerText != null)
            battleTimerText.text = "07";
        var c = battleTimerText.color;
        c.a = 1f;
        battleTimerText.color = c;

        battleTimerText.gameObject.SetActive(true); // 필요하면 SetActive(false)로 바꿔도 됨
    }

    // 카운트다운 코루틴
    private IEnumerator BattleTimerRoutine(float seconds)
    {
        float remain = seconds;

        // 시작 시 보이기
        battleTimerText.gameObject.SetActive(true);

        while (remain > 0f)
        {
            int show = Mathf.CeilToInt(remain); 
            battleTimerText.text = show.ToString("D2");  // 07,06,05...          

            // 마지막 3초 깜빡임
            if (blinkLast3Sec && show <= 3)
            {
                float a = (Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f); // 0~1
                var c = battleTimerText.color;
                c.a = Mathf.Lerp(0.35f, 1f, a);
                battleTimerText.color = c;
            }
            else
            {
                var c = battleTimerText.color;
                c.a = 1f;
                battleTimerText.color = c;
            }

            remain -= Time.deltaTime;
            yield return null;
        }

        // 0초 처리
        battleTimerText.text = "00";
        

        // GameManager에 알림
        GameEvents.InvokeBattleTimerTimeUp();

        // 자동 숨김(원하면 유지해도 됨)
        yield return null;
        battleTimerText.text = "07";

        battleTimerCo = null;
    }
    private IEnumerator StageTextRoutine()
    {
        // 시작 시 알파 0
        Color color = stageText.color;
        color.a = 0;
        stageText.color = color;

        // Fade In
        float time = 0f;
        while (time < fadeDuration)
        {
            color.a = Mathf.Lerp(0, 1, time / fadeDuration);
            stageText.color = color;
            time += Time.deltaTime;
            yield return null;
        }
        color.a = 1;
        stageText.color = color;

        yield return new WaitForSeconds(holdDuration);

        time = 0f;
        while (time < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, time / fadeDuration);
            stageText.color = color;
            time += Time.deltaTime;
            yield return null;
        }
        color.a = 0;
        stageText.color = color;

        GameEvents.TriggerStageIntroFinished();
    }

    private void ShowEvadeText(bool isPlayer, Vector3 worldPos)
    {
        if (evadeTextUI == null)
        {
            Debug.LogWarning("EvadeTextUI가 연결되어 있지 않습니다.");
            return;
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos + Vector3.up * 0.1f);
        Color color = isPlayer ? Color.yellow : Color.red;
        evadeTextUI.Show(screenPos, color);
    }

    private void ShowWarningText()
    {
        if (warningText == null) return;

        warningText.SetActive(true);
        StartCoroutine(HideWarning());
    }

    IEnumerator HideWarning()
    {
        yield return new WaitForSeconds(1.5f);
        warningText.SetActive(false);
    }
}
