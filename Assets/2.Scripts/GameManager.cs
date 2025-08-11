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
        GameEvents.OnRPSFinished += EvaluateRPSResult;
        GameEvents.OnWeaponSelected += OnWeaponSelected;
        GameEvents.OnStageIntroFinished += OnStageIntroFinished;
    }

    private void OnDisable()
    {      
        GameEvents.OnRPSFinished -= EvaluateRPSResult;
        GameEvents.OnWeaponSelected -= OnWeaponSelected;
        GameEvents.OnStageIntroFinished -= OnStageIntroFinished;
    }

    private void Start()
    {     
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
                    Debug.Log("가위바위보 선택 시작");
                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    GameEvents.TriggerShowRPS();
                    yield return new WaitUntil(() => currentState != GameState.RPS_Select);
                    break;

                case GameState.Weapon_Select:
                    Debug.Log("무기 선택 단계");
                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    GameEvents.TriggerShowWeaponPanel(UserData.isPlayerWinner);                     
                    yield return new WaitUntil(() => currentState == GameState.Battle);
                    break;

                case GameState.Battle:
                    Debug.Log("전투 시작");
                    SetPlayerMovement(PlayerController.MovementMode.Battle);

                    float t = 0f;
                    while (currentState == GameState.Battle && t < 7f)
                    {
                        t += Time.deltaTime;
                        yield return null;
                    }

                    // 누군가 죽어 RoundEnd로 바뀌었으면 더 진행하지 않음
                    if (currentState != GameState.Battle) break;

                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    currentState = GameState.RPS_Select;
                    break;

                case GameState.RoundEnd:
                    Debug.Log("라운드 종료 처리");
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
    private void OnStageIntroFinished()
    {
        allowGameStart = true; // Waiting → RPS_Select 넘어갈 준비 완료
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

    private void OnWeaponSelected(bool isHammer)
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        player?.EquipWeapon(isHammer);
       
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
            Debug.Log("비김! 다시 가위바위보");
            currentState = GameState.RPS_Select;
            StartCoroutine(RestartRPSAfterDelay());
        }
        else
        {
            Debug.Log(playerWin ? "플레이어 승!" : "AI 승!");
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
