using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArcherTopController : PlayerTopScript {

    public GameObject ArcherTurningActive;

	GameObject arrowPrefab;
	GameObject player;
	GameObject arrow;

	private float stickDir;
	private Vector2 stickInput;
	private float deadzone = 0.25f;

	private float aimRange = 90f;
	private float leftRange;
	private float rightRange;

    public AudioClip shootSound;
    private AudioSource source;


	void Start() {
		arrowPrefab = Resources.Load ("Arrow") as GameObject;
		player = GameObject.Find("CompletePlayer");
		arrow = null;
        //audio setup
        source = GetComponent<AudioSource>();
	}

	void FixedUpdate()
	{
		/* Rotate the players based off the given input */
		float turn = Input.GetAxisRaw("Horizontal2");
        if (turn != 0)
        {
            ArcherTurningActive.SetActive(true);
            Turn(turn);
        }
        else
        {
            ArcherTurningActive.SetActive(false);
        }

        // Aim Axis
        float aimX = Input.GetAxis("AimX");
		float aimY = Input.GetAxis ("AimY");

		// Math to convert Axis to Angle Direction
		stickDir = Mathf.Atan2 (aimX, aimY) * Mathf.Rad2Deg;

		// Used for Deadzone Check
		stickInput = new Vector2(aimX, aimY);

		// If joystick is active
		if (stickInput.magnitude > deadzone) {
			Aim (aimX, aimY);
		} 
		// Fire on X-Axis Release
		else {
			// Store the arrow in the arrow list
			if (arrow != null) {
				// Calls Destroy on the arrow
				Shoot (arrow);
				// Reset arrow, for next arrow to be shot
				arrow = null;
			}
		}
	}

	/*
	 * Arrow Related Code Start
	 */

	void CreateArrow() {
		arrow = Instantiate (arrowPrefab) as GameObject;
	}

	void UpdatePosition(float angle) {
		// Arrow Should Rotate Around Player
		Vector3 playerPos = player.transform.position;
		float rads = angle * Mathf.Deg2Rad;

		// Math here...
		// Rotation around Origin with Vector [0, 0, 0.5]
		float newX = Mathf.Sin(rads) / 2;
		float newZ = Mathf.Cos(rads) / 2;

		// Make new vector for the arrow position
		Vector3 newPos = new Vector3 (newX, 0, newZ);

		// Move the arrow into place
		arrow.transform.position = newPos + 
			// Center Origin at the player
			playerPos + 
			// Move Arrow up to proper height
			Vector3.up * transform.GetComponent<CapsuleCollider>().height / 2;
	}

	void Shoot(GameObject arrow)
	{
		// Add Arrow Velocity, Driection of Arrow Head
		Rigidbody rb = arrow.GetComponent<Rigidbody>();
		rb.velocity = arrow.transform.up * 10;

        //shoot sound
        source.PlayOneShot(shootSound, 1F);

		// Destroy the Arrow After 3 Seconds
		Destroy(arrow.gameObject, 3);
	}

	void Aim (float aimX, float aimY) {
		// if arrow does not exist
		if (arrow == null) {
			// Init Arrow
			CreateArrow ();
		}
		// Arrow exist, now aim the arrow
		else {
			// Rotate Arrow in the direction of the Joystick
			LimitAimCone();
		}
	}

	void LimitStickDir(float angle) {
		// Controller Direction angles are 180 to -180
		// Convert rotation angle to controller Angle
		if (angle > 180) {
			angle = angle - 360;
		}
		arrow.transform.rotation = Quaternion.Euler (new Vector3 (90, angle, 0));
	}

	void LimitAimCone() {
		float playerRotation = player.transform.eulerAngles.y;
		float arrowRot = playerRotation + stickDir;

		arrow.transform.rotation = Quaternion.Euler (new Vector3 (90, arrowRot, 0));

		if (arrowRot > 360) {
			arrowRot = arrowRot - 360f;
		}
		if (arrowRot < 0) {
			arrowRot = arrowRot + 360f;
		}

		leftRange = playerRotation - aimRange;
		rightRange = playerRotation + aimRange;


		// Find Min Ranges
		if (leftRange < 0f) {
			leftRange = leftRange + 360f;
		}

		// Find Max Ranges
		if (rightRange > 360f) {
			rightRange = rightRange - 360f;
		}

		// EDGE CASE
		// if leftRange is larger than rightRange
		// the cone will be from min to 0 to max
		// this case need to be cover :(
		if (leftRange > rightRange) {
			if (arrowRot < leftRange && arrowRot > leftRange - aimRange) {
				UpdatePosition (leftRange);
				LimitStickDir (leftRange);
			} else if (arrowRot > rightRange && arrowRot < rightRange + aimRange) {
				UpdatePosition (rightRange);
				LimitStickDir (rightRange);
			} else {
				UpdatePosition (arrowRot);
			}
		} 
		// NORMAL CASE HERE
		else {
			// Update Arrow with Range Restrictions
			// Maintain Min Value if Rotation is under Min
			if (arrowRot < leftRange) {
				UpdatePosition (leftRange);
				LimitStickDir (leftRange);
			} 
			// Maintain Max Value if Rotation is under Max
			else if (arrowRot > rightRange) {
				UpdatePosition (rightRange);
				LimitStickDir (rightRange);
			} 
			// All good, No limit
			else {
				UpdatePosition (arrowRot);
			}
		}
	}
	/*
	 * Arrow Related Code End
	 */
}
