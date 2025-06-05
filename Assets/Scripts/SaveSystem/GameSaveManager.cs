using UnityEngine;
using System.IO;

public class GameSaveManager : MonoBehaviour
{
    private static string SaveDirectory => Path.Combine(Application.persistentDataPath, "saves");
    private static string CurrentSaveId = "";

    public static bool HasSavedGame()
    {
        return Directory.Exists(SaveDirectory) && Directory.GetFiles(SaveDirectory, "*.json").Length > 0;
    }

    public static void SetCurrentSaveId(string saveId)
    {
        CurrentSaveId = saveId;
        Debug.Log($"Set current save ID to: {saveId}");
    }

    private string GetSaveFilePath(string saveId = "")
    {
        if (string.IsNullOrEmpty(saveId))
        {
            saveId = CurrentSaveId;
        }
        
        // Create saves directory if it doesn't exist
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }
        
        return Path.Combine(SaveDirectory, $"gamesave_{saveId}.json");
    }

    public void SaveGame(GameManager gameManager, TimerController timerController)
    {
        var saveData = new GameSaveData();
        
        // Save basic game state
        saveData.movesCount = int.Parse(gameManager.uiController.countText.text);
        saveData.elapsedTime = timerController.GetCurrentTime();
        saveData.remainingCards = gameManager.cardCount;
        
        Debug.Log($"Saving game state - Moves: {saveData.movesCount}, Time: {saveData.elapsedTime}, Cards: {saveData.remainingCards}");
        
        // Save grid configuration
        saveData.gridWidth = MenuController.instance.GridWidth;
        saveData.gridHeight = MenuController.instance.GridHeight;
        
        // Save card states
        var cards = GameObject.FindGameObjectsWithTag("Card");
        Debug.Log($"Found {cards.Length} cards to save");
        saveData.cardStates = new GameSaveData.CardState[cards.Length];
        
        for (int i = 0; i < cards.Length; i++)
        {
            var cardController = cards[i].GetComponent<CardController>();
            if (cardController != null)
            {
                saveData.cardStates[i] = new GameSaveData.CardState(
                    cardController.IsMatched(),
                    cardController.IsFaceUp(),
                    i,
                    cardController.cardType.cardName,
                    cardController.cardType.pairName,
                    cardController.GetGridX(),
                    cardController.GetGridY(),
                    cards[i].activeSelf
                );
                Debug.Log($"Saved card {i}: ID={i}, Matched={cardController.IsMatched()}, Pos=({cardController.GetGridX()},{cardController.GetGridY()})");
            }
        }
        
        // Save to JSON file with unique ID
        try
        {
            string saveId = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            SetCurrentSaveId(saveId);
            string jsonData = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(GetSaveFilePath(), jsonData);
            Debug.Log($"Game saved successfully to {GetSaveFilePath()}!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public GameSaveData LoadGame()
    {
        try
        {
            string savePath = GetSaveFilePath();
            Debug.Log($"Attempting to load save file: {savePath}");
            
            if (File.Exists(savePath))
            {
                string jsonData = File.ReadAllText(savePath);
                var saveData = JsonUtility.FromJson<GameSaveData>(jsonData);
                
                Debug.Log($"Loading game state - Moves: {saveData.movesCount}, Time: {saveData.elapsedTime}, Cards: {saveData.remainingCards}");
                Debug.Log($"Grid dimensions: {saveData.gridWidth}x{saveData.gridHeight}");
                Debug.Log($"Found {saveData.cardStates?.Length ?? 0} saved card states");
                
                return saveData;
            }
            Debug.LogWarning($"Save file not found at: {savePath}");
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return null;
        }
    }

    public void DeleteSaveGame(string saveId)
    {
        string savePath = GetSaveFilePath(saveId);
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"Deleted save file: {savePath}");
        }
    }
} 