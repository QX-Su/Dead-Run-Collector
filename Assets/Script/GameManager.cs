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

    [Header("End UI")]
    public GameObject endPanel;
    public TMP_Text endText;

    public bool IsEnded { get; private set; }

    float _timeLeft;

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
    }

    void Update()
    {
        if (IsEnded) return;

        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            EndRound();
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

    void EndRound()
    {
        IsEnded = true;

        if (endPanel) endPanel.SetActive(true);

        if (endText)
        {
            endText.text =
                $"Time Up!\n" +
                $"Final Score: {score}\n" +
                $"Coin: {coinCount}\n" +
                $"Diamond: {diamondCount}\n\n" +
                $"Press R to Restart";
        }
    }

    // 可选：键盘R重开（先加上，方便测试）
    void LateUpdate()
    {
        if (!IsEnded) return;

        // 你项目用 Input System，所以用 Keyboard.current
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartRound();
        }
    }
}