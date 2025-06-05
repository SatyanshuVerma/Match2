using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsManager: MonoBehaviour
{
	public GameObject starSpawnerPrefab;

	public void SpawnStarSpawner(Vector3 position)
	{
		GameObject starSpawner = Instantiate(starSpawnerPrefab, position, Quaternion.identity, this.transform);
	}
}
