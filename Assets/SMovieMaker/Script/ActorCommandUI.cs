using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ActorCommandUI : MonoBehaviour 
{
	[SerializeField] private Camera _camera;
	[SerializeField] private GameController gameController;
	[SerializeField] private LayerMask _mask;

	[SerializeField] private GameObject _commandsUIParent;
	[SerializeField] private Text _characterNameText;

	void Update()
	{
		var aimedChar = GetAimedAtCharacter();

		SetUIForCharacter(aimedChar);
		InputCheckForCharacter(aimedChar);
	}

	private void SetUIActive(bool isActive)
	{
		_commandsUIParent.SetActive(isActive);
	}

	private SonicController GetAimedAtCharacter()
	{
		RaycastHit hit;
//		if(Physics.Raycast(_camera.ViewportPointToRay(Vector3.one * .5f), out hit, Mathf.Infinity, _mask))
		if(Physics.SphereCast(_camera.ViewportPointToRay(Vector3.one * .5f), .08125f, out hit, Mathf.Infinity, _mask))
		{
			return hit.collider.GetComponentInParent<SonicController>();
		}
		return null;
	}

	private void InputCheckForCharacter(SonicController character)
	{
		if(character == null)
		{
			return;
		}

		_characterNameText.text = character.name.Replace("(Clone)","") + ":";

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
	}

	private void SetUIForCharacter(SonicController character)
	{
		SetUIActive(character != null);
		if(character == null)
		{
			return;
		}
	}
}
