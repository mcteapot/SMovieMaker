using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class CamRecordController : MonoBehaviour {
	
	private bool recording;
	public bool recVideo;
	public bool recAudio;

	public bool saveToDesktop;
	public bool recMainCam;
	
	private static string filePath;
	
	private int tempVsync;
	private float capTime;
	
	private int imageQuality;
	private float framerate;
	
	public RenderTexture renderTexture;

	public Transform recordIcon;
	
	// Use this for initialization
	void Start () {
		
		framerate = 30.0f;
		imageQuality = 75;
		recording = false;
		recVideo = false;
		recAudio = false;
		tempVsync = QualitySettings.vSyncCount;

		recordIcon.gameObject.SetActive(false);

		SetFilePatch(); 

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
			//Debug.Log("Adding Frame");
			if (Time.fixedTime > capTime + (1/framerate)) {  
				capTime = Time.fixedTime;
				//Debug.Log("Add Frame");
				//baKnoCapture.AddFrame();
				if(recMainCam) {
					StartCoroutine(CamVideoCapture.AddFrame());
				} else {
					StartCoroutine(CamVideoCapture.AddFrameWithRenderTexture(renderTexture));
				}
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

	private void InitVideoAudio() {
		// set up recorder


		CamVideoCapture.PrepareAudio();
		
		if(recMainCam) {
			CamVideoCapture.PrepareVideo(imageQuality);
		} else {
			CamVideoCapture.PrepareVideoWithRenderTexture(imageQuality, renderTexture);
		}

		CamVideoCapture.ClearVideoData();
	}
	
	private void StartCapture() {

		Debug.Log("Capture start");

		if(recordIcon != null) {
			recordIcon.gameObject.SetActive(true);
		}

		recording = true;
		QualitySettings.vSyncCount = 0;
	}
	
	private void StopCapture() {

		Debug.Log("Capture stop");

		if(recordIcon != null) {
			recordIcon.gameObject.SetActive(false);
		}

		recording = false;
		QualitySettings.vSyncCount = tempVsync;

		//SaveMovie(); 
	}
	
	public void SaveMovie() {

		Debug.Log("Save Movie");

		//SetFilePatch(); 

		byte[] bin = CamVideoCapture.GetBinary();
		File.WriteAllBytes(filePath, bin);

		//CamVideoCapture.ClearVideoData();
	}
	
}
