using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Diagnostics;
using System.Threading;

public class UIController : MonoBehaviour {

	public float recordTimer;

	public Text uiTimerText;
	public Text uiRecordText;

	public RectTransform uiRecordIcon;
	public RectTransform uiSaveIcon;
	public RectTransform uiTimeIcon;

	public Slider uiTimerRemainingSlider;

	public void setTimerText(string timeText) {
		if(uiTimerText != null) {
			uiTimerText.text = timeText;
		}
	}

	public void SetTimerBar(float amount)
	{
		uiTimerRemainingSlider.value = amount;
	}
}
