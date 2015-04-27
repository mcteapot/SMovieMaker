using UnityEngine;
using System.Collections;

public class Desirable : MonoBehaviour 
{
	private MovieMakerGameController GameController;

	public float MaxDistance = 5f;
	public float AttractionPower = 100f;

	void Start()
	{
		GameController = FindObjectOfType<MovieMakerGameController>();
	}

	private void FixedUpdate()
	{
		var sqrMag = MaxDistance * MaxDistance;
		foreach(var sonic in GameController.CurrentPlayers)
		{
			if(sonic == null)
			{
				continue;
			}

			if(Vector3.SqrMagnitude(sonic.Head.transform.position - transform.position) <= sqrMag)
			{
				sonic.AttractTo(gameObject, AttractionPower);
				Debug.DrawLine(sonic.Head.transform.position, transform.position, Color.red);
			}
		}
	}
}
