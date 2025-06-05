using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    public static bool isLoadingSavedGame = false;  // New flag to track game state

    [SerializeField] private GameObject playMenuPanel;
    [SerializeField] private GameObject mainMenuPanel;  // Reference to main menu panel
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject highScoreMenuPanel;
    [SerializeField] private GameObject highScoreUIPrefab;
    [SerializeField] private Transform highScoreList;
    [SerializeField] private Transform savedGamesList; // Add reference for saved games list
    [SerializeField] private GameObject savedGameEntryPrefab; // New prefab reference

    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    [SerializeField] private InputField rowVal;
    [SerializeField] private InputField colVal;

    private int gridWidth, gridHeight;

    private GameSaveManager saveManager;

    public int GridWidth { get => gridWidth; set => gridWidth = value; }
    public int GridHeight { get => gridHeight; set => gridHeight = value; }

    [System.Serializable]
    private class HighScores
    {
        public List<ScoreEntry> entryList = new List<ScoreEntry>();
    }

    [System.Serializable]
    private class ScoreEntry
    {
        public string userName;
        public int score;

        public ScoreEntry(string userName, int score)
        {
            this.userName = userName;
            this.score = score;
        }
    }

    [System.Serializable]
    private class SavedGames
    {
        public List<SavedGameEntry> gameList = new List<SavedGameEntry>();
    }

    [System.Serializable]
    public class SavedGameEntry
    {
        public string saveId;
        public int movesCount;
        public int remainingCards;
        public int totalCards;
        public string date;
        public string gridSize;

        public SavedGameEntry(string id, int moves, int remaining, int total, string grid)
        {
            saveId = id;
            movesCount = moves;
            remainingCards = remaining;
            totalCards = total;
            gridSize = grid;
            date = DateTime.Now.ToString("MM/dd/yyyy HH:mm");
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If we already have an instance, copy the input fields references before destroying
            if (instance.rowVal == null) instance.rowVal = rowVal;
            if (instance.colVal == null) instance.colVal = colVal;
            if (instance.playMenuPanel == null) instance.playMenuPanel = playMenuPanel;
            if (instance.mainMenuPanel == null) instance.mainMenuPanel = mainMenuPanel;
            if (instance.continueButton == null) instance.continueButton = continueButton;
            if (instance.highScoreMenuPanel == null) instance.highScoreMenuPanel = highScoreMenuPanel;
            if (instance.highScoreUIPrefab == null) instance.highScoreUIPrefab = highScoreUIPrefab;
            if (instance.highScoreList == null) instance.highScoreList = highScoreList;
            if (instance.savedGamesList == null) instance.savedGamesList = savedGamesList;
            if (instance.savedGameEntryPrefab == null) instance.savedGameEntryPrefab = savedGameEntryPrefab;
            
            Destroy(gameObject);
            return;
        }
        
        saveManager = gameObject.AddComponent<GameSaveManager>();
        UpdateContinueButton();
    }

    private void OnEnable()
    {
        // If main menu panel exists and is active, update scores
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            UpdateScoreDisplay();
            UpdateSavedGamesDisplay();
        }
    }

    private void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.SetActive(GameSaveManager.HasSavedGame());
        }
    }

    public void PlayGame()
    {
        if (Int32.TryParse(colVal.text, out int valueX) && Int32.TryParse(rowVal.text, out int valueY))
        {
            if ((valueX * valueY) % 2 == 0)
            {
                isLoadingSavedGame = false;
                // Reset grid dimensions for new game
                GridWidth = valueX;
                GridHeight = valueY;
                Debug.Log($"Starting new game with grid size: {GridWidth}x{GridHeight}");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else 
            {
                Debug.LogWarning("Invalid grid dimensions - must be even number of cards");
                colVal.text = "";
                rowVal.text = "";
            }
        }
    }

    public void ContinueGame()
    {
        if (GameSaveManager.HasSavedGame())
        {
            var saveData = saveManager.LoadGame();
            if (saveData != null)
            {
                isLoadingSavedGame = true;  // This is a loaded game
                GridWidth = saveData.gridWidth;
                GridHeight = saveData.gridHeight;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    public void SaveCurrentGame()
    {
        var gameManager = FindObjectOfType<GameManager>();
        var timerController = FindObjectOfType<TimerController>();
        
        if (gameManager != null && timerController != null)
        {
            saveManager.SaveGame(gameManager, timerController);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            UpdateScoreDisplay();
            if (playMenuPanel != null)
            {
                playMenuPanel.SetActive(false);
            }
        }
    }

    private void UpdateScoreDisplay()
    {
        if (highScoreList != null)
        {
            // Clear existing entries
            foreach (Transform child in highScoreList)
            {
                Destroy(child.gameObject);
            }

            string json = PlayerPrefs.GetString("HighScores", JsonUtility.ToJson(new HighScores()));
            HighScores highScores = JsonUtility.FromJson<HighScores>(json);

            if (highScores != null && highScores.entryList != null && highScores.entryList.Count > 0)
            {
                foreach (var entry in highScores.entryList)
                {
                    GameObject scoreEntryObj = Instantiate(highScoreUIPrefab, highScoreList);
                    TextMeshProUGUI scoreText = scoreEntryObj.GetComponent<TextMeshProUGUI>();
                    if (scoreText != null)
                    {
                        scoreText.text = $"Player: {entry.userName} | Score: {entry.score}";
                    }
                }
            }
            else
            {
                // Show "No scores yet" message
                GameObject noScoresObj = Instantiate(highScoreUIPrefab, highScoreList);
                TextMeshProUGUI noScoresText = noScoresObj.GetComponent<TextMeshProUGUI>();
                if (noScoresText != null)
                {
                    noScoresText.text = "No scores yet!";
                }
            }
        }
    }

    public void AddNewScore(string userName, int score)
    {
        string json = PlayerPrefs.GetString("HighScores", JsonUtility.ToJson(new HighScores()));
        HighScores highScores = JsonUtility.FromJson<HighScores>(json);

        if (highScores == null)
        {
            highScores = new HighScores();
        }

        highScores.entryList.Add(new ScoreEntry(userName, score));

        // Keep only top 10 scores
        if (highScores.entryList.Count > 10)
        {
            highScores.entryList.Sort((a, b) => b.score.CompareTo(a.score)); // Sort by score descending
            highScores.entryList.RemoveAt(highScores.entryList.Count - 1);
        }

        PlayerPrefs.SetString("HighScores", JsonUtility.ToJson(highScores));
        PlayerPrefs.Save();

        // Update display immediately if we're on the main menu
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            UpdateScoreDisplay();
        }
    }

    // Helper method to clear scores (for testing)
    public void ClearScores()
    {
        PlayerPrefs.DeleteKey("HighScores");
        PlayerPrefs.Save();
        Debug.Log("Cleared all scores");
        UpdateScoreDisplay();
    }

    public void ShowHighScores()
    {
        UpdateScoreDisplay();
    }

    public void HideHighScores()
    {
        // This method might not be needed anymore
        // Keep it for compatibility if you still want the option to hide scores
        if (highScoreMenuPanel != null)
        {
            highScoreMenuPanel.SetActive(false);
        }
    }

    private void UpdateSavedGamesDisplay()
    {
        if (savedGamesList != null)
        {
            // Clear existing entries
            foreach (Transform child in savedGamesList)
            {
                Destroy(child.gameObject);
            }

            string json = PlayerPrefs.GetString("SavedGames", JsonUtility.ToJson(new SavedGames()));
            SavedGames savedGames = JsonUtility.FromJson<SavedGames>(json);

            if (savedGames != null && savedGames.gameList != null && savedGames.gameList.Count > 0)
            {
                for (int i = 0; i < savedGames.gameList.Count; i++)
                {
                    var game = savedGames.gameList[i];
                    GameObject saveEntryObj = Instantiate(savedGameEntryPrefab, savedGamesList);
                    
                    // Set the save info text
                    TextMeshProUGUI saveText = saveEntryObj.GetComponentInChildren<TextMeshProUGUI>();
                    if (saveText != null)
                    {
                        saveText.text = $"Grid: {game.gridSize} | Moves: {game.movesCount} | Cards: {game.remainingCards}/{game.totalCards} | {game.date}";
                    }

                    // Setup load button
                    int saveIndex = i; // Capture the index for the button callbacks
                    Button loadButton = saveEntryObj.transform.Find("LoadButton")?.GetComponent<Button>();
                    if (loadButton != null)
                    {
                        loadButton.onClick.AddListener(() => LoadSpecificSave(saveIndex));
                    }

                    // Setup delete button
                    Button deleteButton = saveEntryObj.transform.Find("DeleteButton")?.GetComponent<Button>();
                    if (deleteButton != null)
                    {
                        deleteButton.onClick.AddListener(() => DeleteSpecificSave(saveIndex));
                    }
                }
            }
            else
            {
                GameObject noSavesObj = Instantiate(savedGameEntryPrefab, savedGamesList);
                TextMeshProUGUI noSavesText = noSavesObj.GetComponentInChildren<TextMeshProUGUI>();
                if (noSavesText != null)
                {
                    noSavesText.text = "No saved games!";
                }
                
                // Disable buttons for "No saved games" entry
                Button[] buttons = noSavesObj.GetComponentsInChildren<Button>();
                foreach (var button in buttons)
                {
                    button.gameObject.SetActive(false);
                }
            }
        }
    }

    private void LoadSpecificSave(int saveIndex)
    {
        string json = PlayerPrefs.GetString("SavedGames", JsonUtility.ToJson(new SavedGames()));
        SavedGames savedGames = JsonUtility.FromJson<SavedGames>(json);

        if (savedGames != null && savedGames.gameList != null && saveIndex < savedGames.gameList.Count)
        {
            var savedGame = savedGames.gameList[saveIndex];
            
            // Set the current save ID in GameSaveManager
            GameSaveManager.SetCurrentSaveId(savedGame.saveId);
            isLoadingSavedGame = true;  // This is a loaded game
            
            // Parse grid size
            string[] gridDimensions = savedGame.gridSize.Split('x');
            if (gridDimensions.Length == 2)
            {
                GridWidth = int.Parse(gridDimensions[0]);
                GridHeight = int.Parse(gridDimensions[1]);
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void DeleteSpecificSave(int saveIndex)
    {
        string json = PlayerPrefs.GetString("SavedGames", JsonUtility.ToJson(new SavedGames()));
        SavedGames savedGames = JsonUtility.FromJson<SavedGames>(json);

        if (savedGames != null && savedGames.gameList != null && saveIndex < savedGames.gameList.Count)
        {
            var savedGame = savedGames.gameList[saveIndex];
            saveManager.DeleteSaveGame(savedGame.saveId);
            savedGames.gameList.RemoveAt(saveIndex);
            PlayerPrefs.SetString("SavedGames", JsonUtility.ToJson(savedGames));
            PlayerPrefs.Save();
            
            // Refresh the display
            UpdateSavedGamesDisplay();
        }
    }

    public void AddSavedGame(int moves, int remainingCards, int totalCards)
    {
        string json = PlayerPrefs.GetString("SavedGames", JsonUtility.ToJson(new SavedGames()));
        SavedGames savedGames = JsonUtility.FromJson<SavedGames>(json);

        if (savedGames == null)
        {
            savedGames = new SavedGames();
        }

        string gridSize = $"{GridWidth}x{GridHeight}";
        string saveId = DateTime.Now.ToString("yyyyMMddHHmmss");
        savedGames.gameList.Add(new SavedGameEntry(saveId, moves, remainingCards, totalCards, gridSize));

        // Keep only last 10 saved games
        if (savedGames.gameList.Count > 10)
        {
            var oldestSave = savedGames.gameList[0];
            saveManager.DeleteSaveGame(oldestSave.saveId);
            savedGames.gameList.RemoveAt(0); // Remove oldest save
        }

        PlayerPrefs.SetString("SavedGames", JsonUtility.ToJson(savedGames));
        PlayerPrefs.Save();

        // Update display immediately if we're on the main menu
        if (mainMenuPanel != null && mainMenuPanel.activeSelf)
        {
            UpdateSavedGamesDisplay();
        }
    }

    public void ReturnToMenu()
    {
        // Reset the singleton when returning to menu
        if (instance == this)
        {
            instance = null;
            Destroy(gameObject);
        }
        SceneManager.LoadScene(0); // Assuming menu is scene 0
    }
}
