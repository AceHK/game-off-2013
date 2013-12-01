using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour 
{
	// main script for player control

	// gui style
	public GUISkin guiSkin;
	private GUIStyle styleSmall = new GUIStyle();

	// music audio component
	public AudioSource musicAudio;

	// the instructions gui texture
	public GUITexture instructions, restartGUI;

	// level controller script
	public LevelController levelController;

	// the two transforms for the upper and lower player objects
	public Transform upperTrans, lowerTrans;

	// the accessory mesh renderers
	public Renderer upperAccessoryRenderer, lowerAccessoryRenderer;

	// various player components
	private Transform playerTrans;
	private Rigidbody playerRigid;

	// the transform for the player shadow
	public Transform shadowTrans;

	// materials and colors for the player and shadow taken from the renderers of the player and shadow objects
	public Renderer upperRenderer, lowerRenderer;
	private Material upperMat, lowerMat;
	private Color colorOn, colorOff;

	// the two light sources and culling masks for lighting and not lighting the player object
	public Light upperSun, lowerSun;
	public LayerMask cullMaskPlayerOn, cullMaskPlayerOff;

	// int for storing the current orientation of the player (right side up in the upper level OR upside down in the lower level) and should always be 1 or -1
	public int curOri = 1;

	// layerMask used for testing grounded status and status of the grounded raycast test where 0 is not grounded and 1 is grounded
	public LayerMask groundedMask;
	private int groundedInt = 1;

	private bool canSwap = true;

	// moveSpeed is the speed at which the player is moved along the X (horizontal) axis
	public float moveSpeed;

	// forces to use for jumping and gravity, and the bool used to give a jump cooldown
	public float jumpForce, gravity;
	private bool canJump = true;

	// bool for whether the player is dead and the float to hold the float for distance travelled this run
	public bool isDead = false;
	private float completedDistance = 0;

	// can the gui show now?
	private bool showGUI = false;

	// -----------------------------------

	// get necessary components
	void Start()
	{
		playerTrans = upperTrans.transform;
		playerRigid = upperTrans.rigidbody;
	
		upperMat = upperRenderer.material;
		lowerMat = lowerRenderer.material;

		StartCoroutine("FadeMusic", 1);
		StartCoroutine ("FadeInstructions");

		styleSmall.normal.textColor = new Color(1,1,1,0.4f);
		styleSmall.fontSize = 11;
		styleSmall.alignment = TextAnchor.LowerLeft;
	}


	// fixed update is used for physics reliant stuff
	void FixedUpdate()
	{
		if (!isDead)
		{
			GroundedTest();
			
			// if player is not grounded, pull the player towards the center of gravity
			if (groundedInt == 0)
			{
				playerRigid.AddForce( new Vector3(0, -1 * curOri * gravity, 0));
			}
		}
	}


	// test whether the player is grounded using a raycast in the appropriate direction
	void GroundedTest()
	{
		RaycastHit hit;
		
		if (Physics.Raycast(playerTrans.position + new Vector3(0,curOri,0), Vector3.up * curOri * -1, out hit, 1.1f, groundedMask))
		{
			groundedInt = 1;
		}
		
		else
			groundedInt = 0;
	}


	// update handles inputs and timers
	void Update()
	{
		if (!isDead)
		{
			// int for storing the player's input during this update
			int inputInt = 0;

			// Jump and swap will vary based on whether the player is on the upper or lower part of the level, this will be controlled via the int curOri
			if (Input.GetKey(KeyCode.W) && canJump)
				StartCoroutine("DoJump");

			else if (Input.GetKey(KeyCode.E) && canSwap)
				StartCoroutine("DoSwap");
		}

		// update the shadow transform's position to mirror this transform
		shadowTrans.position = new Vector3(playerTrans.position.x, -1 * playerTrans.position.y, playerTrans.position.z);

		// update the distance completed in each update
		completedDistance = levelController.transform.position.x / -2;
	}


	// kill the player
	public void KillPlayer()
	{
		if (!isDead)
		{
			isDead = true;
			levelController.levelSpeed = 0;
			Time.timeScale = 1f;
			upperTrans.rigidbody.isKinematic = true;
			lowerTrans.rigidbody.isKinematic = true;

			StartCoroutine("FadeMusic", 0);
		}
	}

	// allow the player to jump based on the side of the map that the player is on
	IEnumerator DoJump()
	{
		canJump = false;

		// if grounded, then jump on the next fixed update
		if (groundedInt == 1)
		{
			playerRigid.AddForce(new Vector3(0, curOri * jumpForce, 0), ForceMode.Impulse);

			for (float t = 0f ; t < 1f ; t += Time.fixedDeltaTime)
			{
				yield return new WaitForFixedUpdate();

				if (Input.GetKey(KeyCode.W))
					playerRigid.AddForce(new Vector3(0, 8 * (1f - Mathf.Sqrt(t)) * curOri * jumpForce, 0));
				else
					break;
			}
		}

		yield return new WaitForSeconds(0.1f);
		canJump = true;
	}


	// allow the player to swap sides of the level
	IEnumerator DoSwap()
	{
		canSwap = false;
		canJump = false;

		yield return new WaitForFixedUpdate();

		if (curOri == -1)
		{
			curOri = 1;
			Vector3 thisVeloc = playerRigid.velocity;
			lowerTrans.collider.enabled = false;
			upperTrans.collider.enabled = true;
			playerTrans = upperTrans;
			shadowTrans = lowerTrans;
			playerRigid = upperTrans.rigidbody;
			playerRigid.velocity = new Vector3(thisVeloc.x, -thisVeloc.y, thisVeloc.z);

			upperRenderer.material = upperMat;
			lowerRenderer.material = lowerMat;

			upperAccessoryRenderer.enabled = true;
			lowerAccessoryRenderer.enabled = false;
		}
		
		else
		{
			curOri = -1;
			Vector3 thisVeloc = playerRigid.velocity;
			upperTrans.collider.enabled = false;
			lowerTrans.collider.enabled = true;
			playerTrans = lowerTrans;
			shadowTrans = upperTrans;
			playerRigid = lowerTrans.rigidbody;
			playerRigid.velocity = new Vector3(thisVeloc.x, -thisVeloc.y, thisVeloc.z);

			upperRenderer.material = lowerMat;
			lowerRenderer.material = upperMat;

			upperAccessoryRenderer.enabled = false;
			lowerAccessoryRenderer.enabled = true;
		}
				
		// wait for a fixed update before allowing the player to swap again
		yield return new WaitForSeconds(0.2f);
		canSwap = true;
		canJump = true;
	}


	// fade out music then show gui after death OR fade in music
	IEnumerator FadeMusic(int fadeDir)
	{
		float startVol = (fadeDir - 1) * -1;
		float endVol = fadeDir;

		for (float t = 0 ; t <= 1 ; t += 4 * Time.deltaTime)
		{
			musicAudio.volume = Mathf.Lerp(startVol, endVol, t);
			yield return null;
		}

		musicAudio.volume = endVol;

		if (fadeDir < 1)
			showGUI = true;
	}

	// fade the instructions in and out at the start of each attempt
	IEnumerator FadeInstructions()
	{
		Color thisColor = Color.grey;
		thisColor.a = 0;

		// fade in
		for (float t = 0f; t <= 1 ; t += 2 * Time.deltaTime)
		{
			thisColor.a = t / 2;
			instructions.color = thisColor;
			yield return null;
		}

		yield return new WaitForSeconds(3f);

		// fade out
		for (float t = 0f; t <= 1 ; t += 2 * Time.deltaTime)
		{
			thisColor.a =  (-t + 1) / 2;
			instructions.color = thisColor;
			yield return null;
		}

		thisColor.a = 0;
		instructions.color = thisColor;
	}

	// display relevent gui info
	void OnGUI()
	{
		GUI.Label ( new Rect(4, Screen.height - 20, 800, 20), "Music provided by Benjamin Burnes -- abstractionmusic.bandcamp.com", styleSmall);

		GUI.skin = guiSkin;

		// current distance travelled
		GUI.Label( new Rect(8,4,50,20), completedDistance.ToString("f0")+"m"); 

		// if player is dead, show restart menu
		if (isDead && showGUI)
		{
			GUI.Label( new Rect(400, 80, 200, 20),  "Total Distance:  " + completedDistance.ToString("f0") + "m");
			restartGUI.enabled = true;

			if ( Input.GetKeyUp(KeyCode.R))
			{
				Time.timeScale = 1;
				Application.LoadLevel(Application.loadedLevel);
			}
		}
	}
}
