using UnityEngine;
using System.Collections.Generic;

public class SteadyCamController : MonoBehaviour 
{
	public HingeJoint[] HingeJoints;

	public class JointInfo
	{
		public HingeJoint Joint;
		public float StartDamper;
		public float TargetDamper;

		public JointLimits StartLimits;
		public bool UseLimitsAtStart;

		public JointInfo(HingeJoint joint)
		{
			Joint = joint;
			StartDamper = joint.spring.damper;
			TargetDamper = (StartDamper+1f) * 100f;

			StartLimits = joint.limits;
			UseLimitsAtStart = joint.useLimits;
		}

		public void SetDamper(float dampFactor)
		{
			var spring = Joint.spring;
			spring.damper = Mathf.Lerp(StartDamper, TargetDamper, dampFactor);
			Joint.spring = spring;
		}

		public void SetLimits(float limitFactor)
		{
			Joint.useLimits = limitFactor > 0f ? true : UseLimitsAtStart;

			var limits = Joint.limits;
			limits.max = Mathf.Lerp(StartLimits.max, 0f, limitFactor);
			limits.min = Mathf.Lerp(StartLimits.min, 0f, limitFactor);
			Joint.limits = limits;
		}
	}

	private JointInfo[] JointInfos;

	void Awake()
	{
		InitJoints();
	}

	void InitJoints()
	{
		JointInfos = new JointInfo[HingeJoints.Length];
		for(int i=0;i<HingeJoints.Length;i++)
		{
			JointInfos[i] = new JointInfo(HingeJoints[i]);
		}
	}

	void Update()
	{
		var dampFactor =  Input.GetButton("Fire2") ? 1f : 0f;

		foreach(var jointInfo in JointInfos)
		{
//			jointInfo.SetDamper(dampFactor);
			jointInfo.SetLimits(dampFactor);
		}
	}

}
