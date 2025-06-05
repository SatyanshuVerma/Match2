using UnityEngine;

public class Card : MonoBehaviour
{
    private bool isMatched = false;
    private int cardId;
    private int gridX;
    private int gridY;

    public void SetCardData(int id, int x, int y)
    {
        cardId = id;
        gridX = x;
        gridY = y;
    }

    public bool IsMatched()
    {
        return isMatched;
    }

    public int GetCardId()
    {
        return cardId;
    }

    public int GetGridX()
    {
        return gridX;
    }

    public int GetGridY()
    {
        return gridY;
    }

    public void SetMatched(bool matched)
    {
        isMatched = matched;
    }
} 