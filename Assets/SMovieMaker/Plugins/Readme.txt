               b a K n o' s     M o v i e    E x p o r t 


INTRO

Movie Export works on Unity version 4.5 or higher, for any platform.

Movie Export generates an AVI movie file, encoded as Motion-JPG with uncompressed audio. This movie file format is almost universal. It plays on Windows, Mac and Linux without any additional software required.



HOW TO USE IT

Make sure the baKnoCapture script is located within the Plugins folder or the Standard Assets folder.

Place the baknoCapture script within the object which has the Audio Listener component in your scene (usually the main camera).

Easy way:

1- Call PrepareAudio() to initiallize the audio recorder.

2- Call PrepareVideo(Qlty) to initiallize the video recorder. Qlty is the image quality similar to JPG quality. Range within 1 anf 100, 100 being the highest quality but creates a bigger file size.

3- Set recOutput to record audio samples. False does not record, True records audio samples.

4- Video is recorded by grabbing single frames. Call AddFrame() to grab a new frame and add it to the video stream.

5- Call GetBinary() to generate the binary file. Usually to save it or upload it.

Easiest Way:

Use the CaptureControl script placing it into any object within your scene. And check the sample scripts included.

1- Set CaptureControl.recAudio to control when to record audio

2- Set CaptureControl.recVideo to control when to record video and save the video file.

You can modify CaptureControl to save the movie file with a different name and into a different location, or to upload it instead.



WHEN TO USE IT

You can use Movie Export in different ways. Recording audio and video at the same time, or separately. Usually audio first and then video. When finished recording audio and video, both are mixed into a single movie. This is useful in Replays where audio is recorded during playtime, but video during Replay running slower to grab more frames.



IMPORTANT

Final video duration is defined by the audio stream recorded. Video frames are evenly distributed within the audio, therefore FPS is defined at the end by the total number of video frames divided by the audio time recorded.

Grabbing and encoding video frames is a processor intensive task, which requires more processing power as the window size is increased. Simple scenes as the one provided can record regular frame-size videos at 30FPS, but most likely your comlex game scene will not. In this case you will have to either implement a Replay functionality or try to grab at an smaller window size.

Be carefull on the amount of time you capture video and audio, not only because the exported movie file might get very high, but also because the amount of memory required internally wiil increase proportinally as well.