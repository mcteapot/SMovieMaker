using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class ActorCommandUI : MonoBehaviour 
{
	[SerializeField] private Camera _camera;
	[SerializeField] private MovieMakerGameController gameController;
	[SerializeField] private LayerMask _mask;

	[SerializeField] private GameObject _commandsUIParent;
	[SerializeField] private Text _characterNameText;

	//[SerializeField] private Text _

	void Update()
	{
		var aimedChars = GetAimedAtCharacters();

		SetUIForCharacters(aimedChars);

		foreach(var sonic in aimedChars)
		{
			InputCheckForCharacter(sonic);
		}
	}

	private void SetUIActive(bool isActive)
	{
		_commandsUIParent.SetActive(isActive);
	}

	private List<SonicController> GetAimedAtCharacters()
	{
		var list = new List<SonicController>();

		var hits = Physics.SphereCastAll(_camera.ViewportPointToRay(Vector3.one * .5f), .25f, Mathf.Infinity, _mask);

		foreach(var hit in hits)
		{
			var sonic = hit.collider.GetComponentInParent<SonicController>();
			if(sonic != null && !list.Contains(sonic))
			{
				list.Add(sonic);
			}
		}

		return list;
	}

	private void InputCheckForCharacter(SonicController character)
	{
		if(character == null)
		{
			return;
		}

		if(Input.GetKeyDown(KeyCode.Z))
		{
			character.PlayRandomVoiceClip();
			character.TellSlowDown();
		}
		if(Input.GetKeyDown(KeyCode.X))
		{
			character.PlayRandomVoiceClip();
			character.TellSpeedUp();
		}
		if(Input.GetKeyDown(KeyCode.C))
		{
			character.IsLimp = !character.IsLimp;
		}
	}

	private void SetUIForCharacters(List<SonicController> characters)
	{
		SetUIActive(characters != null && characters.Count > 0);

		var str = string.Empty;
		foreach(var sonic in characters)
		{
			str += sonic.name.Replace("(Clone)","") + " ";
		}

		_characterNameText.text = str.TrimEnd(' ') + ":";
	}
}
