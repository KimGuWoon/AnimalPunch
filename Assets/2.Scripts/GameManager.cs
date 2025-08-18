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
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        GameEvents.OnBattleTimerTimeUp += HandleBattleTimeUp;
    }

    private void OnDisable()
    {      
        GameEvents.OnRPSFinished -= EvaluateRPSResult;
        GameEvents.OnWeaponSelected -= OnWeaponSelected;
        GameEvents.OnStageIntroFinished -= OnStageIntroFinished;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        GameEvents.OnBattleTimerTimeUp -= HandleBattleTimeUp;
    }

    private void Start()
    {     
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (ai == null)
            ai = FindObjectOfType<AIController>();

        allowGameStart = false;

        // 씬 시작 즉시 이동 잠금 (인트로 동안)
        SetPlayerMovement(PlayerController.MovementMode.Locked);

        StartCoroutine(GameFlow());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartToStage1();
        }
    }

    private void OnActiveSceneChanged(Scene prev, Scene next)
    {
        // 씬 시작 즉시 대기 상태로 초기화하고 이동 잠금
        currentState = GameState.Waiting;
        allowGameStart = false;

        // 플레이어 컨트롤러가 새 씬에서 바뀌었을 수도 있으므로 다시 찾기
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (ai == null) ai = FindObjectOfType<AIController>();

        SetPlayerMovement(PlayerController.MovementMode.Locked);
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

                    yield return new WaitUntil(() => currentState != GameState.Battle);
                    break;

                case GameState.RoundEnd:
                    Debug.Log("라운드 종료 처리");
                    GameEvents.InvokeBattleTimerStop();
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

    public void SetPlayerMovement(PlayerController.MovementMode mode)
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
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (ai == null) ai = FindObjectOfType<AIController>();

        // 플레이어 무기 장착
        player?.EquipWeapon(isHammer);

        // 플레이어가 먼저 선택했어도 즉시 배틀 진입
        ForceEnterBattleNow();
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
    // 배틀 모드 진입 조건
    public void ForceEnterBattleNow()
    {
        if (currentState == GameState.Battle) return; // 중복 호출 방지

        currentState = GameState.Battle;
        SetPlayerMovement(PlayerController.MovementMode.Battle); // 플레이어 즉시 배틀 이동 모드
        Debug.Log("[GM] ForceEnterBattleNow -> Battle 진입");

        GameEvents.InvokeBattleTimerStart(7f);
    }

    // 가위바위보 재시작 지연
    IEnumerator RestartRPSAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        GameEvents.TriggerShowRPS();
    }

    // 전투 종료 처리 
    private void HandleBattleTimeUp()
    {
        if (currentState != GameState.Battle) return; // 안전장치

        // 타이머는 UI 쪽에서 정지/숨김까지 처리하지만, 혹시 모를 중복 방지:
        GameEvents.InvokeBattleTimerStop();

        SetPlayerMovement(PlayerController.MovementMode.Locked);
        currentState = GameState.RPS_Select;

        // RPS 패널 다시 열기
        GameEvents.TriggerShowRPS();
    }

    // 게임 오버
    public void ShowGameOver()
    {

        GameEvents.InvokeBattleTimerStop();
        // 현재 씬 이름을 저장 (죽기 직전)
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastStage", currentScene);
        PlayerPrefs.Save();

        // 혹시 전투 중 타임스케일을 건드렸다면 원복
        Time.timeScale = 1f;

        SceneManager.LoadScene("5.GameOver", LoadSceneMode.Single);
    }

    public void RestartToStage1()
    {
        // 라운드/상태 초기화
        currentState = GameState.Waiting;
        allowGameStart = false;

        // 플레이어는 DDOL이라 남아있음 → 즉시 살아있는 상태로 초기화
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ReviveCommon();                          // ★ 체력/물리/애니메이터/Idle까지 복구
            SetPlayerMovement(PlayerController.MovementMode.Locked);
        }

        // AI는 씬에서 다시 스폰될 예정이니 남아있으면 제거(선택)
        if (ai != null && ai.gameObject != null)
        {
            Destroy(ai.gameObject);
            ai = null;
        }

        // 스테이지1로 로드
        UnityEngine.SceneManagement.SceneManager.LoadScene("2.STAGE1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
