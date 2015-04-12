using UnityEngine;
using System.Collections;

public class CamcorderZoom : MonoBehaviour 
{
	public float MaxFOV = 60f;
	public float MinFOV = 20f;

	public float ZoomSpeed = 30f;
	private float _targetFOV;

	private float smoothVec;

	public Camera TargetCamera;

	void Awake()
	{
		_targetFOV = MaxFOV;
	}

	void Update()
	{
//		float input = Mathf.Sign(Input.GetAxis("Mouse ScrollWheel"));
		float input = Input.GetAxisRaw("Mouse ScrollWheel");
//		input = Mathf.Sign(input);

		if(Mathf.Abs(input) > .01f)
		{
			_targetFOV = input < 0 ? MaxFOV : MinFOV;
		}

		var fov = TargetCamera.fieldOfView;

		if(fov != _targetFOV)
		{
			fov = Mathf.SmoothDamp(fov, _targetFOV, ref smoothVec, .125f, ZoomSpeed);
//			fov = Mathf.MoveTowards(fov, _targetFOV, Time.deltaTime * ZoomSpeed);
			TargetCamera.fieldOfView = fov;
		}
	}

}
