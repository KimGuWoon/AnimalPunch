using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EvadeTextUI : MonoBehaviour
{
    public float floatUpDistance = 30f;     // ���� �ö� �Ÿ� (�ȼ� ����)
    public float duration = 1f;             // ���� �ð�

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI textMesh;

    private Vector3 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        textMesh = GetComponentInChildren<TextMeshProUGUI>();

        if (textMesh == null)
            Debug.LogError("TextMeshProUGUI ������Ʈ�� ã�� ���߽��ϴ�!", this);
    }

    public void Show(Vector3 screenPosition, Color color)
    {
        gameObject.SetActive(true);
        startPosition = screenPosition;
        rectTransform.position = screenPosition;
        canvasGroup.alpha = 1f;

        if (textMesh != null)
        {
            textMesh.text = "Miss!";
            textMesh.color = color; //�߰�: ���޹��� ���� ����
        }

        StartCoroutine(FloatUpAndFade());
    }

    System.Collections.IEnumerator FloatUpAndFade()
    {
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            rectTransform.position = startPosition + Vector3.up * (floatUpDistance * t);
            canvasGroup.alpha = 1f - t;

            time += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
