using UnityEngine;
using System.Collections;

public class FilterController : MonoBehaviour 
{
	public MonoBehaviour[] ImageEffects;
	private int _imageEffectIndex;

	void Awake()
	{
		_imageEffectIndex = ImageEffects.Length;
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.F))
		{
			NextImageEffect();
		}
	}

	public void NextImageEffect()
	{
		if(_imageEffectIndex < ImageEffects.Length)
		{
			ImageEffects[_imageEffectIndex].enabled = false;
		}

		_imageEffectIndex++;
		if(_imageEffectIndex != ImageEffects.Length)
		{
			if(_imageEffectIndex > ImageEffects.Length)
			{
				_imageEffectIndex = 0;
			}
			ImageEffects[_imageEffectIndex].enabled = true;
		}
	}
}
