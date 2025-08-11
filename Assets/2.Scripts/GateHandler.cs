using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GateHandler : MonoBehaviour
{
    // 게이트 프리팹 연결
    public GameObject gatePrefab;

    [SerializeField] private Transform spawnPoint;

    // 이벤트: AI가 죽었을 때 GameManager나 다른 곳에서 연결됨
    public static event Action OnAIDeath;

    private void OnEnable()
    {
        OnAIDeath += SpawnGate;
    }

    private void OnDisable()
    {
        OnAIDeath -= SpawnGate;
    }



    public void SpawnGate()
    {
        if (gatePrefab == null)
        {
            Debug.LogWarning("GatePrefab이 비어 있음", this);
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Debug.Log($"Gate 생성 @ {pos}");
        Instantiate(gatePrefab, pos, Quaternion.identity);
    }

    // 외부에서 AI 죽음을 알려주는 함수
    public static void TriggerAIDeath()
    {
        OnAIDeath?.Invoke();
    }
}
