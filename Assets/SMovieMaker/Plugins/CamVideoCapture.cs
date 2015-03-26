using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class CamVideoCapture : MonoBehaviour {	
	static int videoRate;
	static int microSecs;
	static int frameWidth;
	static int frameHeight;
	static System.Text.UTF8Encoding Encoding = new System.Text.UTF8Encoding();
	static int audioRate;
	static public bool recOutput;
	static List<byte> byteFrames = new List<byte>(); 
	static List<byte> byteSamples = new List<byte>();
	static int frameIndex = -1;
	static List<List<byte>> byteFrame = new List<List<byte>>(); 
	static List<List<byte>> byteSample = new List<List<byte>>();
	static Texture2D tex;
	static List<int> sizes = new List<int>();
	static List<int> padds = new List<int>();
	static int biggestFrame = 0;
	static public bool grabbing = false;
	static float totalTime;
	static int imageQuality = 50;


	// INIT METHODS
	static public void PrepareAudio(){
		audioRate = AudioSettings.outputSampleRate;
		//Debug.Log("audioRate " + audioRate);
		//int bufferSize;
		//int numBuffers;
		//AudioSettings.GetDSPBufferSize(out bufferSize, out numBuffers);
	}
	
	static public void PrepareVideo(int qlty){

		imageQuality = qlty;
     	frameWidth = Screen.width;
	    frameHeight = Screen.height;
		tex = new Texture2D (frameWidth, frameHeight, TextureFormat.RGB24, false);
	}

	static public void ClearVideoData() {
		byteFrames = new List<byte>(); 	
		byteSamples = new List<byte>();

		frameIndex = -1;

		grabbing = false;

		byteFrame = new List<List<byte>>(); 
		byteSample = new List<List<byte>>();

		sizes = new List<int>();
		padds = new List<int>();
	}

	static public void PrepareVideoWithRenderTexture(int qlty, RenderTexture newRenderTexture){
		imageQuality = qlty;
		frameWidth = newRenderTexture.width;
		frameHeight = newRenderTexture.height;
		tex = new Texture2D (frameWidth, frameHeight, TextureFormat.RGB24, false);
	}
	
	void OnAudioFilterRead (float[] data, int channels) {
    	if(recOutput) ConvertAndWrite(data); 
	}
	
	void ConvertAndWrite(float[] dataSource) { 
	    short[] intData = new short[dataSource.Length];
	    byte[] bytesData = new byte[dataSource.Length*2];
	    int rescaleFactor = 32767;
	    for (int i = 0; i<dataSource.Length; i++) {
	        intData[i] = System.Convert.ToInt16(Mathf.Clamp(dataSource[i]*rescaleFactor, -32768, 32767)); 
	        byte[] byteArr = new byte[2];
	        byteArr = System.BitConverter.GetBytes(intData[i]);
	        byteArr.CopyTo(bytesData,i*2);
	    }
	    byteSamples.AddRange(bytesData);
	}


	// ADD FRAME TO CAPTURE
	static public IEnumerator AddFrame() {
		//Debug.Log("AddFrame");

		if(!grabbing) {
			yield return new WaitForEndOfFrame();
			grabbing = true;
			if (tex) {
				frameIndex = frameIndex + 1;
				byteFrame.Add(new List<byte>());
		    	tex.ReadPixels (new Rect(0, 0, frameWidth, frameHeight), 0, 0, false); 
		     	byte[] frame = tex.EncodeToJPG(imageQuality);
		     	byteFrame[frameIndex].AddRange(System.BitConverter.GetBytes(1667510320));
		     	sizes.Add(frame.Length);
		     	byteFrame[frameIndex].AddRange(System.BitConverter.GetBytes(frame.Length)); 
				byteFrame[frameIndex].AddRange(frame);
				if (frame.Length%2==1) { 
					byteFrame[frameIndex].Add((byte) 0);
					padds.Add(1);
				}else{
					padds.Add(0);
				}
				if (frame.Length > biggestFrame) biggestFrame = frame.Length;
			} else {
				Debug.LogWarning("tex not defined");
			}
			grabbing = false;
		}
	}

	static public IEnumerator AddFrameWithRenderTexture(RenderTexture newRenderTexture) {
		//Debug.Log("AddFrame");

		if(!grabbing) {
			yield return new WaitForEndOfFrame();
			grabbing = true;
			if (tex) {
				frameIndex = frameIndex + 1;
				//Debug.Log("RECORDING " + frameIndex);
				byteFrame.Add(new List<byte>()); 
				RenderTexture.active = newRenderTexture;
				tex.ReadPixels(new Rect(0, 0, frameWidth, frameHeight), 0, 0);
				tex.Apply();
				byte[] frame = tex.EncodeToJPG(imageQuality);
				byteFrame[frameIndex].AddRange(System.BitConverter.GetBytes(1667510320));
				sizes.Add(frame.Length);
				byteFrame[frameIndex].AddRange(System.BitConverter.GetBytes(frame.Length));
				byteFrame[frameIndex].AddRange(frame);
				if (frame.Length%2==1) { 
					byteFrame[frameIndex].Add((byte) 0);
					padds.Add(1);
				}else{
					padds.Add(0);
				}
				if (frame.Length > biggestFrame) biggestFrame = frame.Length;

				RenderTexture.active = null; 
			} else {
				Debug.LogWarning("tex not defined");
			}
			grabbing = false;
		}
	}	
	
	static public byte[] GetBinary(){ 
		int audioChunksize;
		Destroy(tex);
		audioChunksize = (byteSamples.Count / frameIndex);
		if ((audioChunksize % 4) > 0) audioChunksize = audioChunksize + 4 - (audioChunksize % 4);
		for(int i = 0; i < frameIndex; i++) {
			byteSample.Add(new List<byte>());
			byteSample[i].AddRange(System.BitConverter.GetBytes(1651978544));
			if ((i+1)*audioChunksize <= byteSamples.Count){
				byteSample[i].AddRange(System.BitConverter.GetBytes(audioChunksize));
				byteSample[i].AddRange(byteSamples.GetRange(i*audioChunksize, audioChunksize));
			}else{
				if(byteSamples.Count > (i*audioChunksize)){
					byteSample[i].AddRange(System.BitConverter.GetBytes(byteSamples.Count - (i*audioChunksize)));
					byteSample[i].AddRange(byteSamples.GetRange(i*audioChunksize, byteSamples.Count - (i*audioChunksize)));
				}
			}
		}
		for(int i = 0; i < frameIndex; i++) {
			byteFrames.AddRange(byteFrame[i]);
			byteFrames.AddRange(byteSample[i]);
		}
		//Debug.Log("byteSamples.Count " + byteSamples.Count);
		totalTime = 1.0f * byteSamples.Count / (4 * audioRate);
		//Debug.Log("frameIndex " + frameIndex);
		//Debug.Log("totalTime " + totalTime);
		videoRate = Mathf.RoundToInt(frameIndex/totalTime);
		//Debug.Log("videoRate " + videoRate);
		microSecs = Mathf.RoundToInt(1000000 / videoRate);
		List<byte> byteList = new List<byte>();
		byteList.AddRange(Encoding.GetBytes("RIFF"));	
		byteList.AddRange(System.BitConverter.GetBytes(byteFrames.Count + (32 * frameIndex) + 326));
		byteList.AddRange(Encoding.GetBytes("AVI "));
		byteList.AddRange(Encoding.GetBytes("LIST"));	
		byteList.AddRange(System.BitConverter.GetBytes(294));
		byteList.AddRange(Encoding.GetBytes("hdrl"));	
		byteList.AddRange(Encoding.GetBytes("avih"));	
		byteList.AddRange(System.BitConverter.GetBytes(56));  
		byteList.AddRange(System.BitConverter.GetBytes(microSecs));
		byteList.AddRange(System.BitConverter.GetBytes(biggestFrame * videoRate + 5880));
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(272));	
		byteList.AddRange(System.BitConverter.GetBytes(frameIndex));
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(2));		
		byteList.AddRange(System.BitConverter.GetBytes(biggestFrame + 5880));
		byteList.AddRange(System.BitConverter.GetBytes(frameWidth));	
		byteList.AddRange(System.BitConverter.GetBytes(frameHeight));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
        byteList.AddRange(Encoding.GetBytes("LIST"));		 
        byteList.AddRange(System.BitConverter.GetBytes(116));	
        byteList.AddRange(Encoding.GetBytes("strl"));	
        byteList.AddRange(Encoding.GetBytes("strh"));	 
        byteList.AddRange(System.BitConverter.GetBytes(56));
        byteList.AddRange(Encoding.GetBytes("vids"));	
        byteList.AddRange(Encoding.GetBytes("MJPG"));
        byteList.AddRange(System.BitConverter.GetBytes(0));	
        byteList.AddRange(System.BitConverter.GetBytes(0));	
        byteList.AddRange(System.BitConverter.GetBytes(0));	
        byteList.AddRange(System.BitConverter.GetBytes(1000));
        byteList.AddRange(System.BitConverter.GetBytes(videoRate * 1000));	
        byteList.AddRange(System.BitConverter.GetBytes(0));		
        byteList.AddRange(System.BitConverter.GetBytes(frameIndex));	
        byteList.AddRange(System.BitConverter.GetBytes(biggestFrame));	
        byteList.AddRange(System.BitConverter.GetBytes(0));		
        byteList.AddRange(System.BitConverter.GetBytes(0));	
        byteList.AddRange(System.BitConverter.GetBytes(0));		
        byteList.AddRange(ByteShort(frameWidth));	
        byteList.AddRange(ByteShort(frameHeight));			
		byteList.AddRange(Encoding.GetBytes("strf"));
		byteList.AddRange(System.BitConverter.GetBytes(40));
		byteList.AddRange(System.BitConverter.GetBytes(40));
		byteList.AddRange(System.BitConverter.GetBytes(frameWidth));
		byteList.AddRange(System.BitConverter.GetBytes(frameHeight));
		byteList.AddRange(ByteShort(1));	
		byteList.AddRange(ByteShort(24));	
		byteList.AddRange(Encoding.GetBytes("MJPG"));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
        byteList.AddRange(Encoding.GetBytes("LIST"));	
        byteList.AddRange(System.BitConverter.GetBytes(94));
        byteList.AddRange(Encoding.GetBytes("strl"));	
        byteList.AddRange(Encoding.GetBytes("strh"));	
		byteList.AddRange(System.BitConverter.GetBytes(56)); 
		byteList.AddRange(Encoding.GetBytes("auds"));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		byteList.AddRange(System.BitConverter.GetBytes(1));	
		byteList.AddRange(System.BitConverter.GetBytes(audioRate));	
		byteList.AddRange(System.BitConverter.GetBytes(0));	
		int tempIntt = byteSamples.Count / 4;
		byteList.AddRange(System.BitConverter.GetBytes(tempIntt));
		byteList.AddRange(System.BitConverter.GetBytes(audioChunksize));
		byteList.AddRange(System.BitConverter.GetBytes(4294967295)); 
		byteList.AddRange(System.BitConverter.GetBytes(4));	
		byteList.AddRange(System.BitConverter.GetBytes(0));
		byteList.AddRange(System.BitConverter.GetBytes(0));
		byteList.AddRange(Encoding.GetBytes("strf"));
		byteList.AddRange(System.BitConverter.GetBytes(18)); 
		byteList.AddRange(ByteShort(1));
		byteList.AddRange(ByteShort(2));
		byteList.AddRange(System.BitConverter.GetBytes(audioRate));
		byteList.AddRange(System.BitConverter.GetBytes(audioRate * 4));
		byteList.AddRange(ByteShort(4));
		byteList.AddRange(ByteShort(16));
		byteList.AddRange(ByteShort(0));
		byteList.AddRange(Encoding.GetBytes("LIST"));
		byteList.AddRange(System.BitConverter.GetBytes(byteFrames.Count + 4));
        byteList.AddRange(Encoding.GetBytes("movi"));
        byteList.AddRange(byteFrames);
        byteList.AddRange(Encoding.GetBytes("idx1"));
        byteList.AddRange(System.BitConverter.GetBytes(frameIndex * 32));
        int tempInt = 4;
		for(int i = 0; i < frameIndex; i++) {
	        byteList.AddRange(System.BitConverter.GetBytes(1667510320));
	        byteList.AddRange(System.BitConverter.GetBytes(16));
	        if (i > 0) tempInt = tempInt + audioChunksize + 8;
	        byteList.AddRange(System.BitConverter.GetBytes(tempInt));
	        byteList.AddRange(System.BitConverter.GetBytes(sizes[i]));
			byteList.AddRange(System.BitConverter.GetBytes(1651978544));
	        byteList.AddRange(System.BitConverter.GetBytes(16));
	        tempInt = tempInt + sizes[i] + padds[i] + 8;
	        byteList.AddRange(System.BitConverter.GetBytes(tempInt));
	        byteList.AddRange(System.BitConverter.GetBytes(audioChunksize));
		}
		byte[] BinAvi = new byte[byteList.Count];
		for(int i = 0; i < byteList.Count; i++) BinAvi[i] = byteList[i];
		byteFrames = null;
		byteSamples = null;
		return BinAvi;
	}

	static List<byte> ByteShort(int largo) {
		List<byte> result = new List<byte>();
		byte[] num = System.BitConverter.GetBytes(largo);
		result.Add((byte) num[0]);
		result.Add((byte) num[1]);
		return result;
	}	
}