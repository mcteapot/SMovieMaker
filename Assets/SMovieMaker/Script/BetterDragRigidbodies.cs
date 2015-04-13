// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

using UnityEngine;
using System.Collections;

public class BetterDragRigidbodies : MonoBehaviour 
{
	public float spring = 50f;
	public float damper = 5f;
	public float drag = 10f;
	public float angularDrag = 5f;
	public float distance = 0.2f;
	public bool attachToCenterOfMass = false;

	public GameObject HoverGUIObject;

	public LayerMask Mask;

	private SpringJoint springJoint;

	public bool CheckInputDown()
	{
		return Input.GetMouseButtonDown (2);
	}

	public bool CheckInput()
	{
		return Input.GetMouseButton(2);
	}

	public Vector3 GetMousePos()
	{
		return Input.mousePosition;
	}

	void  Update ()
	{
		if(HoverGUIObject != null)
		{
			HoverGUIObject.SetActive(false);
		}

		var mainCamera = FindCamera();
		// We need to actually hit an object
		RaycastHit hit;
		if (!Physics.Raycast(mainCamera.ScreenPointToRay(GetMousePos()), out hit, 100f, Mask))
		{

			return;
		}

		// We need to hit a rigidbody that is not kinematic
		if (!hit.rigidbody || hit.rigidbody.isKinematic)
		{
			return;
		}

		if(HoverGUIObject != null)
		{
			HoverGUIObject.SetActive(true);
		}

		// Make sure the user pressed the mouse down
		if (!CheckInputDown())
		{
			return;
		}

		if (!springJoint)
		{
			var go = new GameObject("Rigidbody dragger");
			Rigidbody body = go.AddComponent<Rigidbody>();
			springJoint = go.AddComponent<SpringJoint>();
			body.isKinematic = true;
		}

		springJoint.transform.position = hit.point;
		if (attachToCenterOfMass)
		{
			var anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position;
			anchor = springJoint.transform.InverseTransformPoint(anchor);
			springJoint.anchor = anchor;
		}
		else
		{
			springJoint.anchor = Vector3.zero;
		}

		springJoint.spring = spring;
		springJoint.damper = damper;
		springJoint.maxDistance = distance;
		springJoint.connectedBody = hit.rigidbody;

		StartCoroutine (DragObject(hit.distance));
	}

	IEnumerator DragObject (float distance)
	{
		var oldDrag = springJoint.connectedBody.drag;
		var oldAngularDrag = springJoint.connectedBody.angularDrag;

		springJoint.connectedBody.drag = drag;
		springJoint.connectedBody.angularDrag = angularDrag;

		var mainCamera= FindCamera();

		while (CheckInput())
		{
			Debug.Log(springJoint.connectedBody, springJoint);
			var ray = mainCamera.ScreenPointToRay (GetMousePos());
			springJoint.transform.position = ray.GetPoint(distance);
			yield return null;
		}

		if (springJoint.connectedBody)
		{
			springJoint.connectedBody.drag = oldDrag;
			springJoint.connectedBody.angularDrag = oldAngularDrag;
			springJoint.connectedBody = null;
		}
	}

	Camera  FindCamera ()
	{
		if (GetComponent<Camera>())
			return GetComponent<Camera>();
		else
			return Camera.main;
	}
}