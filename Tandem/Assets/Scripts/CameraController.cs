﻿using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    /* Borrowing the method for camera control from Unity's Training Day tutorial */
    public Transform player;
    public float smoothing = 5f;

    public Vector3 offset;

	// Use this for initialization
	void Start () {
        //offset = transform.position - player.position;
        offset = new Vector3(0f, 7f, -7f);
	}
	
	void FixedUpdate () {
        Vector3 targetCameraPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetCameraPos, smoothing * Time.deltaTime);
	}
}