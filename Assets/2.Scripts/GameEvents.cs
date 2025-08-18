using System;
using UnityEngine;

public static class GameEvents
{
    // �������� �ؽ�Ʈ(��Ʈ��) ���� ��ȣ 
    public static event System.Action OnStageIntroFinished;
    public static void TriggerStageIntroFinished()
    {
        OnStageIntroFinished?.Invoke();
    }

    // ���������� �Ϸ� �� �÷��̾�� AI ���� ��� ����
    public static event Action<RPSHandler.RPS, RPSHandler.RPS> OnRPSFinished;
    public static void RPSFinished(RPSHandler.RPS player, RPSHandler.RPS ai)
    {
        OnRPSFinished?.Invoke(player, ai);
    }

    // ���������� ���� ��û
    public static event Action OnShowRPS;
    public static void TriggerShowRPS()
    {
        OnShowRPS?.Invoke();
    }

    // ��Ʋ UI ����
    public static event System.Action<float> OnBattleTimerStart; // seconds
    public static event System.Action OnBattleTimerStop;
    public static event System.Action OnBattleTimerTimeUp;

    public static void InvokeBattleTimerStart(float seconds) => OnBattleTimerStart?.Invoke(seconds);
    public static void InvokeBattleTimerStop() => OnBattleTimerStop?.Invoke();
    public static void InvokeBattleTimerTimeUp() => OnBattleTimerTimeUp?.Invoke();

    // ���������� ��� ����
    public static event System.Action<bool> OnRPSResult;
    public static void RPSResult(bool playerWon) => OnRPSResult?.Invoke(playerWon);

    // ���� ���� �Ϸ� (true = Hammer ����)
    public static event Action<bool> OnWeaponSelected;
    public static void WeaponSelected(bool isHammer)
    {
        OnWeaponSelected?.Invoke(isHammer);
    }

    // �߸��� ���� ����
    public static Action TriggerWarning;

    public static void ShowWarning()
    {
        TriggerWarning?.Invoke();
    }

    // ���� ��������
    public static event Action<bool> OnShowWeaponPanel;
    public static void TriggerShowWeaponPanel(bool isPlayerWinner)
    {
        OnShowWeaponPanel?.Invoke(isPlayerWinner);
    }

    // Player HeadAnchor ��������
    public static event Action<Transform> OnSetPlayerAnchor;
    public static void SetPlayerAnchor(Transform anchor)
    {
        OnSetPlayerAnchor?.Invoke(anchor);
    }

    // AI HeadAnchor ��������
    public static event Action<Transform> OnSetAIAnchor;
    public static void SetAIAnchor(Transform anchor)
    {
        OnSetAIAnchor?.Invoke(anchor);
    }

    // ȸ�� �ؽ�Ʈ �̺�Ʈ
    public static event Action<bool, Vector3> OnEvadeOccurred;

    public static void EvadeOccurred(bool isPlayer, Vector3 worldPosition)
    {
        OnEvadeOccurred?.Invoke(isPlayer, worldPosition);
    }

    // ĳ���� ��� (true = �÷��̾�, false = AI)
    public static event Action<bool> OnCharacterDied;
    public static void CharacterDied(bool isPlayer)
    {
        OnCharacterDied?.Invoke(isPlayer);
    }

    // ���� ����
    public static event Action OnBattleEnded;
    public static void BattleEnded()
    {
        OnBattleEnded?.Invoke();
    }
}
