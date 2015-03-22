using UnityEngine;
using System.Collections;

public class StickToSomething : MonoBehaviour {


	// variable
	public Transform stickObject;

	public bool setPosition;
	public bool setRotation;
	public bool setScale;

	public bool offSetPosition;
	public bool offSetRotation;

	private Vector3 offPosition;
	private Quaternion offRotation;
	

	// Use this for initialization
	void Start () {
		offPosition = transform.position;
		offRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		//followWorldTransform(stickObject, setPosition, setRotation, setScale);	
	}

	void FixedUpdate () 
	{
		followWorldTransform(stickObject, setPosition, setRotation, setScale);
	}

	void followWorldTransform(Transform stickingObject, bool setsPostion, bool setsRotation, bool setsScale) {
		if(stickObject != null) {
			if(setsPostion) {
				transform.position = stickObject.position;
			} else if(offSetPosition) {
				transform.position = stickObject.position - offPosition;
			}

			if(setsRotation) {
				transform.rotation = stickObject.rotation;
			} else if(offSetRotation) {
				transform.rotation = stickObject.localRotation * offRotation;
			}
			/*
			if(setsScale) {
				transform.lossyScale = stickObject.lossyScale;
			}
			*/
		}

	}
}
