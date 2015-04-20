using UnityEngine;
using System.Collections;
using System;

public class MovieMakerScenario : MonoBehaviour
{
	public string Name = "New Scenario";
	public CharacterSetup[] Characters;
	public float VideoLength = 12f;

	public Action<MovieMakerMenu.MovieMakerLevelResult> OnComplete { get; set; }

	[Serializable]
	public class CharacterSetup
	{
		public SonicController Character;
		public float Scale = 1f;

		public CharacterSetup()
		{
			Scale = 1f;
		}
	}
}
