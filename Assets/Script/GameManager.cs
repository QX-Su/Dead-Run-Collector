using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer")]
    public float roundTime = 180f; // 3 minutes
    public TMP_Text timerText;

    [Header("Score")]
    public int score;
    public int coinCount;
    public int diamondCount;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text coinText;
    public TMP_Text diamondText;

    [Header("Start UI")]
    public GameObject startPanel;

    [Header("End UI")]
    public GameObject endPanel;
    public TMP_Text endText;

    [Header("Player")]
    public Transform player;
    public Vector3 playerSpawnPosition = new Vector3(0f, 4.72f, 0f);

    [Header("Fall Detection")]
    public float fallThreshold = -10f;

    public bool IsEnded { get; private set; }

    float _timeLeft;
    bool _gameStarted;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Show start screen, freeze game until player clicks Start
        IsEnded = true;
        _gameStarted = false;

        if (startPanel) startPanel.SetActive(true);
        if (endPanel) endPanel.SetActive(false);

        Time.timeScale = 0f;
    }

    // Called by the "Start Game" button on StartPanel
    public void OnStartGameClicked()
    {
        if (startPanel) startPanel.SetActive(false);
        Time.timeScale = 1f;
        _gameStarted = true;
        StartRound();
    }

    public void StartRound()
    {
        IsEnded = false;

        _timeLeft = roundTime;
        score = 0;
        coinCount = 0;
        diamondCount = 0;

        if (endPanel) endPanel.SetActive(false);
        RefreshUI();

        // Respawn player to original position
        RespawnPlayer();

        // Reset all enemies
        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
            enemy.ResetEnemy();
    }

    void RespawnPlayer()
    {
        if (player == null) return;

        // CharacterController must be disabled before teleporting
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = playerSpawnPosition;
        player.rotation = Quaternion.identity;

        if (cc != null) cc.enabled = true;
    }

    void Update()
    {
        if (IsEnded) return;

        // --- Countdown ---
        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            EndRound("Time Up!");
            return;
        }

        // --- Fall-off-map detection ---
        if (player != null && player.position.y < fallThreshold)
        {
            EndRound("FELL OFF THE MAP!");
            return;
        }

        RefreshUI();
    }

    // --- API for Collectible ---
    public void AddScore(int amount)
    {
        if (IsEnded) return;
        score += amount;
        RefreshUI();
    }

    public void AddCoin(int amount = 1)
    {
        if (IsEnded) return;
        coinCount += amount;
        RefreshUI();
    }

    public void AddDiamond(int amount = 1)
    {
        if (IsEnded) return;
        diamondCount += amount;
        RefreshUI();
    }

    void RefreshUI()
    {
        if (timerText) timerText.text = FormatTime(_timeLeft);
        if (scoreText) scoreText.text = $"Score: {score}";
        if (coinText) coinText.text = $"Coin: {coinCount}";
        if (diamondText) diamondText.text = $"Diamond: {diamondCount}";
    }

    string FormatTime(float t)
    {
        int sec = Mathf.CeilToInt(t);
        int m = sec / 60;
        int s = sec % 60;
        return $"{m:00}:{s:00}";
    }

    public void TriggerGameOver()
    {
        if (IsEnded) return;
        EndRound("CAUGHT BY ZOMBIE!");
    }

    void EndRound(string reason = "Time Up!")
    {
        IsEnded = true;

        if (endPanel) endPanel.SetActive(true);

        if (endText)
        {
            endText.text =
                $"{reason}\n" +
                $"Final Score: {score}\n" +
                $"Coin: {coinCount}\n" +
                $"Diamond: {diamondCount}\n\n" +
                $"Press R to Restart";
        }
    }

    void LateUpdate()
    {
        if (!IsEnded || !_gameStarted) return;

        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartRound();
        }
    }
}
