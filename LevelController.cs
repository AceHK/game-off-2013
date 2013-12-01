using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour 
{
	// controls the level, specifically movement
		
	private Transform trans;

	// how much the level moves per update and how fast it increases (should be quite a small number)
	public float levelSpeed, speedIncreaseRate;

	// player animation components must adjust speed based on the level speed
	public Animation upperAnim, lowerAnim;

	// -------------------------------------

	void Start()
	{
		trans = transform;
	}


	// movement of the level
	void Update()
	{
		trans.position = new Vector3(trans.position.x - levelSpeed * Time.deltaTime, trans.position.y, trans.position.z);

		if (levelSpeed > 0.5f)
			levelSpeed += Time.deltaTime * speedIncreaseRate;

		upperAnim["Walk"].speed = levelSpeed / 4;
		lowerAnim["Walk"].speed = levelSpeed / 4;
	}
}
