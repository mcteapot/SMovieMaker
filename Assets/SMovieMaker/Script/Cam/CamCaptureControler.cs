using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class CamCaptureControler : MonoBehaviour {
	
	public bool recording;
	public bool recVideo;
	public bool recAudio;

	private static string filePath;

	public bool saveToDesktop;
	public bool recMainCam;

	private int tempVsync;
	private float capTime;

	private int imageQuality;
	private float framerate;

	public RenderTexture renderTexture;
	
	public Transform recordIcon;

	public bool isSavingToDrive;

	//public CamCaptureControler camCaptureControler;
	private bool isRecording = false; 
	private bool isMaxTime = false;
	
	//public int maxTimeSeconds = 15; think I dont need this anymore

	// Use this for initialization
	void Start () {

		framerate = 30.0f;
		imageQuality = 75;
		recording = false;
		recVideo = false;
		recAudio = false;
		tempVsync = QualitySettings.vSyncCount;

		isSavingToDrive = false;

		if(recordIcon != null) recordIcon.gameObject.SetActive(false);

		InitVideoAudio();
	}
	
	// Update is called once per frame
	void Update () {
		if (recVideo) {
			if (!recording) StartCapture();
		}else {
			if (recording) StopCapture();
		}
		CamVideoCapture.recOutput = recAudio;  
	}

	// FixedUpdate for physics and stuff
	void FixedUpdate () {
		if (recording) {
			if (Time.fixedTime > capTime + (1/framerate)) {  
				capTime = Time.fixedTime;
				//StartCoroutine(baKnoCapture.AddFrame());
				StartCoroutine(CamVideoCapture.AddFrameWithRenderTexture(renderTexture));
				//StartCoroutine(CamVideoCapture.AddFrame());
				/*
				if(recMainCam) {
					StartCoroutine(CamVideoCapture.AddFrame());
				} else {
					StartCoroutine(CamVideoCapture.AddFrameWithRenderTexture(renderTexture));
				}
				*/
			}
		}
	}

	private void SetFilePatch() {
		
		string dateString = DateTime.Now.ToString("MMddyyyy_hmmss");
		
		//Debug.Log(dateString);
		
		
		if(saveToDesktop) {
			if ((Application.platform == RuntimePlatform.WindowsPlayer) || (Application.platform == RuntimePlatform.WindowsEditor)) {
				filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "\\SonicMM_" + dateString + ".avi";
			} else {
				filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/SonicMM_" + dateString + ".avi";
			}
		} else {
			string directorLocation = Application.dataPath;
			//Debug.Log("directorLocation " + directorLocation);
			
			if ((Application.platform == RuntimePlatform.WindowsPlayer) || (Application.platform == RuntimePlatform.WindowsEditor)) {
				filePath = directorLocation + "\\SonicMM_" + dateString + ".avi";
			} else {
				filePath = directorLocation + "/SonicMM_" + dateString + ".avi";
			}
			
			
		}
		
	}
	

	private void StartCapture() {
		Debug.Log("Capture start");
		//CamVideoCapture.ClearVideoData();
		//InitVideoAudio();

		if(recordIcon != null) recordIcon.gameObject.SetActive(true);

		recording = true;
		QualitySettings.vSyncCount = 0;
	}

	private void StopCapture() {
		Debug.Log("Capture stop");

		if(recordIcon != null) recordIcon.gameObject.SetActive(false);

		recording = false;
		QualitySettings.vSyncCount = tempVsync;
	}

	public string SaveMovie() {
		Debug.Log("Save Movie");
		isSavingToDrive = true;

		SetFilePatch();
		byte[] bin = CamVideoCapture.GetBinary();
		File.WriteAllBytes(filePath, bin);

		isSavingToDrive = false;

		return filePath;
	}

	public void InitVideoAudio() {
		// set up recorder

		CamVideoCapture.PrepareAudio();
		
		//CamVideoCapture.PrepareVideo(imageQuality);
		CamVideoCapture.PrepareVideoWithRenderTexture(imageQuality, renderTexture);
		/*
		if(recMainCam) {
			CamVideoCapture.PrepareVideo(imageQuality);
		} else {
			CamVideoCapture.PrepareVideoWithRenderTexture(imageQuality, renderTexture);
		}
		*/
		
		CamVideoCapture.ClearVideoData();
	}

}
