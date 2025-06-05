[System.Serializable]
public class GameSaveData
{
    // Game state
    public int movesCount;
    public float elapsedTime;
    public int remainingCards;
    
    // Grid configuration
    public int gridWidth;
    public int gridHeight;
    
    // Enhanced card tracking
    public CardState[] cardStates;

    [System.Serializable]
    public class CardState
    {
        public bool isMatched;
        public bool isVisible;
        public int cardId;
        public string cardName;    // Store the actual card name/type
        public string pairName;    // Store the pair name for matching
        public int gridX;
        public int gridY;
        public bool isActive;      // Whether the card is still in play

        public CardState(bool matched, bool visible, int id, string cardName, string pairName, int x, int y, bool active)
        {
            isMatched = matched;
            isVisible = visible;
            cardId = id;
            this.cardName = cardName;
            this.pairName = pairName;
            gridX = x;
            gridY = y;
            isActive = active;
        }
    }

    // Store initial card configuration for proper restoration
    public InitialCardData[] initialConfiguration;

    [System.Serializable]
    public class InitialCardData
    {
        public int position;       // Index in the grid
        public string cardName;    // Card's scriptable object name
        public string pairName;    // Pair's name for matching
        public int cardId;         // Unique identifier

        public InitialCardData(int pos, string cardName, string pairName, int id)
        {
            position = pos;
            this.cardName = cardName;
            this.pairName = pairName;
            cardId = id;
        }
    }
} 