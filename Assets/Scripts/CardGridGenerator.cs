using System;
using System.Collections.Generic;
using UnityEngine;

public class CardGridGenerator 
{
	CardCollectionSO cardCollection;

	

	List<int> availableImageIndexes;
	List<int> availablePositionIndexes;
    Vector2[] positions;
    List<GameObject> cards;

    int cardCount;

	public CardGridGenerator(CardCollectionSO cardCollection)//, GameDatasSO gameDatas)
	{
		this.cardCollection = cardCollection;		
        cardCount = MenuController.instance.GridHeight * MenuController.instance.GridWidth;

        Debug.Log("CARD COUNT" + cardCount);
		GenerateAvailableImageIndexes();
		GenerateAvailablePositionIndexes(cardCount);
	}

	public CardScriptableObject GetRandomAvailableCardSO()
	{
		int random = UnityEngine.Random.Range(0, this.availableImageIndexes.Count);
		int randomIndex = availableImageIndexes[random];

		availableImageIndexes.RemoveAt(random);

		return cardCollection.cards[randomIndex];
	}

	public CardScriptableObject GetCardPairSO(string cardPairName)
	{
		foreach(CardScriptableObject card in cardCollection.cards)
		{
			if(card.IsPair(cardPairName))
			{
				return card;
			}
		}

		return null;
	}

	public int GetRandomCardPositionIndex()
	{
		int randomIndex = UnityEngine.Random.Range(0, availablePositionIndexes.Count);
		int randomPosition = availablePositionIndexes[randomIndex];
		Debug.Log("GetRandomCardPositionIndex -- :availablePositionIndexes: " + randomPosition);
       
        availablePositionIndexes.RemoveAt(randomIndex);

		return randomPosition;
	}

	void GenerateAvailableImageIndexes()
	{
		availableImageIndexes = new List<int>();
		int requiredPairs = cardCount / 2;  // Calculate how many unique pairs we need
		int index = cardCollection.cards.Count;

		// Add indices for the required number of pairs
		for(int i = 0; i < index && availableImageIndexes.Count < requiredPairs; i++)
		{
			if (i % 2 == 0)  // Only add one card from each pair
			{
				availableImageIndexes.Add(i);
			}
		}

		// If we don't have enough pairs, start over from the beginning
		if (availableImageIndexes.Count < requiredPairs)
		{
			int currentCount = availableImageIndexes.Count;
			for(int i = 0; i < index && availableImageIndexes.Count < requiredPairs; i++)
			{
				if (i % 2 == 0)
				{
					availableImageIndexes.Add(i);
				}
			}
		}

		Debug.Log($"Generated {availableImageIndexes.Count} available pairs for {cardCount} total cards");
	}

	private void GenerateAvailablePositionIndexes(int cardCount)
	{
		availablePositionIndexes = new List<int>();

		for (int i = 0; i < cardCount; i++)
		{
			availablePositionIndexes.Add(i);
			Debug.Log("availablePositionIndexes ADDED: "+ i);
		}
	}
}
