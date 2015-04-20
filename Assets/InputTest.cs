using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InputTest : MonoBehaviour 
{
	public InputField inputField;

	void OnGUI()
	{
		Debug.Log(Event.current);

		if(Event.current != null)
		{
			Debug.Log(Event.current.type);
			Debug.Log(Event.current.keyCode);
		}
	}

}
