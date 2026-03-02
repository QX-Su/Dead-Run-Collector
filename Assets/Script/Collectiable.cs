using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectType { Coin, Diamond, Thunder, Poison }

    [Header("Type")]
    public CollectType type = CollectType.Coin;

    [Header("Score (only for Coin/Diamond)")]
    public int scoreValue = 5;

    [Header("Buff (only for Thunder/Poison)")]
    public float buffDuration = 8f; // 例如 8 秒

    [Header("Optional")]
    public bool rotate = true;
    public float rotateSpeed = 90f;

    void Update()
    {
        if (rotate)
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance == null || GameManager.Instance.IsEnded)
            return;

        // Coin / Diamond: 加分+计数
        if (type == CollectType.Coin)
        {
            GameManager.Instance.AddScore(scoreValue); // 设为 5
            GameManager.Instance.AddCoin(1);
        }
        else if (type == CollectType.Diamond)
        {
            GameManager.Instance.AddScore(scoreValue); // 设为 10
            GameManager.Instance.AddDiamond(1);
        }
        else
        {
            // Thunder / Poison: 上Buff（不加分也可以，你要加分也能在这里加）
            PlayerBuffs buffs = other.GetComponent<PlayerBuffs>();
            if (buffs == null)
            {
                // 玩家Tag对了但没有Buff脚本时的兜底：尝试从根节点找
                buffs = other.GetComponentInParent<PlayerBuffs>();
            }

            if (buffs != null)
            {
                if (type == CollectType.Thunder) buffs.ApplyThunder(buffDuration);
                else if (type == CollectType.Poison) buffs.ApplyPoison(buffDuration);
            }
        }

        Destroy(gameObject);
    }
}