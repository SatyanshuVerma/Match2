using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CardController : MonoBehaviour, IPointerDownHandler
{
	public Image frontFace;
	public Image backFace;

	public CardScriptableObject cardType;

	GameManager gameManager;
	StarsManager starsManager;
	public AudioManager audioManager;

	public CardState actualState;
	public FrontState frontState;
	public BackState backState;
	public FlippingState flippingState;
	public BackFlippingState backFlippingState;
	public MemorizeState memorizeState;
	public HideAwayState hideAwayState;

	float cardScale = 1.0f;
	float flipSpeed = 2.0f;
	float flipTolerance = 0.05f;

	// Start is called before the first frame update
	void Start()
	{
		if (frontState == null)
		{
			InitializeStates();
		}
	}

	// Update is called once per frame
	void Update()
	{
		actualState.UpdateActivity();
	}

	internal void SetCardDatas( CardScriptableObject card)
	{
		this.cardType = card;

		frontFace.sprite = card.cardImage;	

		backFace.gameObject.SetActive(true);
		frontFace.gameObject.SetActive(false);
	}

	public void TransitionState(CardState newState)
	{
		if (newState != null)  // Add null check
		{
			this.actualState.EndState();
			this.actualState = newState;
			this.actualState.EnterState();
		}
		else
		{
			Debug.LogWarning("Attempted to transition to null state!");
		}
	}

	public void SwitchFaces()
	{
		backFace.gameObject.SetActive(!backFace.gameObject.activeSelf);
		frontFace.gameObject.SetActive(!frontFace.gameObject.activeSelf);
	}

	public void InactivateCard()
	{
		backFace.gameObject.SetActive(false);
		frontFace.gameObject.SetActive(false);

		Image cardImage = this.GetComponent<Image>();
		Color newColor = cardImage.color;
		newColor.a = 0.0f;
		cardImage.color = newColor;
	}

	public void AddStarSpawner()
	{
		starsManager.SpawnStarSpawner(this.transform.position);
	}

	public void ChangeScale(float newScale)
	{
		this.transform.localScale = new Vector3(newScale, 1, 1);
	}

	public void Flip()
	{
		//Hide background
		if (backFace.gameObject.activeSelf == true)
		{
			cardScale = cardScale - (flipSpeed * Time.deltaTime);
			ChangeScale(cardScale);
			//Show foreground
			if (flipTolerance > cardScale)
			{
				SwitchFaces();
			}
		}
		else
		{
			cardScale = cardScale  + (flipSpeed * Time.deltaTime);
			ChangeScale(cardScale);

			if(cardScale >= 1.0f)
			{
				ChangeScale(1.0f);
				TransitionState(this.frontState);
				gameManager.SetSelectedCard(this.gameObject);
			}
		}
	}
	
	public void BackFlip()
	{
		//Hide foreground
		if (backFace.gameObject.activeSelf == false)
		{
			cardScale = cardScale - (flipSpeed * Time.deltaTime);
			ChangeScale(cardScale);
			//Show foreground
			if (flipTolerance > cardScale)
			{
				SwitchFaces();
			}
		}
		else
		{
			cardScale = cardScale + (flipSpeed * Time.deltaTime);
			ChangeScale(cardScale);

			if (cardScale >= 1.0f)
			{
				ChangeScale(1.0f);
				TransitionState(this.backState);
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		// Prevent clicking if card is already matched or in transition
		if (IsMatched() || actualState is FlippingState || actualState is BackFlippingState || actualState is MemorizeState)
		{
			return;
		}

		// Prevent clicking if this card is already selected
		if (gameManager.selectedCards[0] == this.gameObject || gameManager.selectedCards[1] == this.gameObject)
		{
			return;
		}

		actualState.OnClickAction();
	}

	public bool IsMatched()
	{
		return actualState is HideAwayState;
	}

	public bool IsFaceUp()
	{
		return frontFace.gameObject.activeSelf;
	}

	public int GetGridX()
	{
		return transform.GetSiblingIndex() % MenuController.instance.GridWidth;
	}

	public int GetGridY()
	{
		return transform.GetSiblingIndex() / MenuController.instance.GridWidth;
	}

	public void RestoreState(bool isMatched, bool isVisible, string cardName, string pairName, bool isActive)
	{
		// Initialize states if they haven't been initialized yet
		if (frontState == null)
		{
			InitializeStates();
		}

		// Find the matching card data in the card collection
		var cardCollection = FindObjectOfType<CardCollectionSO>();
		if (cardCollection != null)
		{
			var matchingCard = cardCollection.cards.Find(c => c.cardName == cardName && c.pairName == pairName);
			if (matchingCard != null)
			{
				SetCardDatas(matchingCard);
			}
		}

		// Set visibility
		gameObject.SetActive(isActive);
		
		if (isMatched)
		{
			TransitionState(hideAwayState);
		}
		else if (isVisible)
		{
			TransitionState(frontState);
			frontFace.gameObject.SetActive(true);
			backFace.gameObject.SetActive(false);
		}
		else
		{
			TransitionState(backState);
			frontFace.gameObject.SetActive(false);
			backFace.gameObject.SetActive(true);
		}
	}

	private void InitializeStates()
	{
		gameManager = FindObjectOfType<GameManager>();
		starsManager = FindObjectOfType<StarsManager>();
		audioManager = FindObjectOfType<AudioManager>();

		frontState = new FrontState(this);
		backState = new BackState(this);
		flippingState = new FlippingState(this);
		backFlippingState = new BackFlippingState(this);
		hideAwayState = new HideAwayState(this);
		memorizeState = new MemorizeState(this, 0.5f);

		actualState = backState;
	}
}
