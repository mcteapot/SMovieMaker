using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Debug = UnityEngine.Debug;


public class ControllerCamera : MonoBehaviour {

	public CamCaptureControler camCaptureControler;
	private bool isRecording = false; 
	private bool isMaxTime = false;
	private bool saveMovie = false;
	private bool isSaving = false;

	// TODO: see if this works as a float instead
	public int maxTimeSeconds = 15;

	public UIController uiControler;

	public bool countDown;

	public float saveTime = 3.0f;

	private Stopwatch stopWatch;
	private TimeSpan maxTime;

	private TimeSpan remaining { get { return maxTime - stopWatch.Elapsed; } }
	
	public bool CanRecord
	{
		// HACK: to deal with leftover input from menu
		get { return !isSaving && Time.timeSinceLevelLoad > .25f; }
	}

	public Action<string> SaveCompleted;

	// Use this for initialization
	void Start () 
	{
		//countDown = true;

		stopWatch = new Stopwatch();
		maxTime = TimeSpan.FromSeconds(maxTimeSeconds);

		if(uiControler != null)
		{
			uiControler.uiSaveIcon.gameObject.SetActive(false);
		}
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

	void inputControls() 
	{
		if(CanRecord)
		{
			if(isRecording)
			{
				if(Input.GetMouseButton(0) == false)
				{
					uiControler.uiRecordText.text = "RECORD";
					isRecording = false;

					if(stopWatch.IsRunning)
					{
						stopWatch.Stop();
					}
				}
			}
			else
			{
				if(Input.GetMouseButton(0))
				{
					uiControler.uiRecordText.text = "PAUSE";
					isRecording = true;

					if(!isMaxTime) 
					{
						stopWatch.Start();
					}
				}
			}
		}

		/*
		if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
		{
			//Debug.Log("WORKING");
			if(!isSaving) 
			{
				if(isRecording)
				{
					uiControler.uiRecordText.text = "RECORD";
					isRecording = false;
				} 
				else 
				{
					if(stopWatch.Elapsed.Seconds <= maxTimeSeconds) 
					{
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
		}*/

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
			float progressRatio = (float)(stopWatch.ElapsedTicks / (double)maxTime.Ticks);
			uiControler.SetTimerBar(progressRatio);

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

	IEnumerator saveingMovie() 
	{
		isSaving = true;

		uiControler.uiSaveIcon.gameObject.SetActive(true);
		uiControler.uiRecordIcon.gameObject.SetActive(false);
		uiControler.uiTimeIcon.gameObject.SetActive(false);

		while(camCaptureControler.recording) 
		{
			yield return null;
		}

		//UnityEngine.Debug.Log("Saving Movie");


		var filePath = camCaptureControler.SaveMovie();

		yield return new WaitForSeconds(saveTime);

		while(camCaptureControler.isSavingToDrive) 
		{
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

		if(SaveCompleted != null)
		{
			SaveCompleted(filePath);
		}
	}
	
}
