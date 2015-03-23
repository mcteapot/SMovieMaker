using UnityEngine;
using System.Collections;

public class ScoreController : MonoBehaviour {
	
	public static ScoreController Instance;

	public GUIText[] ScoreText;

	public float Score {get; private set;}

	void Awake()
	{
		Instance = this;
	}

	public void AddScore(float amount)
	{
		SetScore(Score + amount);
	}

	public void SetScore(float amount)
	{
		Score = amount;
		UpdateGUI();
	}


	void UpdateGUI()
	{
		var text = Score.ToString() + "%";
		foreach(var guitext in ScoreText)
		{
			guitext.text = text;
		}
	}

}
