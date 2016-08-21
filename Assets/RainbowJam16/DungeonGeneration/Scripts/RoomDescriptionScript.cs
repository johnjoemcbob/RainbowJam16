// Matthew Cormack (@johnjoemcbob | www.matthewcormack.co.uk)
// 20/08/16
//
// Rainbow Jam 2016!
//
// Describes room prefabs
// (i.e. Size, Doors)
//

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RoomDescriptionScript : MonoBehaviour
{
	public static int Static_GridSize = 5;

	[Header( "Room Description" )]
	// Flag to NOT be destroyed when a new dungeon is generated
	public bool Static = false;
	// Size of individual grid units
	// (Defines static on startup, then is ignored)
	public int GridSize = 5;
	// Size (in grid units) of this room
	public Vector3 Size;
	// All connectable doorways in this room
	// (Vector3 points to the connected grid point OUTSIDE the room)
	public List<Vector3> Doors;
	// Flag to auto find door objects in children
	// (Uses RoomDescriptionScript as a flag for Door objects)
	public bool AutoFindDoors = false;

	void Start()
	{
		Static_GridSize = GridSize;
    }

	void Update()
	{
		if ( AutoFindDoors )
		{
			Doors.Clear();

			// Find all flagged 'Door' objects in this tree
			foreach ( DoorDescriptionScript door in GetComponentsInChildren<DoorDescriptionScript>() )
			{
				// Calculate grid position from world position & add
				Vector3 gridpos = door.transform.localPosition;
				{
					gridpos /= (float) Static_GridSize;
				}
				Doors.Add( gridpos );
			}
			AutoFindDoors = false;
        }
	}

	// Getters for the local position of room sides
	public int GetLeft()
	{
		return -GetRight();
	}
	public int GetRight()
	{
		return Mathf.CeilToInt( ( Size.x - 1 ) / 2 );
	}
	public int GetBottom()
	{
		return 0;
	}
	public int GetTop()
	{
		return Mathf.CeilToInt( Size.y ) - 1;
	}
	public int GetNear()
	{
		return -GetDeep();
	}
	public int GetDeep()
	{
		return Mathf.CeilToInt( ( Size.z - 1 ) / 2 );
	}

	// Get room position on the dungeon grid
	public Vector3 GetGridPosition()
	{
		return new Vector3( transform.localPosition.x / Static_GridSize, transform.localPosition.y / Static_GridSize, transform.localPosition.z / Static_GridSize );
	}
}
