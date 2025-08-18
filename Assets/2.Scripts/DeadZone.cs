using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DeadZone : MonoBehaviour
{
    [Tooltip("배틀 상태에서만 낙사 트리거를 작동시킬지 여부")]
    [SerializeField] private bool onlyDuringBattle = true;

    private bool IsBattleActive()
    {
        // GameManager 싱글톤이 살아 있고 현재 상태가 Battle 인지 확인
        return GameManager.Instance != null &&
               GameManager.Instance.currentState == GameManager.GameState.Battle;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 배틀 중이 아니면 낙사 무시
        if (onlyDuringBattle && !IsBattleActive())
            return;

        // 부모/자식 중에 CharacterBase 상속 객체 찾기
        var cb = other.GetComponentInParent<CharacterBase>();
        if (cb != null)
        {
            cb.Die();
        }
    }
}
