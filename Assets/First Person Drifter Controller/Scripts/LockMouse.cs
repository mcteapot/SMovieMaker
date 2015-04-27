// by @torahhorse

using UnityEngine;
using System.Collections;

public class LockMouse : MonoBehaviour
{	
	void Start()
	{
		LockCursor(true);
	}

    void Update()
    {
		if(Input.GetMouseButtonDown(1))
		{
			LockCursor(true);
		}

    	// lock when mouse is clicked
    	if( Input.GetMouseButtonDown(0) && Time.timeScale > 0.0f )
    	{
    		LockCursor(true);
    	}
    
    	// unlock when escape is hit
        if  ( Input.GetKeyDown(KeyCode.Escape) )
        {
        	LockCursor(!Screen.lockCursor);
        }
    }
    
    public void LockCursor(bool lockCursor)
    {
    	Screen.lockCursor = lockCursor;
    }
}