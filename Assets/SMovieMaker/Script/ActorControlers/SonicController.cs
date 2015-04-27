using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonicController : MonoBehaviour 
{
	static bool FirstPersonMode = false;

	public enum ControlType
	{
		Old = 0,
		Arms = 1,
	}

	public enum AIType
	{
		Player = 0,
		Normal = 1,
	}

	public Heart HeartPrefab;

	public CharacterJoint[] Joints;
	public Transform CenterPos;

	private float[] inputs = new float[]{0f,0f,0f,0f};

	public AudioClip SpinSound;
	private float spinTimer = 0f;

	public Rigidbody MainRigidBody;

	public Rigidbody RightArm;
	public Rigidbody LeftArm;
	public Rigidbody RightElbow;
	public Rigidbody LeftElbow;

	public Rigidbody RightThigh;
	public Rigidbody LeftThigh;
	public Rigidbody RightCalf;
	public Rigidbody LeftCalf;

	public Transform Head;

	public List<CharacterJoint> Others = new List<CharacterJoint>();

	public ControlType ControlMode;
	public AIType AIMode;

	public int ControllerNumber = 1;

	public AudioClip[] VoiceClips;

	private float _resetTimer = 0f;

	public Transform CamParent;
	public Camera FirstPersonCam;

	public bool IsLimp { get; set; }

	private bool _initScale = false;
	private Vector3 _originalScale;
	private float _scale = 1f;
	public float Scale 
	{ 
		get
		{
			return _scale;
		}
		set
		{
			if(_initScale == false)
			{
				_initScale = true;
				_originalScale = transform.localScale;
			}

			_scale = value;
			transform.localScale = _originalScale * _scale;

			foreach(var rb in GetComponentsInChildren<Rigidbody>())
			{
				rb.mass = rb.mass * (_scale * _scale);
			}
		}
	}

	void Awake()
	{
		FindBodyParts();

		foreach(var j in Joints)
		{
			j.rigidbody.angularDrag = 0f;
			j.rigidbody.drag = 0f;
			if(ControlMode == ControlType.Arms) j.rigidbody.mass *= 3f;
			j.gameObject.AddComponent<CoolCollider>().OnCollisionAction = OnCollisionHandler;
		}

		SetControllerIndex(ControllerNumber);
	}

	public void SetControllerIndex(int number)
	{
		ControllerNumber = number;
		foreach(var coll in GetComponentsInChildren<Collider>())
		{
			coll.tag = "Player"+ControllerNumber;
		}

		ConfigureCamera();
	}

	private void ConfigureCamera()
	{
		if(FirstPersonMode == false)
		{
			if(FirstPersonCam != null)
			{
				FirstPersonCam.enabled = false;
			}
			return;
		}

		if(FirstPersonCam == null)
		{
			if(CamParent != null)
			{
				FirstPersonCam = CamParent.gameObject.AddComponent<Camera>();
			}

			if(Head != null && false)
			{
				FirstPersonCam = new GameObject("headcam").AddComponent<Camera>();
				FirstPersonCam.transform.parent = Head;
				FirstPersonCam.transform.localPosition = new Vector3(0f, -0.013f, .148f);
				FirstPersonCam.transform.localRotation = Quaternion.identity;

				if(Head.gameObject.name == "head")
				{
					FirstPersonCam.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
				}
				else
				{

				}
			}
		}

		if(FirstPersonCam == null)
		{
			return;
		}

		var rect = new Rect((ControllerNumber-1) * .25f, 0f, .25f, .2f);
		FirstPersonCam.rect = rect;
	}

	protected void FindBodyParts()
	{
		Joints = transform.GetComponentsInChildren<CharacterJoint>();

		foreach(var joint in Joints)
		{
			var lowerName = joint.name.ToLower();
			if(lowerName.Contains("upperarm"))
			{
				if(lowerName.EndsWith("_l"))
				{
					LeftArm = joint.rigidbody;
				}
				else
				{
					RightArm = joint.rigidbody;
				}
			}
			else if(lowerName.Contains("forearm"))
			{
				if(lowerName.EndsWith("_l"))
				{
					LeftElbow = joint.rigidbody;
				}
				else
				{
					RightElbow = joint.rigidbody;
				}
			}
			else if(lowerName.Contains("calf"))
			{
				if(lowerName.EndsWith("_l"))
				{
					LeftCalf = joint.rigidbody;
				}
				else
				{
					RightCalf = joint.rigidbody;
				}
			}
			else if(lowerName.Contains("thigh"))
			{
				if(lowerName.EndsWith("_l"))
				{
					LeftThigh = joint.rigidbody;
				}
				else
				{
					RightThigh = joint.rigidbody;
				}
			}
			else
			{
				Others.Add(joint.GetComponent<CharacterJoint>());
			}

			if(Head == null)
			{
				if(lowerName.Contains("head"))
				{
					Head = joint.transform;
				}
				else if(lowerName.Contains("neck"))
				{
					var child = joint.transform.FindChild("head");

					if(child != null)
					{
						Head = child;
					}
					else
					{
						Head = joint.transform.FindChild("head");
					}
				}
			}
		}
	}


	private void Update()
	{
		UpdateInputs(ControllerNumber);

		_resetTimer += Time.deltaTime;

		foreach(var input in inputs)
		{
			if(!Mathf.Approximately(input, 0f))
			{
				_resetTimer = 0f;
			}
		}

		if(AIMode == AIType.Player && _resetTimer > 30f)
		{
			Destroy(gameObject);
		}

		if(MainRigidBody.angularVelocity.magnitude > 5f)
		{
			if(spinTimer <= 0f)
			{
				spinTimer = Random.Range(1.5f, 2.8f);
				audio.clip = SpinSound;
				audio.pitch = Random.Range(.75f, 1.25f);
				audio.volume = .1f;
				audio.Play();
			}
		}

		spinTimer -= Time.deltaTime;
		_voiceTimer -= Time.deltaTime;
	}

	private void UpdateInputs(int controllerNumber)
	{
		switch (AIMode)
		{
		case AIType.Player:
			inputs[0] = Input.GetAxis(string.Format("Player{0} Horizontal", controllerNumber));
			inputs[1] = Input.GetAxis(string.Format("Player{0} Vertical", controllerNumber));
			break;
		default:
			UpdateAIInputs();
			break;
		}


	}

	public float AISpeed = 1f;
	float _aiTime;
	private void UpdateAIInputs()
	{
		_aiTime += Time.deltaTime * AISpeed;
		inputs[0] = 0f;
		inputs[1] = Mathf.Cos(_aiTime);
	}

	private void FixedUpdate()
	{
		if(IsLimp)
		{
			return;
		}

		switch(ControlMode)
		{
		case ControlType.Old:
			FullBody();
			break;
		case ControlType.Arms:
			SpecificControls();
			break;
		}
	}

	private void SpecificControls()
	{
		RotateArms(inputs[0]);
		RotateElbows(inputs[0]);

		RotateLegs(inputs[0]);
		RotateKnees(inputs[0]);

		FullBody(Others, inputs[0]);
	}

	private void RotateArms(float amount)
	{
		var force = amount * 150f;
		var forceMode = ForceMode.VelocityChange;

		RightArm.AddRelativeTorque(force * (-Vector3.up + Vector3.forward) , forceMode);
		LeftArm.AddRelativeTorque(force * (-Vector3.up - Vector3.forward) , forceMode);
	}

	private void RotateElbows(float amount)
	{
		var force = Vector3.up * amount * 2f;
		var forceMode = ForceMode.Force;

		//RightElbow.AddRelativeTorque(force * -10000f, forceMode);
		RightElbow.AddRelativeTorque(Vector3.forward * amount * 10f, forceMode);
		LeftElbow.AddRelativeForce(force * 8f, forceMode);
	}

	private void RotateLegs(float amount)
	{
		var force = amount * -15000f;
		var forceMode = ForceMode.VelocityChange;

		RightThigh.AddTorque(RightThigh.transform.up * force, forceMode);
		LeftThigh.AddTorque(LeftThigh.transform.up * force, forceMode);

//		RightArm.AddRelativeTorque(force * (-Vector3.up + Vector3.forward) , forceMode);
//		LeftArm.AddRelativeTorque(force * (-Vector3.up - Vector3.forward) , forceMode);
	}

	private void RotateKnees(float amount)
	{
		var force = amount * 2000f;
		var forceMode = ForceMode.Force;

		RightCalf.AddTorque(RightCalf.transform.up * force, forceMode);
		LeftCalf.AddTorque(LeftCalf.transform.up * force, forceMode);
	}

	private void FullBody()
	{
		FullBody(Joints, inputs[1]);
	}

	private void FullBody(IEnumerable<CharacterJoint> rigidbodies, float input)
	{
		foreach(var cj in rigidbodies)
		{
			FullBody(cj, input);
		}
	}
			
	private void FullBody(CharacterJoint joint, float input)
	{
		//var input = inputs[i%inputs.Length];
		//joint.rigidbody.AddRelativeTorque(joint.swingAxis * input * 5000f * Time.deltaTime);

		var lowerName = joint.gameObject.name.ToLower();

		//if(lowerName.Contains("neck") || lowerName.Contains("knee") || lowerName.Contains("elbow"))
		{
			var force = input * 15000f;
//			var hForce = Input.GetAxis("Horizontal") * 15000f;

			if(lowerName.Contains("knee"))
			{
				force *= -1f;
			}

			if(lowerName.Contains("forearm"))
			{
				var tforce = force;
				if(lowerName.Contains("_l"))
				{
					tforce *= -1f;
				}

				joint.rigidbody.AddRelativeTorque(Vector3.forward * tforce);
			}
			else if(lowerName.Contains("elbow"))
			{
				var tforce = force;
				if(lowerName.Contains("right"))
				{
//						tforce *= -1f;
				}

				joint.rigidbody.AddRelativeTorque(Vector3.forward * tforce);
			}
			else
			{
				var pos = joint.transform.position;

				var offset = CenterPos.InverseTransformPoint(pos);
				//joint.rigidbody.AddForce(offset * input * 50f);

				//var horiz = (offset - (Vector3.right * Input.GetAxis("Horizontal"))).x;
				//offset += horiz * Vector3.right * 10f;

				joint.rigidbody.AddRelativeTorque(offset * force, ForceMode.Acceleration);

				//joint.rigidbody.AddForce(offset.normalized * .001f * force, ForceMode.Force);
			}
		}
	}

	private float _voiceTimer = 0f;

	public void OnCollisionHandler(Collision collision)
	{
		SpawnHeart(collision.contacts[0].point);

		if(_voiceTimer <= 0f)
		{
			var clip = PlayRandomVoiceClip();
			_voiceTimer = clip.length;
		}

		//ScoreController.Instance.AddScore(1);
	}

	private void SpawnHeart(Vector3 position)
	{
		var heart = Instantiate(HeartPrefab, position, Quaternion.identity) as Heart;
	}

	public AudioClip PlayRandomVoiceClip()
	{
		var clip = GetRandomVoiceClip();
		AudioClipUtil.PlayClipAtPoint(clip, transform.position, Mathf.Lerp(1f, 1f/ Scale, .1f));
		return clip;
	}

	private AudioClip GetRandomVoiceClip()
	{
		if(VoiceClips.Length == 0) return null;
		return VoiceClips[Random.Range(0, VoiceClips.Length)];
	}

	private void AdjustAISpeed(float amount)
	{
		AISpeed = Mathf.Clamp(AISpeed + amount, 0f, 20f);
	}

	public void TellSpeedUp()
	{
		AdjustAISpeed(.25f);
	}

	public void TellSlowDown()
	{
		AdjustAISpeed(-.25f);
	}

	public void AttractTo(GameObject target, float amount)
	{
		foreach(var joint in Joints)
		{
			var offset = target.transform.position - joint.transform.position;
			joint.rigidbody.AddForce(offset.normalized * amount * Time.deltaTime, ForceMode.Force);
		}
	}
}
