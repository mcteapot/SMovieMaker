using UnityEngine;
using System.Collections;

public class Bumper : MonoBehaviour {

	public AudioClip Clip;
	public float force = 100f;
	public ForceMode forceMode = ForceMode.Impulse;

	public Transform RenderObject;

	private Vector3 _renderObjectStartPos;

	void Awake()
	{
		_renderObjectStartPos = RenderObject.localPosition;
	}

	void OnCollisionEnter(Collision collision)
	{
		collision.rigidbody.AddForce(transform.forward * force, forceMode);
		RenderObject.transform.localPosition = Vector3.forward * .3f;

		if(Clip != null)
		{
			AudioSource.PlayClipAtPoint(Clip, collision.contacts[0].point);
		}
	}

	void Update()
	{
		RenderObject.localPosition = Vector3.Lerp(RenderObject.localPosition, _renderObjectStartPos, Time.deltaTime * 8f);
	}
}
