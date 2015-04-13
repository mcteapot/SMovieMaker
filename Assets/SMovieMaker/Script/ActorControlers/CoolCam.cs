using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CoolCam : MonoBehaviour 
{
	public MovieMakerGameController GameController;
	public Transform CenterPoint;

	void Start()
	{
		GameController = GameController ?? FindObjectOfType<MovieMakerGameController>();
	}

	void LateUpdate()
	{
		var targets = new List<SonicController>();
		foreach(var cont in GameController.CurrentPlayers)
		{
			if(cont != null)
			{
				targets.Add(cont);
			}
		}

		var avePos = CenterPoint.position;

		foreach(var target in targets)
		{
			avePos += target.MainRigidBody.worldCenterOfMass;
		}

		avePos /= (targets.Count + 1);

		LookAtPoint(avePos);
	}

	public void LookAtPoint(Vector3 point)
	{
//		transform.LookAt(point, Vector3.up);
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(point - transform.position, Vector3.up), Time.deltaTime * 10f);
	}
}
