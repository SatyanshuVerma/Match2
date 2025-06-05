using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameState : GameState
{
	private bool hasActivatedEndPanel = false;

	public EndGameState(GameManager gameManager) : base(gameManager)
	{
	}

	public override void EnterState()
	{
		base.EnterState();

		if (!hasActivatedEndPanel)
		{
			TimerController tc = GameObject.FindObjectOfType<TimerController>();
			if (tc != null)
			{
				tc.PauseGame();
			}

			gameManager.uiController.ActivateEndPanel();
			hasActivatedEndPanel = true;
		}
	}

	public override void EndState()
	{
		base.EndState();
		hasActivatedEndPanel = false;
	}

	public override void UpdateAction()
	{
		return;
	}
}
