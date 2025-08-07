using System;
using UnityEngine;

public static class GameEvents
{
    // 가위바위보 완료 → 플레이어와 AI 선택 결과 전달
    public static event Action<RPSHandler.RPS, RPSHandler.RPS> OnRPSFinished;
    public static void RPSFinished(RPSHandler.RPS player, RPSHandler.RPS ai)
    {
        OnRPSFinished?.Invoke(player, ai);
    }

    // 가위바위보 시작 요청
    public static event Action OnShowRPS;
    public static void TriggerShowRPS()
    {
        OnShowRPS?.Invoke();
    }

    // 무기 선택 완료 (true = Hammer 선택)
    public static event Action<bool> OnWeaponSelected;
    public static void WeaponSelected(bool isHammer)
    {
        OnWeaponSelected?.Invoke(isHammer);
    }

    // 잘못된 무기 선택
    public static Action TriggerWarning;

    public static void ShowWarning()
    {
        TriggerWarning?.Invoke();
    }

    // 무기 가져오기
    public static event Action<bool> OnShowWeaponPanel;
    public static void TriggerShowWeaponPanel(bool isPlayerWinner)
    {
        OnShowWeaponPanel?.Invoke(isPlayerWinner);
    }

    // Player HeadAnchor 가져오기
    public static event Action<Transform> OnSetPlayerAnchor;
    public static void SetPlayerAnchor(Transform anchor)
    {
        OnSetPlayerAnchor?.Invoke(anchor);
    }

    // AI HeadAnchor 가져오기
    public static event Action<Transform> OnSetAIAnchor;
    public static void SetAIAnchor(Transform anchor)
    {
        OnSetAIAnchor?.Invoke(anchor);
    }

    // 회피 텍스트 이벤트
    public static event Action<bool, Vector3> OnEvadeOccurred;

    public static void EvadeOccurred(bool isPlayer, Vector3 worldPosition)
    {
        OnEvadeOccurred?.Invoke(isPlayer, worldPosition);
    }

    // 캐릭터 사망 (true = 플레이어, false = AI)
    public static event Action<bool> OnCharacterDied;
    public static void CharacterDied(bool isPlayer)
    {
        OnCharacterDied?.Invoke(isPlayer);
    }

    // 전투 종료
    public static event Action OnBattleEnded;
    public static void BattleEnded()
    {
        OnBattleEnded?.Invoke();
    }
}
