// Matthew Cormack (@johnjoemcbob / www.matthewcormack.co.uk)
// 20/07/16
//
// The Gods Are Wanting
//
// Input Activator Script
// Activates inspector defined arrays of various other
// scripts on user input
//

using UnityEngine;
using System.Collections;

public class InputActivatorScript : MonoBehaviour
{
	public string Button = "Fire1";
	public bool Invert = false;
	public ActivatableScript[] Scripts;

	void Update()
	{
		if ( Input.GetButtonDown( Button ) != Invert )
		{
			foreach ( ActivatableScript script in Scripts )
			{
				if ( script == null ) continue;

				script.OnActivate();
            }
		}
	}
}
