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

	public int ControllerNumber = 1;

	public AudioClip[] VoiceClips;

	private float _resetTimer = 0f;

	public Transform CamParent;
	public Camera FirstPersonCam;

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


	public void Update()
	{
		inputs = GetInputs(ControllerNumber);

		_resetTimer += Time.deltaTime;

		foreach(var input in inputs)
		{
			if(!Mathf.Approximately(input, 0f))
			{
				_resetTimer = 0f;
			}
		}

		if(_resetTimer > 30f)
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

		/*
		if(Input.GetButtonDown("Player"+ControllerNumber+" Voice"))
		{
			PlayRandomVoiceClip();
		}
		*/

		spinTimer -= Time.deltaTime;
		_voiceTimer -= Time.deltaTime;
	}

	public float[] GetInputs(int controllerNumber)
	{
		return new float[]
		{
			Input.GetAxis(string.Format("Player{0} Horizontal", controllerNumber)),
			Input.GetAxis(string.Format("Player{0} Vertical", controllerNumber)),
		};
	}

	public void FixedUpdate()
	{
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

	public void SpecificControls()
	{
		RotateArms(inputs[0]);
		RotateElbows(inputs[0]);

		RotateLegs(inputs[0]);
		RotateKnees(inputs[0]);

		FullBody(Others, inputs[0]);
	}

	public void RotateArms(float amount)
	{
		var force = amount * 150f;
		var forceMode = ForceMode.VelocityChange;

		RightArm.AddRelativeTorque(force * (-Vector3.up + Vector3.forward) , forceMode);
		LeftArm.AddRelativeTorque(force * (-Vector3.up - Vector3.forward) , forceMode);
	}

	public void RotateElbows(float amount)
	{
		var force = Vector3.up * amount * 2f;
		var forceMode = ForceMode.Force;

		//RightElbow.AddRelativeTorque(force * -10000f, forceMode);
		RightElbow.AddRelativeTorque(Vector3.forward * amount * 10f, forceMode);
		LeftElbow.AddRelativeForce(force * 8f, forceMode);
	}

	public void RotateLegs(float amount)
	{
		var force = amount * -15000f;
		var forceMode = ForceMode.VelocityChange;

		RightThigh.AddTorque(RightThigh.transform.up * force, forceMode);
		LeftThigh.AddTorque(LeftThigh.transform.up * force, forceMode);

//		RightArm.AddRelativeTorque(force * (-Vector3.up + Vector3.forward) , forceMode);
//		LeftArm.AddRelativeTorque(force * (-Vector3.up - Vector3.forward) , forceMode);
	}

	public void RotateKnees(float amount)
	{
		var force = amount * 2000f;
		var forceMode = ForceMode.Force;

		RightCalf.AddTorque(RightCalf.transform.up * force, forceMode);
		LeftCalf.AddTorque(LeftCalf.transform.up * force, forceMode);
	}

	public void FullBody()
	{
		FullBody(Joints, inputs[1]);
	}

	public void FullBody(IEnumerable<CharacterJoint> rigidbodies, float input)
	{
		foreach(var cj in rigidbodies)
		{
			FullBody(cj, input);
		}
	}
			
	public void FullBody(CharacterJoint joint, float input)
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

	public void SpawnHeart(Vector3 position)
	{
		var heart = Instantiate(HeartPrefab, position, Quaternion.identity) as Heart;
	}

	public AudioClip PlayRandomVoiceClip()
	{
		var clip = GetRandomVoiceClip();
		AudioSource.PlayClipAtPoint(clip, transform.position);
		return clip;
	}

	public AudioClip GetRandomVoiceClip()
	{
		if(VoiceClips.Length == 0) return null;
		return VoiceClips[Random.Range(0, VoiceClips.Length)];
	}
}
