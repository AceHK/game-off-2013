using UnityEngine;
using System.Collections;

public class PlayerCollisions : MonoBehaviour 
{
	// script attached to the actual player collider objects in order to detect collisions
	
	// the player control script which should be attached to the parent of each object that this script is on
	public PlayerControl playerControl;

	// this unit's transform
	private Transform trans;

	// this unit's parent's orientation where 1 is rightside up and -1 is upside down
	public int parentOri = 1;
	
	// -------------------------------------

	void Awake()
	{
		trans = transform;
		playerControl = transform.parent.GetComponent<PlayerControl>();
	}


	// on collision enter, test whether the collision will kill the player or not
	void OnCollisionEnter(Collision other)
	{
		foreach(ContactPoint contact in other.contacts)
		{
			if (contact.point.x > trans.position.x - 0.5f)
			{
				if (parentOri == 1)
				{
					if (contact.point.y - 0.15f > trans.position.y)
						playerControl.KillPlayer();
				}

				else
				{
					if (contact.point.y + 0.15f < trans.position.y)
						playerControl.KillPlayer();
				}
			}
		}
	}
}
