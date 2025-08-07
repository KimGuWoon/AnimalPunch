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

    private void OnEnable()
    {
        GameEvents.OnEvadeOccurred += ShowEvadeText;
        GameEvents.TriggerWarning += ShowWarningText;
    }

    private void OnDisable()
    {
        GameEvents.OnEvadeOccurred -= ShowEvadeText;
        GameEvents.TriggerWarning -= ShowWarningText;
    }

    void Start()
    {
        warningText.SetActive(false);     
        StartCoroutine(StageTextRoutine());
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
        
        GameEvents.TriggerShowRPS();
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
