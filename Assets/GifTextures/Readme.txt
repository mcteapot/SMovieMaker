Hello,

Gif2Textures is a Unity plugin which allows you to use Gif as a texture.

First of all, please check the demo http://www.cheng.byethost18.com to get an idea of how it looks like.

It is very easy to use.
------------------
If you are an artist or a designer (I mean if you don't know anything about scripting):
------------------
1. Put your Gif file to Resources folder of your project.
2. Change the extension name of your Gif to "bytes". For example, change "abc.gif" to "abc.bytes" or "abc.gif.bytes". We need to do this is because Unity has its own Gif parser and it will just load the 1st frame of a Gif. So, to be able to load all frames, we have to use "bytes" as extension name to bypass Unity's Gif parser.
3. Add component "Glf Texture" to your game object. Your game object has to have either component "Mesh Renderer" with a valid texture material or component "GUITexture". Otherwise, component "Glf Texture" won't work becasue it couldn't know where to set those textures.
4. On inspector of component "Glf Texture", fill the name of your Gif to the property "Gif File Name". For example, fill "abc" to this property if you have a "abc.bytes" in Resources folder; fill "abc.gif" to this property if you have a "abc.gif.bytes" in Resources folder.
5. This step is optional. You can choose smoother animation or less memory usage by check property "Faster But More Memory Usage" or not. If you check this property, all frames of the Gif will be loaded and be cached at the beginning. If you don't check it, each frame will be load when it need to be display and none of them will be cached. This property is checked as default.
6. That's it. Enjoy!

------------------
If you know something about scripting and you need to use Gif texture in a special way (for example, neither cache all frame at the beginning nor load them over and over is acceptable for you), you can use Gif2Textures.dll directly:
------------------
1. Import namespace Gif2Texture in your script.
2. New a GifFrames instance.
3. Pass the stream of Gif to the GifFrames by function GifFrames.Load().
4. Then you can get textures of every frame and their delay time by call function GifFrames.GetNextFrame() again and again.
5. Now you can use these textures in any way.

You can check GifTexture.cs as an example for how to use Gif2Textures.dll directly. Below is the details of those 2 function of GifFrames:
	// Summary:
	//     Loads a gif stream.
	//
	// Parameters:
	//   stream:
	//     The gif stream
	//
	//   cacheTextures:
	//     true to load all frames from the gif stream, convert them to textures immediately
	//     and catch them.  This way consume more memory, but the animation can be very
	//     smooth; false to just load all frames from the gif stream but not convert
	//     them to textures immediately.  Only 1 frame of gif will be converted to texture
	//     every time you call GetNextFrame. And previous frame will be released.  So
	//     this way consume less memory, but the animation may be no so smooth becasue
	//     we have to generate a new texture and release previous one every time you
	//     call GetNextFrame.
	//
	// Returns:
	//     A value that determines whether the gif loaded successfully or not. If it
	//     reture false, it means the gif stream is incomplete or it isn't a gif at
	//     all.
	public bool Load(Stream stream, bool cacheTextures = true);

	// Summary:
	//     Get a texture and a delay of a frame of the gif. You will get the 1st frame
	//     when 1st time call this function.  You will get 2nd frame if you call this
	//     it again. You will get 1st frame again when you call it the N+1 (N is the
	//     frame number of the gif) time.  So, you can just keep calling this function
	//     after the delay of previous frame to get the texture of next frame and use
	//     it no matter how.
	//
	// Parameters:
	//   texture:
	//     the texture of next frame
	//
	//   delay:
	//     how long the frame should stay
	//
	// Returns:
	//     A value that determines whether you get next frame successfully or not. If
	//     it return false, it means the gif hasn't been loaded successfully.
	public bool GetNextFrame(out Texture2D texture, out float delay);

Thanks for your attention. Don't hesitate to contact me if you have any question or suggestion. Please email to fengcheng0308@gmail.com.

And I'd appreciate it if you could leave your user review to Asset Store.

Thanks in advance!

Cheng