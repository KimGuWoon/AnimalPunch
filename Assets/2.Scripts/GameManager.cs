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

        // �� ���� ��� �̵� ��� (��Ʈ�� ����)
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
        // �� ���� ��� ��� ���·� �ʱ�ȭ�ϰ� �̵� ���
        currentState = GameState.Waiting;
        allowGameStart = false;

        // �÷��̾� ��Ʈ�ѷ��� �� ������ �ٲ���� ���� �����Ƿ� �ٽ� ã��
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
                    Debug.Log("���������� ���� ����");
                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    GameEvents.TriggerShowRPS();
                    yield return new WaitUntil(() => currentState != GameState.RPS_Select);
                    break;

                case GameState.Weapon_Select:
                    Debug.Log("���� ���� �ܰ�");
                    SetPlayerMovement(PlayerController.MovementMode.Locked);
                    GameEvents.TriggerShowWeaponPanel(UserData.isPlayerWinner);                     
                    yield return new WaitUntil(() => currentState == GameState.Battle);
                    break;

                case GameState.Battle:
                    Debug.Log("���� ����");
                    SetPlayerMovement(PlayerController.MovementMode.Battle);

                    yield return new WaitUntil(() => currentState != GameState.Battle);
                    break;

                case GameState.RoundEnd:
                    Debug.Log("���� ���� ó��");
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
        allowGameStart = true; // Waiting �� RPS_Select �Ѿ �غ� �Ϸ�
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

        // �÷��̾� ���� ����
        player?.EquipWeapon(isHammer);

        // �÷��̾ ���� �����߾ ��� ��Ʋ ����
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
    // ��Ʋ ��� ���� ����
    public void ForceEnterBattleNow()
    {
        if (currentState == GameState.Battle) return; // �ߺ� ȣ�� ����

        currentState = GameState.Battle;
        SetPlayerMovement(PlayerController.MovementMode.Battle); // �÷��̾� ��� ��Ʋ �̵� ���
        Debug.Log("[GM] ForceEnterBattleNow -> Battle ����");

        GameEvents.InvokeBattleTimerStart(7f);
    }

    // ���������� ����� ����
    IEnumerator RestartRPSAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        GameEvents.TriggerShowRPS();
    }

    // ���� ���� ó�� 
    private void HandleBattleTimeUp()
    {
        if (currentState != GameState.Battle) return; // ������ġ

        // Ÿ�̸Ӵ� UI �ʿ��� ����/������� ó��������, Ȥ�� �� �ߺ� ����:
        GameEvents.InvokeBattleTimerStop();

        SetPlayerMovement(PlayerController.MovementMode.Locked);
        currentState = GameState.RPS_Select;

        // RPS �г� �ٽ� ����
        GameEvents.TriggerShowRPS();
    }

    // ���� ����
    public void ShowGameOver()
    {

        GameEvents.InvokeBattleTimerStop();
        // ���� �� �̸��� ���� (�ױ� ����)
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("LastStage", currentScene);
        PlayerPrefs.Save();

        // Ȥ�� ���� �� Ÿ�ӽ������� �ǵ�ȴٸ� ����
        Time.timeScale = 1f;

        SceneManager.LoadScene("5.GameOver", LoadSceneMode.Single);
    }

    public void RestartToStage1()
    {
        // ����/���� �ʱ�ȭ
        currentState = GameState.Waiting;
        allowGameStart = false;

        // �÷��̾�� DDOL�̶� �������� �� ��� ����ִ� ���·� �ʱ�ȭ
        if (player == null) player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ReviveCommon();                          // �� ü��/����/�ִϸ�����/Idle���� ����
            SetPlayerMovement(PlayerController.MovementMode.Locked);
        }

        // AI�� ������ �ٽ� ������ �����̴� ���������� ����(����)
        if (ai != null && ai.gameObject != null)
        {
            Destroy(ai.gameObject);
            ai = null;
        }

        // ��������1�� �ε�
        UnityEngine.SceneManagement.SceneManager.LoadScene("2.STAGE1", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
