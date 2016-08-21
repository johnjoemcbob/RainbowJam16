// Matthew Cormack (@johnjoemcbob / www.matthewcormack.co.uk)
// 20/07/16
//
// The Gods Are Wanting
//
// Activatable Script
// Inheritable class which can be activated by user input
// through a InputActivatorScript instance
//

using UnityEngine;

public class ActivatableScript : MonoBehaviour
{
	[Header( "Activatable Script" )]
	public bool Activated = false;
	public bool InvertedActivation = false;

	void Start()
	{
		// Call activation for componenets preset to activate
		if ( Activated )
		{
			Activated = false;
			OnActivate();
		}
	}

	public virtual bool OnActivate()
	{
		if ( Activated ) return false;

		Activated = true;
		return true;
	}

	public virtual bool OnDeactivate()
	{
		if ( !Activated ) return false;

		Activated = false;
		return true;
	}
}
