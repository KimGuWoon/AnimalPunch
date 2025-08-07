using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GateHandler : MonoBehaviour
{
    // 게이트 프리팹 연결
    public GameObject gatePrefab;

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
        Debug.Log("Gate 생성!");
        Instantiate(gatePrefab, transform.position, Quaternion.identity);
    }

    // 외부에서 AI 죽음을 알려주는 함수
    public static void TriggerAIDeath()
    {
        OnAIDeath?.Invoke();
    }
}
