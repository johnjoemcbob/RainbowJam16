// Matthew Cormack (@johnjoemcbob | www.matthewcormack.co.uk)
// 20/08/16
//
// Rainbow Jam 2016!
//
// Describes door prefabs
// Flags to the generator whether or not this door is connected
//

using UnityEngine;
using System.Collections;

public class DoorDescriptionScript : MonoBehaviour
{
	[Header( "Door Description" )]
	// Flag for generation; is this door attached to another?
	public bool Attached = false;
	// The doorway grid space
	public GameObject Doorway;
}
