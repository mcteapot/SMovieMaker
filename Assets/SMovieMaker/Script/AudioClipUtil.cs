using UnityEngine;
using System.Collections;

public static class AudioClipUtil 
{
	public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 point, float pitch = 1f)
	{
		var source = new GameObject("Audio").AddComponent<AudioSource>();
		source.clip = clip;
		source.pitch = pitch;
		GameObject.Destroy(source.gameObject, clip.length / pitch);
		source.Play();
		return source;
	}
}
