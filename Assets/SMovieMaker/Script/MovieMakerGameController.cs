using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class MovieMakerGameController : MonoBehaviour 
{
	public bool ForceAIMode = false;

	public SonicController[] CharacterPrefabs;
	public SonicController[] CurrentPlayers;

	public int[] CharacterIndexes;

	public const int MAX_PLAYERS = 4;

	public ControllerCamera ControllerCamera;

	private MovieMakerScenario Scenario;

	private Action<MovieMakerMenu.MovieMakerLevelResult> LevelFinished;

	public void Awake()
	{
		var scenario = MovieMakerMenu.NextScenario;

		if(scenario == null)
		{
			OLD_Awake();
		}
		else
		{
			LoadScenario(scenario);
		}
	}

	// be careful, does not clean up any existing players
	private void LoadScenario(MovieMakerScenario scenario)
	{
		Scenario = scenario;
		LevelFinished = scenario.OnComplete;

		// TODO: would be better without rounding to int
		ControllerCamera.maxTimeSeconds = Mathf.FloorToInt(scenario.VideoLength);

		ControllerCamera.SaveCompleted += OnSaveMovieComplete;

		CurrentPlayers = new SonicController[scenario.Characters.Length];
		//CharacterIndexes = new int[scenario.Characters.Length];

		for(int i=0; i<scenario.Characters.Length; i++)
		{
			SpawnPlayer(i, scenario.Characters[i]);
		}
	}

	private void OnSaveMovieComplete(string filePath)
	{
		Debug.Log("save movie completed");

		if(LevelFinished != null)
		{
			var result = new MovieMakerMenu.MovieMakerLevelResult(true, filePath);

			LevelFinished(result);
		}

		// TODO clean this up
		Application.LoadLevel("MovieMakerMenu");
	}

	public void OLD_Awake()
	{
		CurrentPlayers = new SonicController[MAX_PLAYERS];
		CharacterIndexes = new int[MAX_PLAYERS];

		for(int i=0;i<MAX_PLAYERS;i++)
		{
			CharacterIndexes[i] = i;
		}
	}

	void Update()
	{
		if(Scenario != null)
		{
			return;
		}

		for(int i=0;i<MAX_PLAYERS;i++)
		{
			CheckForSwitch(i);
			CheckForAnyInput(i);
		}
	}

	public void CheckForSwitch(int index)
	{
		if(Input.GetButtonDown("Player"+(index+1)+" Switch"))
		{
			NextAndSpawn(index);
		}
	}

	public void CheckForAnyInput(int index)
	{
		if(CurrentPlayers[index] == null)
		{
			var input = Input.GetAxis(string.Format("Player{0} Vertical", index+1));
			if(!Mathf.Approximately(input, 0f))
			{
				NextAndSpawn(index);
			}
		}
	}

	public void NextAndSpawn(int index)
	{
		NextCharacter(index);
		SpawnPlayer(index);
	}

	public void SetCharacterIndex(int playerIndex, int characterIndex)
	{
		CharacterIndexes[playerIndex] = characterIndex;
	}

	public void NextCharacter(int index)
	{
		CharacterIndexes[index]++;
		if(CharacterIndexes[index] >= CharacterPrefabs.Length)
		{
			CharacterIndexes[index] = 0;
		}
	}

	public SonicController SpawnPlayer(int index)
	{
		var prefab = CharacterPrefabs[CharacterIndexes[index]];
		var newObj = SpawnPlayer(index, prefab);
		return newObj;
	}

	public SonicController SpawnPlayer(int playerIndex, MovieMakerScenario.CharacterSetup characterSetup)
	{
		var player = SpawnPlayer(playerIndex, characterSetup.Character);

		if(Mathf.Approximately(characterSetup.Scale, 1f) == false)
		{
			var scale = player.transform.localScale;
			scale *= characterSetup.Scale;
			player.transform.localScale = scale;

			foreach(var rb in player.GetComponentsInChildren<Rigidbody>())
			{
//				rb.mass = rb.mass * (characterSetup.Scale * characterSetup.Scale);
			}
		}

		return player;
	}

	public SonicController SpawnPlayer(int playerIndex, SonicController characterPrefab)
	{
		var currentOne = CurrentPlayers[playerIndex];
		if(currentOne != null)
		{
			Destroy(currentOne.gameObject);
		}

		var newobj = Instantiate(characterPrefab, GetSpawnPos(), Quaternion.identity) as SonicController;

		CurrentPlayers[playerIndex] = newobj;
		newobj.SetControllerIndex(playerIndex+1);

		if(ForceAIMode)
		{
			newobj.AIMode = SonicController.AIType.Normal;
			newobj.AISpeed = Random.Range(.8f, 4f);
			if(Random.value > .5f) newobj.AISpeed *= -1f;
		}

		return newobj;
	}

	public Vector3 GetSpawnPos()
	{
		return transform.position;
	}

	public Quaternion GetSpawnRotation()
	{
		return Quaternion.identity;
	}

}
