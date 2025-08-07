using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageTextFader : MonoBehaviour
{
    public TextMeshProUGUI stageText;
    public float fadeDuration = 1f;
    public float holdDuration = 2f;

    public GameObject rpsPanel; // 가위바위보 패널 직접 연결

    void Start()
    {
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        // 시작: 알파 0
        Color color = stageText.color;
        color.a = 0;
        stageText.color = color;

        // Fade In
        float time = 0;
        while (time < fadeDuration)
        {
            color.a = Mathf.Lerp(0, 1, time / fadeDuration);
            stageText.color = color;
            time += Time.deltaTime;
            yield return null;
        }
        color.a = 1;
        stageText.color = color;

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade Out
        time = 0;
        while (time < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, time / fadeDuration);
            stageText.color = color;
            time += Time.deltaTime;
            yield return null;
        }
        color.a = 0;
        stageText.color = color;

        yield return null;
        GameManager.Instance.allowGameStart = true;

    }
}
