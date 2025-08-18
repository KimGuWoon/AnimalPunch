using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GateTrigger : MonoBehaviour
{
    [Header("옵션")]
    [Tooltip("true면 현재 씬의 다음 빌드 인덱스로 이동")]
    [SerializeField] private bool useNextBuildIndex = true;

    [Tooltip("특정 씬으로 강제 이동하고 싶을 때만 입력(비우면 무시)")]
    [SerializeField] private string overrideSceneName = "";

    [Tooltip("중복 로드 방지")]
    [SerializeField] private float reenterBlockSeconds = 0.5f;

    private bool isTransitioning = false;
    private float lastEnterTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isTransitioning) return;

        // 짧은 시간 내 중복 진입 방지
        if (Time.time - lastEnterTime < reenterBlockSeconds) return;
        lastEnterTime = Time.time;

        isTransitioning = true;

        // 1) 오버라이드 이름이 있으면 그것으로 이동
        if (!string.IsNullOrWhiteSpace(overrideSceneName))
        {
            SceneManager.LoadScene(overrideSceneName);
            return;
        }

        // 2) 기본: 빌드 인덱스 기준 다음 씬
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
                Debug.LogWarning($"[GateTrigger] 다음 빌드 인덱스가 없습니다. 현재={current}, next={next}");
                isTransitioning = false; // 실패 시 재진입 허용
            }
        }
        else
        {
            Debug.LogWarning("[GateTrigger] useNextBuildIndex=false인데 overrideSceneName도 비어 있음.");
            isTransitioning = false;
        }
    }

}
