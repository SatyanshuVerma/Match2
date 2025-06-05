using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
	public GameObject endGamePanel;
	public GameObject pauseGamePanel;

	public TMP_Text countText;

	public TMP_InputField userNameField;

	int movesCount;

    // Start is called before the first frame update
    void Start()
    {
		endGamePanel.SetActive(false);
		pauseGamePanel.SetActive(false);

		countText.text = "0";
    }

	public void ActivateEndPanel()
	{
		endGamePanel.SetActive(true);
	}

	public void ActivatePausePanel()
	{
		pauseGamePanel.SetActive(true);
	}

	public void ChangeMovesCount(int movesCount)
	{
		this.movesCount = movesCount;
		countText.text = movesCount.ToString();
	}

	public void SaveHighScore()
	{

		string userName = userNameField.text;
		int score = HighScoreHelper.CalculateHighScore(movesCount);

		AddNewScore(userName, score);
		
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
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);


	}
	public void QuitToMenu()
	{
		if (MenuController.instance != null)
		{
			MenuController.instance.ReturnToMenu();
		}
		else
		{
			SceneManager.LoadScene(0);
		}
	}
}
