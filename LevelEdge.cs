using UnityEngine;
using System.Collections;

public class LevelEdge : MonoBehaviour 
{
	// controls the level edge collider in order to remove ground blocks as they move off the player's screen

	// prefabs for obstacles
	public Transform obstacleCarrot, obstacleTreeSummer;//, obstacleTreeWinter;

	// the amount in local space that a ground block will be moved along the X axis when reset by this trigger
	private Vector3 localResetVector = new Vector3(48,0,0); 

	// float used to compare random rolls against for spawning obstacles.  Should slowly increase over time
	private float randLimit = 90f;
	private float randOffset = 0f;

	// -------------------------------------

	// as ground objects collide with this trigger, they will have their X value negated in order to contanstly reset their position to be in front of the player
	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Obstacle")
			Destroy (other.gameObject);

		else if (other.tag == "GroundBlock")
		{
			other.transform.localPosition += localResetVector;

			int rollRandom = Random.Range (0, 100);

			if (rollRandom > randLimit + randOffset)
			{
				randOffset += 20f;

				Transform thisObstacle;

				if ( Random.Range(0,2) == 0)
					thisObstacle = (Transform) Instantiate(obstacleCarrot);

				else
					thisObstacle = (Transform) Instantiate(obstacleTreeSummer);

				thisObstacle.parent = other.transform;
				thisObstacle.localPosition = Vector3.zero;
			}
		}
	}

	void Update()
	{
		randLimit -= (Time.deltaTime * (0.02f * (randLimit - 20)) );

		if (randOffset > 0)
			randOffset -= Time.deltaTime * 0.2f * Mathf.Max(1, Time.time);
	}
}
