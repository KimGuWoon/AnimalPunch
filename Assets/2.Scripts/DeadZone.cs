using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DeadZone : MonoBehaviour
{
    [Tooltip("��Ʋ ���¿����� ���� Ʈ���Ÿ� �۵���ų�� ����")]
    [SerializeField] private bool onlyDuringBattle = true;

    private bool IsBattleActive()
    {
        // GameManager �̱����� ��� �ְ� ���� ���°� Battle ���� Ȯ��
        return GameManager.Instance != null &&
               GameManager.Instance.currentState == GameManager.GameState.Battle;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ��Ʋ ���� �ƴϸ� ���� ����
        if (onlyDuringBattle && !IsBattleActive())
            return;

        // �θ�/�ڽ� �߿� CharacterBase ��� ��ü ã��
        var cb = other.GetComponentInParent<CharacterBase>();
        if (cb != null)
        {
            cb.Die();
        }
    }
}
