using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Diagnostics;
using System.Threading;

public class UIController : MonoBehaviour {

	public float recordTimer;

	public Text uiTimerText;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setTimerText(string timeText) {
		if(uiTimerText != null) {
			uiTimerText.text = timeText;
		}
	}
}
