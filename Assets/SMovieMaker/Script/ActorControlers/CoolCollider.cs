using System;
using UnityEngine;
using System.Collections;

public class CoolCollider : MonoBehaviour 
{

	public Action<Collision> OnCollisionAction;

	public void OnCollisionEnter(Collision collision)
	{
		var otherTag = collision.collider.tag;

		if((otherTag.StartsWith("Player") && otherTag != gameObject.tag) || otherTag.StartsWith("Desirable"))
		{
			if(OnCollisionAction != null)
			{
				OnCollisionAction(collision);
			}
		}
	}
}
