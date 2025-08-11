using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GateHandler : MonoBehaviour
{
    // ����Ʈ ������ ����
    public GameObject gatePrefab;

    [SerializeField] private Transform spawnPoint;

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
        if (gatePrefab == null)
        {
            Debug.LogWarning("GatePrefab�� ��� ����", this);
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Debug.Log($"Gate ���� @ {pos}");
        Instantiate(gatePrefab, pos, Quaternion.identity);
    }

    // �ܺο��� AI ������ �˷��ִ� �Լ�
    public static void TriggerAIDeath()
    {
        OnAIDeath?.Invoke();
    }
}
