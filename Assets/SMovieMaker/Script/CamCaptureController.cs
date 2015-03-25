﻿using UnityEngine;
using System.Collections;

public class CamCaptureController : MonoBehaviour {

	
	public CamRecordController camCaptureControler;
	private bool isRecording = false; 
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown ("r")) {
			//Debug.Log("WORKING");
			if(isRecording)
			{
				isRecording = false;
			} else {
				isRecording = true;
			}
		}

		if(Input.GetKeyDown("s")) {
			camCaptureControler.SaveMovie();
		}
		
		if(isRecording) {
			camCaptureControler.recVideo = true;
			camCaptureControler.recAudio = true;
		} else {
			camCaptureControler.recVideo = false;
			camCaptureControler.recAudio = false;
		}
	}
}