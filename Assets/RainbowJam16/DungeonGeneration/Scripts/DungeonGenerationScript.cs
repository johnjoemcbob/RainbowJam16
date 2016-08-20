// Matthew Cormack (@johnjoemcbob | www.matthewcormack.co.uk)
// 20/08/16
//
// Rainbow Jam 2016!
//
// Generates a dungeon using the supplied
// dungeon size & room prefabs
//

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class DungeonGenerationScript : MonoBehaviour
{
	[Header( "Dungeon Generation" )]
	// Size of the dungeon (in grid units)
	public Vector3 Size = new Vector3( 10, 1, 10 );
	// All possible room prefabs which could be generated
	public List<GameObject> RoomPrefabs;
	// All possible dead end prefabs which could be added after generation
	public List<GameObject> DeadEndPrefabs;
	// Flag to regenerate the dungeon
	// (for testing)
	public bool Regenerate = false;

	// Array of grid occupied spaces
	protected bool[,,] Occupied;
	// List of doors attached last generation, to be reset
	List<DoorDescriptionScript> Doors = new List<DoorDescriptionScript>();

	void Update()
	{
		if ( Regenerate )
		{
			Clear();
			Generate();
			Regenerate = false;
		}
	}

	// Generate a dungeon layout & instantiate the prefabs
	protected void Generate()
	{
		// Find starting grid occupied
		CheckOccupy();

		// Until tryaddroom returns fail, create extra rooms
		int runcap = 0;
		while ( true )
		{
			bool success = Generate_TryAddRoom();
			if ( !success || ( runcap > 100 ) )
			{
				break;
			}

			CheckOccupy();
			runcap++;
        }
		Generate_DeadEnds();

		foreach ( DoorDescriptionScript door in Doors )
		{
			door.Attached = false;
		}
	}

	// Try to add another room to the current dungeon
	protected bool Generate_TryAddRoom()
	{
		// All doors must be;
		// * Connected
		// * Free
		// * Not outside dungeon bounds
		// Room must also fit inside dungeon bounds

		// Find a free door (only one per try)
		foreach ( var door in GetComponentsInChildren<DoorDescriptionScript>() )
		{
			if ( door.Attached ) continue;

			// Initialise as all rooms possible, select random & remove as needed
			List<GameObject> PossibleRooms = new List<GameObject>( RoomPrefabs );
			while ( PossibleRooms.Count > 0 )
			{
				// Select random & remove
				int randomroom = Random.Range( 0, PossibleRooms.Count );

				// Try each door until one fits
				GameObject testroom = (GameObject) Instantiate( PossibleRooms[randomroom] );
				bool placed = false;
                foreach ( DoorDescriptionScript newdoor in testroom.GetComponentsInChildren<DoorDescriptionScript>() )
				{
					testroom.transform.SetParent( transform );

					// Rotate to have the opposite forward
					Vector3 oldforward, newforward;
					testroom.transform.localEulerAngles -= new Vector3( 0, 90, 0 );
					do
					{
						testroom.transform.localEulerAngles += new Vector3( 0, 90, 0 );

						oldforward = ( door.Doorway.transform.position - door.transform.position ).normalized;
						newforward = ( newdoor.Doorway.transform.position - newdoor.transform.position ).normalized;
					} while ( oldforward != -1 * newforward );

					// Position doorways adjacent
					Vector3 offset = Quaternion.Euler( testroom.transform.localEulerAngles ) * newdoor.transform.localPosition;
					testroom.transform.position = door.Doorway.transform.position - offset;

					// Check if there is space inside the dungeon for this new layout
					bool hasspace = CollideOccupy( testroom.GetComponent<RoomDescriptionScript>(), door );
					if ( hasspace )
					{
						placed = true;

						// Flag as attached
						door.Attached = true;
						newdoor.Attached = true;
						Doors.Add( door );

						return true;
					}
				}

				if ( !placed )
				{
					DestroyImmediate( testroom );
					PossibleRooms.RemoveAt( randomroom );
				}
			}
		}

		return false;
	}

	// Block any gaps in the dungeon with a dead end prefab
	protected void Generate_DeadEnds()
	{
		foreach ( var door in GetComponentsInChildren<DoorDescriptionScript>() )
		{
			if ( !door.Attached )
			{
				int index = Random.Range( 0, DeadEndPrefabs.Count );
				GameObject deadend = (GameObject) Instantiate( DeadEndPrefabs[index] );
				deadend.transform.SetParent( transform );

				// Rotate to have the opposite forward
				Vector3 oldforward, newforward;
				deadend.transform.localEulerAngles -= new Vector3( 0, 90, 0 );
				do
				{
					deadend.transform.localEulerAngles += new Vector3( 0, 90, 0 );

					oldforward = ( door.Doorway.transform.position - door.transform.position ).normalized;
					newforward = deadend.transform.forward;
				} while ( oldforward != -1 * newforward );

				// Position doorways adjacent;
				deadend.transform.position = door.Doorway.transform.position;
			}
		}
    }

	// Remove any room children (i.e. previous dungeon layouts)
	protected void Clear()
	{
		Occupied = null;

        foreach ( RoomDescriptionScript room in GetComponentsInChildren<RoomDescriptionScript>() )
		{
			if ( !room.Static )
			{
				DestroyImmediate( room.gameObject );
			}
		}
		foreach ( DoorDescriptionScript door in Doors )
		{
			door.Attached = false;
		}
		Doors.Clear();
	}

	// Check the occupy state of the dungeon
	protected void CheckOccupy()
	{
		// Initialise to size of dungeon grid
		if ( Occupied == null )
		{
			Occupied = new bool[(int) Size.x, (int) Size.y, (int) Size.z];
		}

		// Check each contained room's positioning & size
		foreach ( RoomDescriptionScript room in GetComponentsInChildren<RoomDescriptionScript>() )
		{
			for ( int x = room.GetLeft(); x <= room.GetRight(); x++ )
			{
				for ( int y = room.GetBottom(); y <= room.GetTop(); y++ )
				{
					for ( int z = room.GetNear(); z <= room.GetDeep(); z++ )
					{
						Vector3 g = GetDungeonGrid( ( Quaternion.Euler( room.transform.localEulerAngles ) * new Vector3( x, y, z ) ) + room.GetGridPosition() );
						Occupied[(int) g.x, (int) g.y, (int) g.z] = true;
					}
				}
			}
		}
	}

	// Check a new room collision against the occupy grid
	protected bool CollideOccupy( RoomDescriptionScript room, DoorDescriptionScript linkdoor )
	{
		for ( int x = room.GetLeft(); x <= room.GetRight(); x++ )
		{
			for ( int y = room.GetBottom(); y <= room.GetTop(); y++ )
			{
				for ( int z = room.GetNear(); z <= room.GetDeep(); z++ )
				{
					Vector3 g = GetDungeonGrid( new Vector3( x, y, z ) + room.GetGridPosition() );
					if (
						( ( g.x < 0 ) || ( g.y < 0 ) || ( g.z < 0 ) ) ||
						( ( g.x >= Size.x ) || ( g.y >= Size.y ) || ( g.z >= Size.z ) ) ||
						Occupied[(int) g.x, (int) g.y, (int) g.z]
					)
					{
						return false;
					}
				}
			}
		}
		foreach ( DoorDescriptionScript door in room.GetComponentsInChildren<DoorDescriptionScript>() )
		{
			Vector3 g = GetDungeonGrid( door.transform.position / RoomDescriptionScript.Static_GridSize );
			Vector3 doorg = GetDungeonGrid( linkdoor.Doorway.transform.position / RoomDescriptionScript.Static_GridSize );
			if ( Occupied[(int) g.x, (int) g.y, (int) g.z] && ( g != doorg ) )
			{
				return false;
			}
		}

		return true;
	}

	// Offset a position by the dungeon bounds
	public Vector3 GetDungeonPosition( Vector3 pos )
	{
		return pos + ( Size * RoomDescriptionScript.Static_GridSize / 2 );
	}
	public Vector3 GetFromDungeonPosition( Vector3 pos )
	{
		return pos - ( Size * RoomDescriptionScript.Static_GridSize / 2 );
	}
	public Vector3 GetDungeonGrid( Vector3 pos )
	{
		Vector3 grid = pos + ( Size / 2 );
        return new Vector3( Mathf.Floor( grid.x ), Mathf.Floor( grid.y ), Mathf.Floor( grid.z ) );
	}

	// Show the size of the dungeon in the editor
	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube( transform.position, Size * RoomDescriptionScript.Static_GridSize );

		// Draw occupied grid spaces
		if ( Occupied != null )
		{
			for ( int x = 0; x < Size.x; x++ )
			{
			for ( int y = 0; y < Size.y; y++ )
			{
			for ( int z = 0; z < Size.z; z++ )
			{
				if ( Occupied[x, y, z] )
				{
					Gizmos.color = Color.red;
					Gizmos.DrawWireCube(
						transform.position + GetFromDungeonPosition( new Vector3( x, y + 0.5f, z ) * RoomDescriptionScript.Static_GridSize ),
						new Vector3( 1, 1, 1 ) * RoomDescriptionScript.Static_GridSize
					);
				}
			}
			}
			}
		}
	}
}
