#pragma strict
#pragma implicit
#pragma downcast
import System;
import System.IO;


/*
private var recording : boolean;
static var recVideo : boolean;
static var recAudio : boolean;
static var filePath : String;
private var tempVsync : int;
private var capTime : float;
private var imageQuality : int;
private var framerate : float;

function Start () {
	framerate = 30.0;
	imageQuality = 75;
	recording = false;
	recVideo = false;
	recAudio = false;
	tempVsync = QualitySettings.vSyncCount;
	if ((Application.platform == RuntimePlatform.WindowsPlayer) || (Application.platform == RuntimePlatform.WindowsEditor))
		filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "\\SampleMovie.avi";
	else
		filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory) + "/SampleMovie.avi";
	baKnoCapture.PrepareAudio();
	baKnoCapture.PrepareVideo(imageQuality);
}

function Update () {
	if (recVideo) {
		if (!recording) StartCapture();
	}else{
		if (recording) StopCapture();
	}
	baKnoCapture.recOutput = recAudio;  
}

function FixedUpdate () {
	if (recording) {
		if (Time.fixedTime > capTime + (1/framerate)) {  
			capTime = Time.fixedTime;
			baKnoCapture.AddFrame();
		}
	}
}

function StartCapture(){
	recording = true;
	QualitySettings.vSyncCount = 0;
}

function StopCapture(){
	recording = false;
	QualitySettings.vSyncCount = tempVsync;
	SaveMovie();
}

static function SaveMovie(){
	var bin : byte[] = baKnoCapture.GetBinary();
	File.WriteAllBytes(filePath, bin);
}
*/