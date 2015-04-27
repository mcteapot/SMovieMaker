using UnityEngine;
using System.Collections;

public class Desirable : MonoBehaviour 
{
	private MovieMakerGameController GameController;

	public float MaxDistance = 5f;
	public float AttractionPower = 100f;

	protected SonicController NearestCharacter;

	void Start()
	{
		GameController = FindObjectOfType<MovieMakerGameController>();
	}

	protected virtual void FixedUpdate()
	{
		var sqrMag = MaxDistance * MaxDistance;
		var lowest = Mathf.Infinity;
		NearestCharacter = null;

		foreach(var sonic in GameController.CurrentPlayers)
		{
			if(sonic == null)
			{
				continue;
			}

			var sqrMagnitude = Vector3.SqrMagnitude(sonic.Head.transform.position - transform.position);

			if(sqrMagnitude < lowest)
			{
				lowest = sqrMagnitude;
				NearestCharacter = sonic;
			}

			if(sqrMagnitude <= sqrMag)
			{
				sonic.AttractTo(gameObject, AttractionPower);
				Debug.DrawLine(sonic.Head.transform.position, transform.position, Color.red);
			}
		}
	}
}
