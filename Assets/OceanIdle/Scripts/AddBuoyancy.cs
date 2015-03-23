using UnityEngine;
using System.Collections;

namespace OceanSurfaceEffects
{
	/// <summary>
	/// Add to a game object to make it float on the water.
	/// This is just a rough method to demonstrate how to
	/// sample the wave height. You will likely need to 
	/// improve on this for a proper game.
	/// 
	/// There must a game object called Ocean with a Ocean script
	/// for this to work.
	/// </summary>
	public class AddBuoyancy : MonoBehaviour 
	{
		/// <summary>
		/// The m_ocean script.
		/// </summary>
		Ocean m_ocean;

		/// <summary>
		/// Increase the spread for wider objects.
		/// </summary>
		public float m_spread = 1.0f;

		/// <summary>
		/// Adjust offset to make the object float
		/// higher or lower in the water.
		/// </summary>
		public float m_offset = 0.0f;

		/// <summary>
		/// Increase to make the object tilt more.
		/// </summary>
		public float m_tilt = 20.0f;

		void Start() 
		{

			GameObject ocean = GameObject.Find("Ocean");
			
			if(ocean == null)
			{
				Debug.Log("Could not find ocean game object");	
				return;
			}
			
			m_ocean = ocean.GetComponent<Ocean>();
			
			if(m_ocean == null)
				Debug.Log("Could not find ocean script");
		
		}
		
		void LateUpdate() 
		{
			if(m_ocean)
			{
				Vector3 pos = transform.position;

				float ht0 = m_ocean.SampleHeight(pos + new Vector3(m_spread,0,0));
				float ht1 = m_ocean.SampleHeight(pos + new Vector3(-m_spread,0,0));
				float ht2 = m_ocean.SampleHeight(pos + new Vector3(0,0,m_spread));
				float ht3 = m_ocean.SampleHeight(pos + new Vector3(0,0,-m_spread));
				
				pos.y = (ht0+ht1+ht2+ht3)/4.0f + m_offset;

				float dx = ht0 - ht1;
				float dz = ht2 - ht3;
				
				transform.position = pos;
				
				transform.localEulerAngles = new Vector3(-dz*m_tilt,0,dx*m_tilt);
			}
		}
	}

}
