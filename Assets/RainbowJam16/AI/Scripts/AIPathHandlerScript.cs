// Matthew Cormack (@johnjoemcbob | www.matthewcormack.co.uk)
// 21/08/16
//
// Rainbow Jam 2016!
//
// Generates AI paths for a dungeon
// using the AIPathNodes
//

using UnityEngine;
using System.Collections.Generic;

public class AIPathHandlerScript : MonoBehaviour
{
	public List<AIPathNodeScript> PathNodes;
	public Mesh GizmoMesh;

	// Generate the connection points for a new room
	// and add its internal paths to the main web
	public void GenerateRoom( GameObject newroom )
	{
		foreach ( var node in newroom.GetComponentsInChildren<AIPathNodeScript>() )
		{
			// Add door connections
			if ( node.Door )
			{
				if ( node.Door.Attached )
				{
					// Find node attached to the new door connection & link these two
					Transform otherroom = node.Door.Attached.transform.parent;
					foreach ( var othernode in otherroom.GetComponentsInChildren<AIPathNodeScript>() )
					{
						if ( othernode.Door && ( othernode.Door.Attached == node.Door ) )
						{
							node.ConnectedTo.Add( othernode );
							othernode.ConnectedTo.Add( node );
						}
					}
				}
			}

			// Add to full path web
			PathNodes.Add( node );
        }
	}

	public int GetClosestNodeIndex( Vector3 position )
	{
		int index = 0;
		int closestindex = -1;
		float maxdistance = -1;
		{
			foreach ( var node in PathNodes )
			{
				if ( node == null ) continue;

				float distance = Vector3.Distance( position, node.transform.position );
				if ( ( distance < maxdistance ) || ( maxdistance == -1 ) )
				{
					closestindex = index;
					maxdistance = distance;
				}
				index++;
			}
		}
		return closestindex;
	}
}
