using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button restartInGameButton;

    [Header("HUD")]
    [SerializeField] private Text movesText;
    [SerializeField] private Text levelText;

    private BoardManager boardManager;

    public void Initialize(BoardManager boardManager)
    {
        this.boardManager = boardManager;
        HideAllPanels();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnClickContinue);
        }

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnClickRetry);
        }

        if (restartInGameButton != null)
        {
            restartInGameButton.onClick.RemoveAllListeners();
            restartInGameButton.onClick.AddListener(OnClickRestartInGame);
        }
    }

    public void HideAllPanels()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        if (restartInGameButton != null)
            restartInGameButton.gameObject.SetActive(true);
    }

    public void ShowWin(bool canContinue)
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (losePanel != null)
            losePanel.SetActive(false);

        if (continueButton != null)
            continueButton.gameObject.SetActive(canContinue);

        if (restartInGameButton != null)
            restartInGameButton.gameObject.SetActive(false);
    }

    public void ShowLose()
    {
        if (losePanel != null)
            losePanel.SetActive(true);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (restartInGameButton != null)
            restartInGameButton.gameObject.SetActive(false);
    }

    public void UpdateMovesText(int remainingMoves)
    {
        if (movesText != null)
            movesText.text = "Moves: " + remainingMoves;
    }

    public void UpdateLevelText(string levelName)
    {
        if (levelText != null)
            levelText.text = levelName;
    }

    private void OnClickContinue()
    {
        if (boardManager != null)
            boardManager.LoadNextLevelFromUI();
    }

    private void OnClickRetry()
    {
        if (boardManager != null)
            boardManager.RestartCurrentLevelFromUI();
    }

    private void OnClickRestartInGame()
    {
        if (boardManager != null)
            boardManager.RestartCurrentLevelFromUI();
    }
}