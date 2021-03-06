﻿// Matthew Cormack (@johnjoemcbob | www.matthewcormack.co.uk)
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
public class DungeonGenerationScript : ActivatableScript
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
	//public bool Regenerate = false;

	// Array of grid occupied spaces
	protected bool[,,] Occupied;
	// List of doors attached last generation, to be reset
	List<DoorDescriptionScript> Doors = new List<DoorDescriptionScript>();
	// Time optimisation testing
	private double time_tryaddroom;
	private double time_tryaddroom1;
	private double time_tryaddroom2;
	private double time_tryadddoors;
	private double time_deadends;
	private double time_checkoccupy;
	private double time_collideoccupy;

	void Update()
	{
		if ( Activated )
		{
			// Run destroy functionality the frame before generation
			if ( Occupied == null )
			{
				OnDeactivate();
				Generate();
			}
			else
			{
				Clear();
			}
			//Regenerate = false;
		}
	}

	// Generate a dungeon layout & instantiate the prefabs
	protected void Generate()
	{
		float time_total = Time.realtimeSinceStartup;
		time_tryaddroom = 0;
		time_tryadddoors = 0;
		time_deadends = 0;
		time_checkoccupy = 0;
		time_collideoccupy = 0;

		// Find starting grid occupied
		print( Occupied );
		CheckOccupy();

		transform.GetComponentInChildren<AIPathHandlerScript>().PathNodes.Clear();

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
		// Attach other doors if possible, or block exits
		Generate_TryAddDoors();
		Generate_DeadEnds();

		foreach ( DoorDescriptionScript door in Doors )
		{
			door.Attached = null;
		}

		// Find time taken and output
		time_total = Time.realtimeSinceStartup - time_total;
		//print( "Dungeon Generation Times:" );
		//print( "- TryAddRoom:    " + time_tryaddroom );
		//print( "- TryAddRoom 1:  " + time_tryaddroom1 );
		//print( "- TryAddRoom 2:  " + time_tryaddroom2 );
		//print( "- TryAddDoors:   " + time_tryadddoors );
		//print( "- DeadEnds:       " + time_deadends );
		//print( "- CheckOccupy:  " + time_checkoccupy );
		//print( "- CollideOccupy: " + time_collideoccupy );
		//print( "Total Generate: " + time_total );
	}

	// Try to add another room to the current dungeon
	protected bool Generate_TryAddRoom()
	{
		float time_startfunction = Time.realtimeSinceStartup;
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
				float time_startfunction1 = Time.realtimeSinceStartup;
				GameObject testroom = (GameObject) Instantiate( PossibleRooms[randomroom] );
				time_tryaddroom1 += Time.realtimeSinceStartup - time_startfunction1;
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
						//door.Attached = newdoor;
						//newdoor.Attached = door;
						Doors.Add( door );

						// Send to AI to generate
						transform.GetComponentInChildren<AIPathHandlerScript>().GenerateRoom( testroom );

						time_tryaddroom += Time.realtimeSinceStartup - time_startfunction;
						return true;
					}
				}

				if ( !placed )
				{
					if ( !Application.isPlaying )
					{
						DestroyImmediate( testroom );
					}
					else
					{
						Destroy( testroom );
					}
					PossibleRooms.RemoveAt( randomroom );
				}
			}
		}

		time_tryaddroom += Time.realtimeSinceStartup - time_startfunction;
        return false;
	}

	// After successfully adding a room, try to link all doors if possible
	protected void Generate_TryAddDoors()
	{
		float time_startfunction = Time.realtimeSinceStartup;

		DoorDescriptionScript[] doors = GetComponentsInChildren<DoorDescriptionScript>();
        foreach ( var door in doors )
		{
			if ( door.Attached == null )
			{
				// Search for a door opposite this one
				Vector3 position = door.Doorway.transform.position;
				foreach ( var testdoor in doors )
				{
					if ( testdoor.transform.parent == door.transform.parent ) continue;

					float dist = Vector3.Distance( testdoor.transform.position, position );
					//Debug.DrawLine( testdoor.Doorway.transform.position, position + Vector3.up, Color.black, 100 );
					if ( dist < 0.5f )
					{
						door.Attached = testdoor;
						testdoor.Attached = door;

						// Send to AI to generate
						List<AIPathNodeScript> nodes = new List<AIPathNodeScript>();
						{
							door.Node.ConnectedTo.Add( testdoor.Node );
							testdoor.Node.ConnectedTo.Add( door.Node );

							nodes.Add( door.Node );
							nodes.Add( testdoor.Node );
						}
						transform.GetComponentInChildren<AIPathHandlerScript>().PathNodes.AddRange( nodes );

						break;
                    }
				}
			}
		}

		time_tryadddoors += Time.realtimeSinceStartup - time_startfunction;
	}

	// Block any gaps in the dungeon with a dead end prefab
	protected void Generate_DeadEnds()
	{
		float time_startfunction = Time.realtimeSinceStartup;

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

		time_deadends += Time.realtimeSinceStartup - time_startfunction;
	}

	// Remove any room children (i.e. previous dungeon layouts)
	protected void Clear()
	{
		Occupied = null;

        foreach ( RoomDescriptionScript room in GetComponentsInChildren<RoomDescriptionScript>() )
		{
			if ( !room.Static )
			{
				if ( !Application.isPlaying )
				{
					DestroyImmediate( room.gameObject );
				}
				else
				{
					Destroy( room.gameObject );
				}
			}
		}
		foreach ( DoorDescriptionScript door in Doors )
		{
			door.Attached = null;
		}
		Doors.Clear();

		transform.GetComponentInChildren<AIPathHandlerScript>().PathNodes.Clear();
	}

	// Check the occupy state of the dungeon
	protected void CheckOccupy()
	{
		float time_startfunction = Time.realtimeSinceStartup;

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

						if (
							( ( g.x >= 0 ) && ( g.y >= 0 ) && ( g.z >= 0 ) ) &&
							( ( g.x < Size.x ) && ( g.y < Size.y ) && ( g.z < Size.z ) )
						)
						{
							Occupied[(int) g.x, (int) g.y, (int) g.z] = true;
						}
					}
				}
			}
		}

		time_checkoccupy += Time.realtimeSinceStartup - time_startfunction;
	}

	// Check a new room collision against the occupy grid
	protected bool CollideOccupy( RoomDescriptionScript room, DoorDescriptionScript linkdoor )
	{
		float time_startfunction = Time.realtimeSinceStartup;

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
						time_collideoccupy += Time.realtimeSinceStartup - time_startfunction;
						return false;
					}
				}
			}
		}
		foreach ( DoorDescriptionScript door in room.GetComponentsInChildren<DoorDescriptionScript>() )
		{
			Vector3 g = GetDungeonGrid( door.transform.position / RoomDescriptionScript.Static_GridSize );
			Vector3 doorg = GetDungeonGrid( linkdoor.Doorway.transform.position / RoomDescriptionScript.Static_GridSize );
			if (
				( ( g.x < 0 ) || ( g.y < 0 ) || ( g.z < 0 ) ) ||
				( ( g.x >= Size.x ) || ( g.y >= Size.y ) || ( g.z >= Size.z ) ) ||
				(
					Occupied[(int) g.x, (int) g.y, (int) g.z] &&
					( g != doorg )
				)
			)
			{
				time_collideoccupy += Time.realtimeSinceStartup - time_startfunction;
				return false;
			}
		}

		time_collideoccupy += Time.realtimeSinceStartup - time_startfunction;
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
		Gizmos.DrawWireCube( transform.position, ( Size ) * RoomDescriptionScript.Static_GridSize );

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
					Gizmos.color = new Color( 1, 0, 0, 0.1f );
					Gizmos.DrawCube(
						transform.position + GetFromDungeonPosition( new Vector3( x + 0.5f, y + 0.5f, z + 0.5f ) * RoomDescriptionScript.Static_GridSize ),
						new Vector3( 1, 1, 1 ) * RoomDescriptionScript.Static_GridSize
					);
				}
			}
			}
			}
		}
	}
}
