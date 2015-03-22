using UnityEngine;
using System.Collections;

public class Heart : MonoBehaviour 
{
	public float Life = 1f;

	public void Awake()
	{
		var force = Vector3.up * 12f + Random.Range(-2.5f, 2.5f) * Vector3.right + Random.Range(-2.5f, 2.5f) * Vector3.forward;
		rigidbody.AddForce(force * .035f);
		StartCoroutine(Co_Shrink(Life));
	}

	private IEnumerator Co_Shrink(float delay)
	{
		var originalScale = transform.localScale;

		float t = delay;
		while(t > 0f)
		{
			transform.localScale = originalScale * (1f - (Mathf.Abs(.5f - (t/delay)) * 2f));
			t -= Time.deltaTime;
			yield return null;
		}

		Destroy(gameObject);
	}
}
