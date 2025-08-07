    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class GameManager : MonoBehaviour
    {
    public static GameManager Instance;



    public PlayerController player;
    public AIController ai;
    public bool allowGameStart = false;

    public GameState currentState = GameState.Waiting;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public enum GameState
    {
        Waiting,
        RPS_Select,
        RPS_Result,
        Weapon_Select,
        Weapon_Result,
        Battle,
        RoundEnd
    }

    private void OnEnable()
    {
        WeaponHandler.OnWeaponSelected += OnPlayerWeaponSelected;        
        GameEvents.OnRPSFinished += EvaluateRPSResult;
    }

    private void OnDisable()
    {
        WeaponHandler.OnWeaponSelected -= OnPlayerWeaponSelected;
        GameEvents.OnRPSFinished -= EvaluateRPSResult;
    }

    private void Start()
    {
        allowGameStart = true;

        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (ai == null)
            ai = FindObjectOfType<AIController>();

        StartCoroutine(GameFlow());
    }

    IEnumerator GameFlow()
    {
        while (true)
        {
            switch (currentState)
            {
                case GameState.Waiting:
                    yield return new WaitUntil(() => allowGameStart);
                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    currentState = GameState.RPS_Select;
                    break;

                case GameState.RPS_Select:
                    Debug.Log("���������� ���� ����");
                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    yield return new WaitUntil(() => currentState != GameState.RPS_Select);
                    break;

                case GameState.Weapon_Select:
                    Debug.Log("���� ���� �ܰ�");
                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    GameEvents.TriggerShowWeaponPanel(UserData.isPlayerWinner); 
                    ai.SelectWeaponBasedOnResult(!UserData.isPlayerWinner);
                    yield return new WaitUntil(() => currentState == GameState.Battle);
                    break;

                case GameState.Battle:
                    Debug.Log("���� ����");
                    SetPlayerMovement(PlayerController.MovementMode.Battle);
                    yield return new WaitForSeconds(7f);
                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    currentState = GameState.RPS_Select;
                    break;

                case GameState.RoundEnd:
                    Debug.Log("���� ���� ó��");
                    ai?.DestroyWeapon();
                    var playerObj = GameObject.FindGameObjectWithTag("Player");
                    playerObj?.GetComponent<PlayerController>()?.DestroyWeapon();
                    yield return new WaitForSeconds(0.5f);
                    SetPlayerMovement(PlayerController.MovementMode.Free);
                    currentState = GameState.Waiting;
                    allowGameStart = false;
                    break;
            }
            yield return null;
        }
    }

    void SetPlayerMovement(PlayerController.MovementMode mode)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController controller = playerObj.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.movementMode = mode;
            }
        }
    }

    public void OnPlayerWeaponSelected()
    {
        currentState = GameState.Battle;
    }

    public void EvaluateRPSResult(RPSHandler.RPS player, RPSHandler.RPS ai)
    {
        bool playerWin =
            (player == RPSHandler.RPS.Scissors && ai == RPSHandler.RPS.Paper) ||
            (player == RPSHandler.RPS.Rock && ai == RPSHandler.RPS.Scissors) ||
            (player == RPSHandler.RPS.Paper && ai == RPSHandler.RPS.Rock);

        if (player == ai)
        {
            Debug.Log("���! �ٽ� ����������");
            currentState = GameState.RPS_Select;
            StartCoroutine(RestartRPSAfterDelay());
        }
        else
        {
            Debug.Log(playerWin ? "�÷��̾� ��!" : "AI ��!");
            UserData.isPlayerWinner = playerWin;
            currentState = GameState.Weapon_Select;
        }
    }

    IEnumerator RestartRPSAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        GameEvents.TriggerShowRPS();
    }

    public void ShowGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
}
