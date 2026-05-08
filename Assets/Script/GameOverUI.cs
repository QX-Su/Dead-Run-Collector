using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public void OnRestartClicked()
    {
        gameObject.SetActive(false);
        if (GameManager.Instance != null)
            GameManager.Instance.StartRound();
    }

    public void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
