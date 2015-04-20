using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SubtitleController : MonoBehaviour 
{
	public InputField InputField;
	public GUIText SubtitleText;

	public void Awake()
	{
		InputField.onEndEdit.AddListener(EndInput);
		InputField.onValueChange.AddListener(InputChanged);
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Return))
		{
			if(InputField.isFocused == false)
			{
				InputField.transform.parent.gameObject.SetActive(true);
				EventSystem.current.SetSelectedGameObject(InputField.gameObject);
			}
			else
			{
				EndInput("");
			}
		}
	}

	private void InputChanged(string text)
	{
		SubtitleText.text = InputField.text.TrimEnd('\n');
	}

	private void EndInput(string text)
	{
//		EventSystem.current.SetSelectedGameObject(null);
		InputField.transform.parent.gameObject.SetActive(false);
	}
}
