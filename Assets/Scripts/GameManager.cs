using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
	public UIController uiController;
	private GameSaveManager saveManager;
	private GameSaveData.InitialCardData[] initialCardConfiguration;
	private CanvasManager canvasManager;
	private bool isLoadingGame = false;

	GameState gameState;

	public CardSelectionState cardSelectionState;
	public PairSelectionState pairSelectionState;
	public MemorizeCardsState memorizeCardsState;
	public MatchingCardsState matchingCardsState;
	public EndGameState endGameState;
	public PauseGameState pauseGameState;
	

	public GameObject[] selectedCards;

	public int cardCount;
	int movesCount;

	public int CardCount
	{
		set
		{
			cardCount = value;
		}
		get
		{
			return cardCount;
		}
	}

	
	void Start()
	{
		saveManager = gameObject.AddComponent<GameSaveManager>();
		canvasManager = FindObjectOfType<CanvasManager>();
		movesCount = 0;
		selectedCards = new GameObject[2];
		selectedCards[0] = null;
		selectedCards[1] = null;

		InitStates();

		if (MenuController.isLoadingSavedGame)
		{
			StartCoroutine(LoadSavedGameAfterDelay());
		}
	}

	public void SetInitialCardConfiguration(GameSaveData.InitialCardData[] config)
	{
		initialCardConfiguration = config;
		Debug.Log($"Stored initial configuration for {config.Length} cards");
	}

	private IEnumerator LoadSavedGameAfterDelay()
	{
		isLoadingGame = true;
		// Wait for canvas to initialize
		yield return new WaitForSeconds(0.2f);
		LoadSavedGame();
		isLoadingGame = false;
		// Reset the flag after loading
		MenuController.isLoadingSavedGame = false;
	}

	private void LoadSavedGame()
	{
		Debug.Log("Starting to load saved game...");

		// Load the actual game state
		var saveData = saveManager.LoadGame();
		if (saveData != null)
		{
			// Update moves count
			movesCount = saveData.movesCount;
			uiController.ChangeMovesCount(movesCount);

			// Update remaining cards
			cardCount = saveData.remainingCards;

			// Restore timer
			var timer = FindObjectOfType<TimerController>();
			if (timer != null)
			{
				timer.SetCurrentTime(saveData.elapsedTime);
			}
		}
	}

	void Update()
	{
		gameState.UpdateAction();

		if (!isLoadingGame && cardCount <= 0)
		{
			TransitionState(endGameState);
		}
	}

	void InitStates()
	{
		cardSelectionState = new CardSelectionState(this);
		pairSelectionState = new PairSelectionState(this);
		memorizeCardsState = new MemorizeCardsState(this, 0.5f);
		matchingCardsState = new MatchingCardsState(this, 0.2f);
		pauseGameState = new PauseGameState(this);
		endGameState = new EndGameState(this);

		gameState = cardSelectionState;
	}

	public void TransitionState(GameState newState)
	{
		gameState.EndState();
		gameState = newState;
		gameState.EnterState();
	}

	public void SetSelectedCard(GameObject selectedCard)
	{
		// Prevent selecting the same card twice or already matched cards
		if (selectedCard == null || 
			selectedCard.GetComponent<CardController>().IsMatched() ||
			selectedCards[0] == selectedCard ||
			selectedCards[1] == selectedCard)
		{
			return;
		}

		movesCount++;
		uiController.ChangeMovesCount(movesCount);

		if (selectedCards[0] == null)
		{
			selectedCards[0] = selectedCard;
			TransitionState(pairSelectionState);
		}
		else if (selectedCards[1] == null)
		{
			selectedCards[1] = selectedCard;

			if (MatchSelectedCards())
			{
				TransitionState(matchingCardsState);
			}
			else
			{
				TransitionState(memorizeCardsState);
			}
		}
	}

	bool MatchSelectedCards()
	{
		if (selectedCards[0] == null || selectedCards[1] == null)
		{
			return false;
		}

		CardController first = selectedCards[0].GetComponent<CardController>();
		CardController second = selectedCards[1].GetComponent<CardController>();

		if (first == null || second == null || first.cardType == null || second.cardType == null)
		{
			return false;
		}

		return first.cardType.cardName == second.cardType.pairName && 
			   first.cardType.pairName == second.cardType.cardName;
	}

	public void RemoveSelectedCards()
	{
		selectedCards[0] = null;
		selectedCards[1] = null;
	}

	public void SaveGame()
	{
		if (canvasManager != null)
		{
			saveManager.SaveGame(this, FindObjectOfType<TimerController>());
			Debug.Log("Game state saved successfully");

			// Add to saved games list
			MenuController.instance.AddSavedGame(
				movesCount,
				cardCount,
				MenuController.instance.GridWidth * MenuController.instance.GridHeight
			);

			uiController.QuitToMenu();
		}
	}
}
