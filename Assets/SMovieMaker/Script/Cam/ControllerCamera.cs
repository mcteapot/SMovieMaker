using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;


public class ControllerCamera : MonoBehaviour {

	public CamCaptureControler camCaptureControler;
	private bool isRecording = false; 
	private bool isMaxTime = false;
	private bool saveMovie = false;
	private bool isSaving = false;


	public int maxTimeSeconds = 15;

	public UIController uiControler;

	public bool countDown;

	public float saveTime = 3.0f;

	private Stopwatch stopWatch;
	private TimeSpan maxTime;

	private TimeSpan remaining { get { return maxTime - stopWatch.Elapsed; } }

	// Use this for initialization
	void Start () {

		//countDown = true;

		stopWatch = new Stopwatch();
		maxTime = TimeSpan.FromSeconds(maxTimeSeconds);

		uiControler.uiSaveIcon.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {



		inputControls();

		if(isRecording) {
			camCaptureControler.recVideo = true;
			camCaptureControler.recAudio = true;
		} else {
			camCaptureControler.recVideo = false;
			camCaptureControler.recAudio = false;
		}
		checkTime();


	}



	void inputControls() {


		// Input Controls
		if(Input.GetKeyDown ("r")) {
			//Debug.Log("WORKING");
			if(!isSaving) {
				if(isRecording)
				{
					uiControler.uiRecordText.text = "RECORD";
					isRecording = false;
				} else {
					if(stopWatch.Elapsed.Seconds <= maxTimeSeconds) {
						uiControler.uiRecordText.text = "PAUSE";
						isRecording = true;
					}
				}

				if(stopWatch.IsRunning)
				{
					stopWatch.Stop();
				} else {
					if(!isMaxTime) {
						stopWatch.Start();
					}
				}
			}
		}

		/*
		if(Input.GetKeyDown("t")) {
			camCaptureControler.SaveMovie();
			resetTime();
		}


		if(Input.GetKeyDown("p")) {
			if(stopWatch.IsRunning)
			{
				stopWatch.Stop();
			} else {
				if(!isMaxTime) {
					stopWatch.Start();
				}
			}

		}
		*/
	}

	void resetTime() {
		isMaxTime = false;
		stopWatch.Reset();
		maxTime = TimeSpan.FromSeconds(maxTimeSeconds);

	}

	void checkTime() {
		if(stopWatch.Elapsed.Seconds >= maxTimeSeconds) {
			if(stopWatch.IsRunning) {
				isMaxTime = true;
				stopWatch.Stop();
				StartCoroutine(saveingMovie());
			}

			if(isRecording)
			{
				isRecording = false;
			}
		}

		if(uiControler != null) {
			if(stopWatch.Elapsed.Seconds >= maxTimeSeconds) {
				if(!countDown) {
					uiControler.setTimerText("00:" + maxTimeSeconds + ":00");
				} else {
					uiControler.setTimerText("00:00:00");
				}
			} else {
				if(!countDown) {
					// count up
					uiControler.setTimerText(string.Format("{0:00}:{1:00}:{2:00}", stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds, stopWatch.Elapsed.Milliseconds / 10));
				} else {
					// count down
					uiControler.setTimerText(string.Format("{0:00}:{1:00}:{2:00}", remaining.Minutes, remaining.Seconds, remaining.Milliseconds / 10));
				}
			}
		}

		//UnityEngine.Debug.Log(string.Format("{0:00}:{1:00}:{2:00}", stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds, stopWatch.Elapsed.Milliseconds / 10));
		//UnityEngine.Debug.Log(stopWatch.Elapsed.Seconds);

		//UnityEngine.Debug.Log("remaining " + remaining);

	}

	IEnumerator saveingMovie() {
		isSaving = true;

		uiControler.uiSaveIcon.gameObject.SetActive(true);
		uiControler.uiRecordIcon.gameObject.SetActive(false);
		uiControler.uiTimeIcon.gameObject.SetActive(false);

		while(camCaptureControler.recording) {
			yield return null;
		}

		//UnityEngine.Debug.Log("Saving Movie");


		camCaptureControler.SaveMovie();

		yield return new WaitForSeconds(saveTime);

		while(camCaptureControler.isSavingToDrive) {
			yield return null;
		}

		camCaptureControler.InitVideoAudio();

		yield return new WaitForSeconds(1.0f);

		resetTime();

		uiControler.uiSaveIcon.gameObject.SetActive(false);
		uiControler.uiRecordIcon.gameObject.SetActive(true);
		uiControler.uiTimeIcon.gameObject.SetActive(true);

		UnityEngine.Debug.Log("Reset Recording");

		uiControler.uiRecordText.text = "RECORD";

		isSaving = false;
		
		
	}
	
}
