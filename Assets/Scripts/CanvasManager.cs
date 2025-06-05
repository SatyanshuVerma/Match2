using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CanvasManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public CardCollectionSO cardCollection;
    private List<CardController> cardControllers;
    private CardGridGenerator cardGridGenerator;
    
    // Track initial card configuration
    private List<GameSaveData.InitialCardData> initialCardConfiguration;

    private void Awake()
    {
        cardControllers = new List<CardController>();
        initialCardConfiguration = new List<GameSaveData.InitialCardData>();
        
        // Set up the grid layout first
        SetCardConfig();
        
        // Initialize the grid generator with correct dimensions
        cardGridGenerator = new CardGridGenerator(cardCollection);

        // Handle game state
        if (MenuController.isLoadingSavedGame)
        {
            StartCoroutine(LoadSavedGameBoard());
        }
        else
        {
            GenerateCards();
            SetGameManagerCardCount();
        }
    }

    private IEnumerator LoadSavedGameBoard()
    {
        // Wait a frame to ensure GameManager is ready
        yield return null;

        var gameManager = FindObjectOfType<GameManager>();
        var saveManager = FindObjectOfType<GameSaveManager>();
        if (gameManager != null && saveManager != null)
        {
            var saveData = saveManager.LoadGame();
            if (saveData != null)
            {
                // Create cards based on saved state
                int cardCount = saveData.gridHeight * saveData.gridWidth;
                for (int i = 0; i < cardCount; i++)
                {
                    GameObject card = Instantiate(cardPrefab, transform);
                    card.name = "Card (" + i.ToString() + ")";
                    var cardController = card.GetComponent<CardController>();
                    cardControllers.Add(cardController);

                    // Find and apply the saved state for this card
                    var savedState = saveData.cardStates[i];
                    if (savedState != null)
                    {
                        // Find the matching card from collection
                        var cardData = cardCollection.cards.Find(c => 
                            c.cardName == savedState.cardName && 
                            c.pairName == savedState.pairName);
                            
                        if (cardData != null)
                        {
                            cardController.SetCardDatas(cardData);
                            cardController.RestoreState(
                                savedState.isMatched,
                                savedState.isVisible,
                                savedState.cardName,
                                savedState.pairName,
                                savedState.isActive
                            );
                        }
                    }
                }
                
                SetGameManagerCardCount();
                gameManager.cardCount = saveData.remainingCards;
            }
        }
    }

    private void SetCardConfig()
    {
        CardGridLayout cardGridLayout = GetComponent<CardGridLayout>();
        cardGridLayout.rows = MenuController.instance.GridHeight;
        cardGridLayout.columns = MenuController.instance.GridWidth;
        
        cardGridLayout.Invoke("CalculateLayoutInputHorizontal", 0.2f);
    }

    private void GenerateCards()
    {
        int cardCount = MenuController.instance.GridHeight * MenuController.instance.GridWidth;
        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = Instantiate(cardPrefab, transform);
            card.name = "Card (" + i.ToString() + ")";
            cardControllers.Add(card.GetComponent<CardController>());
        }

        int halfCardCount = cardCount / 2;
        for (int i = 0; i < halfCardCount; i++)
        {
            CardScriptableObject randomCard = cardGridGenerator.GetRandomAvailableCardSO();
            SetRandomCardToGrid(randomCard, true);
            CardScriptableObject randomCardPair = cardGridGenerator.GetCardPairSO(randomCard.cardName);
            SetRandomCardToGrid(randomCardPair, false);
        }

        // Save initial configuration to GameManager
        SaveInitialConfiguration();
    }

    private void SetRandomCardToGrid(CardScriptableObject randomCard, bool isFirstOfPair)
    {
        int index = cardGridGenerator.GetRandomCardPositionIndex();
        CardController cardObject = cardControllers[index];
        cardObject.SetCardDatas(randomCard);

        // Store initial configuration
        var initialData = new GameSaveData.InitialCardData(
            index,
            randomCard.cardName,
            randomCard.pairName,
            isFirstOfPair ? index * 2 : index * 2 + 1  // Generate unique ID for pairs
        );
        initialCardConfiguration.Add(initialData);
    }

    private void SaveInitialConfiguration()
    {
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.SetInitialCardConfiguration(initialCardConfiguration.ToArray());
        }
    }

    private void SetGameManagerCardCount()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            int totalCards = MenuController.instance.GridHeight * MenuController.instance.GridWidth;
            gameManager.CardCount = totalCards;
            Debug.Log($"Setting GameManager card count to {totalCards} ({MenuController.instance.GridHeight}x{MenuController.instance.GridWidth})");
        }
    }

    // Method to get current card states for saving
    public GameSaveData.CardState[] GetCurrentCardStates()
    {
        var states = new List<GameSaveData.CardState>();
        
        for (int i = 0; i < cardControllers.Count; i++)
        {
            var card = cardControllers[i];
            var initialData = initialCardConfiguration.FirstOrDefault(c => c.position == i);
            
            if (card != null && initialData != null)
            {
                var state = new GameSaveData.CardState(
                    card.IsMatched(),
                    card.IsFaceUp(),
                    initialData.cardId,
                    initialData.cardName,
                    initialData.pairName,
                    card.GetGridX(),
                    card.GetGridY(),
                    card.gameObject.activeSelf
                );
                states.Add(state);
            }
        }
        
        return states.ToArray();
    }
}
