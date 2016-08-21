// Matthew Cormack (@johnjoemcbob | www.matthewcormack.co.uk)
// 21/08/16
//
// Rainbow Jam 2016!
//
// Describes AIPathNodes for
// pathfinding within a dungeon
//

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AIPathNodeScript : MonoBehaviour
{
	public DoorDescriptionScript Door;
	public List<AIPathNodeScript> ConnectedTo;
	public bool UpdateConnections = false;

	void Update()
	{
		if ( UpdateConnections )
		{
			foreach ( var node in ConnectedTo )
			{
				if ( node == null )
				{
					ConnectedTo.Remove( node );
					continue;
				}
				if ( !node.ConnectedTo.Contains( this ) )
				{
					node.ConnectedTo.Add( this );
				}
			}
			UpdateConnections = false;
        }
	}

	// Show the size of the dungeon in the editor
	void OnDrawGizmos()
	{
		Gizmos.color = Color.gray;
		Gizmos.DrawSphere( transform.position, 0.5f );

		Gizmos.color = new Color( 0.9f, 0.3f, 0.9f, 1 );
		foreach ( var node in ConnectedTo )
		{
			if ( node != null )
			{
				Vector3 offset = Vector3.zero;// Vector3.up / 2;
				Vector3 node1 = transform.position + offset;
				Vector3 node2 = node.transform.position + offset;

				Gizmos.DrawLine( node1, node2 );
			}
		}
	}
}
