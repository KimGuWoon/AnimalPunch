using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GateHandler : MonoBehaviour
{
    // ����Ʈ ������ ����
    public GameObject gatePrefab;

    // �̺�Ʈ: AI�� �׾��� �� GameManager�� �ٸ� ������ �����
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
        Debug.Log("Gate ����!");
        Instantiate(gatePrefab, transform.position, Quaternion.identity);
    }

    // �ܺο��� AI ������ �˷��ִ� �Լ�
    public static void TriggerAIDeath()
    {
        OnAIDeath?.Invoke();
    }
}
