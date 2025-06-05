using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarBummController : MonoBehaviour
{
	public GameObject starPrefab;
	public int starCount;
	public Gradient colors;

	public int minSpeed = 3000;
	public int maxSpeed = 6000;

	public float timeToLive = 3.0f;
	List<GameObject> stars;

	float timer;

	void Start()
    {
		stars = new List<GameObject>();

		timer = 0.0f;

        for(int i = 0; i < starCount; i++)
		{
			GameObject star = Instantiate(starPrefab, this.transform);
			float random = (float)Random.Range(0, 100) / 100.0f;
			star.GetComponent<Image>().color = colors.Evaluate(random);

			int randomRotation = Random.Range(0, 360);
			star.transform.Rotate(new Vector3(0, 0, randomRotation));

			Rigidbody2D rb = star.GetComponent<Rigidbody2D>();
			float speed = Random.Range(minSpeed, maxSpeed);
			rb.AddForce(star.transform.right * speed, ForceMode2D.Impulse);

			stars.Add(star);
		}
    }

    // Update is called once per frame
    void Update()
    {
		timer += Time.deltaTime;

		if (timer >= timeToLive)
		{
			while(stars.Count > 0)
			{
				GameObject star = stars[0];
				stars.Remove(star);
				Destroy(star);
			}

			Destroy(this.gameObject);
		}
    }
}
