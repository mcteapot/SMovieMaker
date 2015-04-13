using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

//[ExecuteInEditMode]
public class MovieMakerLevelButton : MonoBehaviour 
{
	public Button Button;
	public Text Text;

	public int LevelNumber = -1;

	public Action<int> ClickedAction;

	public void Awake()
	{
		if(Button == null)
		{
			Button = GetComponent<Button>();
		}

		if(Text == null)
		{
			Text = GetComponentInChildren<Text>();
		}

		if(LevelNumber == -1)
		{
			if(name.StartsWith("Level"))
			{
				var numberText = name.Replace("Level", "");

				int number;
				if(int.TryParse(numberText, out number))
				{
					LevelNumber = number;
				}
			}
		}

		if(Application.isPlaying)
		{
			Button.onClick.AddListener(Clicked);
		}
	}

	public void SetUnlocked(bool unlocked)
	{
		Button.interactable = unlocked;
	}

	private void Clicked()
	{
		if(ClickedAction != null)
		{
			ClickedAction(LevelNumber);
		}
	}
}












































// fuck monodevelop not scrolling past the end of text