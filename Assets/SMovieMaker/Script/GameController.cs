using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour 
{
	public bool ForceAIMode = false;

	public SonicController[] CharacterPrefabs;

	public SonicController[] CurrentPlayers;

	public int[] CharacterIndexes;

	public const int MAX_PLAYERS = 4;

	public void Awake()
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

	public void NextCharacter(int index)
	{
		CharacterIndexes[index]++;
		if(CharacterIndexes[index] >= CharacterPrefabs.Length)
		{
			CharacterIndexes[index] = 0;
		}
	}

	public void SpawnPlayer(int index)
	{
		var currentOne = CurrentPlayers[index];

		if(currentOne != null)
		{
			Destroy(currentOne.gameObject);
		}

		var prefab = CharacterPrefabs[CharacterIndexes[index]];
		var newobj = Instantiate(prefab, GetSpawnPos(), Quaternion.identity) as SonicController;

		CurrentPlayers[index] = newobj;

		newobj.SetControllerIndex(index+1);

		if(ForceAIMode)
		{
			newobj.AIMode = SonicController.AIType.Normal;
			newobj.AISpeed = Random.Range(.8f, 4f);
			if(Random.value > .5f) newobj.AISpeed *= -1f;
		}
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
