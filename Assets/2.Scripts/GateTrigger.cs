using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateTrigger : MonoBehaviour
{
    [Header("�ɼ�")]
    [Tooltip("true�� ���� ���� ���� ���� �ε����� �̵�")]
    [SerializeField] private bool useNextBuildIndex = true;

    [Tooltip("Ư�� ������ ���� �̵��ϰ� ���� ���� �Է�(���� ����)")]
    [SerializeField] private string overrideSceneName = "";

    [Tooltip("�ߺ� �ε� ����")]
    [SerializeField] private float reenterBlockSeconds = 0.5f;

    private bool isTransitioning = false;
    private float lastEnterTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isTransitioning) return;

        // ª�� �ð� �� �ߺ� ���� ����
        if (Time.time - lastEnterTime < reenterBlockSeconds) return;
        lastEnterTime = Time.time;

        isTransitioning = true;

        // 1) �������̵� �̸��� ������ �װ����� �̵�
        if (!string.IsNullOrWhiteSpace(overrideSceneName))
        {
            SceneManager.LoadScene(overrideSceneName);
            return;
        }

        // 2) �⺻: ���� �ε��� ���� ���� ��
        if (useNextBuildIndex)
        {
            int current = SceneManager.GetActiveScene().buildIndex;
            int next = current + 1;

            if (next < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(next);
            }
            else
            {
                Debug.LogWarning($"[GateTrigger] ���� ���� �ε����� �����ϴ�. ����={current}, next={next}");
                isTransitioning = false; // ���� �� ������ ���
            }
        }
        else
        {
            Debug.LogWarning("[GateTrigger] useNextBuildIndex=false�ε� overrideSceneName�� ��� ����.");
            isTransitioning = false;
        }
    }

}
