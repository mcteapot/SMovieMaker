using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MovieMakerMenu : MonoBehaviour 
{
	public class MovieMakerLevelResult
	{
		public bool Successful { get; set; }
		public string FilePath { get; set; }

		public MovieMakerLevelResult(bool successful, string filePath)
		{
			Successful = successful;
			FilePath = filePath;
		}
	}
	
	[Serializable]
	public class MovieMakerScenario
	{
		public string Name = "New Scenario";
		public SonicController[] Characters;
		public float VideoLength = 12f;

		public Action<MovieMakerLevelResult> OnComplete { get; set; }
	}

	public MovieMakerScenario[] Scenarios;
	public MovieMakerLevelButton[] Buttons;

	// for reading after scene load
	public static MovieMakerScenario NextScenario { get; private set; }

	private void Awake()
	{
		MovieMakerSaveManager.SetLevelUnlocked(1, true);

		SetButtonStates();
	}

	private void SetButtonStates()
	{
		foreach(var button in Buttons)
		{
			button.SetUnlocked(IsLevelUnlocked(button.LevelNumber));
			button.ClickedAction = LevelButtonPressed;
		}
	}

	private static bool IsLevelUnlocked(int levelNumber)
	{
		return MovieMakerSaveManager.IsLevelUnlocked(levelNumber);
	}

	public void LevelButtonPressed(int levelNumber)
	{
		StartLevel(levelNumber);
	}

	private void StartLevel(int levelNumber)
	{
		var scenario = Scenarios[levelNumber -1];

		scenario.OnComplete = (result) =>
		{
			if(result.Successful)
			{
				MovieMakerSaveManager.SetLevelCompleted(levelNumber);
				MovieMakerSaveManager.SetLevelUnlocked(levelNumber + 1);

				var path = "file://" + result.FilePath;

				Application.OpenURL(path);
				Debug.Log(path);
			}
			else
			{

			}

			Screen.lockCursor = false;
		};

		StartLevel(scenario);
	}

	private void StartLevel(MovieMakerScenario scenario)
	{
		/// this is a dumb hack
		NextScenario = scenario;

		// TODO: change this later
		Application.LoadLevel("Sonic_Room01");
	}

	public void ClearSaveData()
	{
		MovieMakerSaveManager.ClearAllLevelData(Buttons.Length);
		MovieMakerSaveManager.SetLevelUnlocked(1);
		SetButtonStates();
	}
}

public static class MovieMakerSaveManager
{
	private const string LevelUnlockedKeyFormat = "SMM_LU_{0}";
	private const string LevelCompletedKeyFormat = "SMM_LC_{0}";


	private static string GetLevelUnlockedKey(int levelNumber)
	{
		return string.Format(LevelUnlockedKeyFormat, levelNumber);
	}

	public static bool IsLevelUnlocked(int levelNumber)
	{
		return PlayerPrefs.GetInt(GetLevelUnlockedKey(levelNumber), 0) > 0;
	}

	public static void SetLevelUnlocked(int levelNumber, bool unlocked = true)
	{
		PlayerPrefs.SetInt(GetLevelUnlockedKey(levelNumber), unlocked ? 1 : 0);
	}


	private static string GetLevelCompletedKey(int levelNumber)
	{
		return string.Format(LevelCompletedKeyFormat, levelNumber);
	}

	public static bool IsLevelCompleted(int levelNumber)
	{
		return PlayerPrefs.GetInt(GetLevelCompletedKey(levelNumber), 0) > 0;
	}
	
	public static void SetLevelCompleted(int levelNumber, bool completed = true)
	{
		PlayerPrefs.SetInt(GetLevelCompletedKey(levelNumber), completed ? 1 : 0);
	}

	public static void ClearAllLevelData(int maxLevelNumber)
	{
		for(int i=1;i<=maxLevelNumber;i++)
		{
			ClearLevelData(i);
		}
	}

	public static void ClearLevelData(int levelNumber)
	{
		PlayerPrefs.DeleteKey(GetLevelCompletedKey(levelNumber));
		PlayerPrefs.DeleteKey(GetLevelUnlockedKey(levelNumber));
	}
}












































// fuck monodevelop not scrolling past the end of text