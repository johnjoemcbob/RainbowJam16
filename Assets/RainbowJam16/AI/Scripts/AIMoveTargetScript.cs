// Matthew Cormack (@johnjoemcbob | www.matthewcormack.co.uk)
// 21/08/16
//
// Rainbow Jam 2016!
//
// Finds a path for an AI agent
// through the dungeon to a specific
// target using the AIPathNodes
//

using UnityEngine;
using System.Collections.Generic;

public struct AIPathFindStruct
{
	public AIPathNodeScript Node;
	public List<AIPathNodeScript> Path;

	public AIPathFindStruct( AIPathNodeScript node )
	{
		Node = node;
		Path = new List<AIPathNodeScript>();
	}
}

public class AIMoveTargetScript : MonoBehaviour
{
	public Transform Target;
	public float TimeBetweenGenerate = -1;
	public bool GenerateNew = false;
	public float tempspeed = 1000;

	[HideInInspector]
	public List<AIPathNodeScript> CurrentPath;

	protected AIPathHandlerScript AIPathHandler;
	protected float TimeNextGenerate = -1;

	void Start()
	{
		AIPathHandler = GameObject.FindObjectOfType<AIPathHandlerScript>();
		FindPath();
	}

	void Update()
	{
		// Temp
		if ( ( CurrentPath != null ) && ( CurrentPath.Count > 0 ) )
		{
			if ( CurrentPath[0] != null )
			{
				// Lerp towards next node
				transform.position = Vector3.Lerp( transform.position, CurrentPath[0].transform.position, Time.deltaTime * tempspeed );

				// Remove node when close
				if ( Vector3.Distance( transform.position, CurrentPath[0].transform.position ) < 1 )
				{
					CurrentPath.RemoveAt( 0 );
				}
			}
		}

		if ( ( TimeBetweenGenerate != -1 ) && ( TimeNextGenerate <= Time.time ) )
		{
			GenerateNew = true;
			TimeNextGenerate = Time.time + TimeBetweenGenerate;
		}
		if ( GenerateNew )
		{
			FindPath();
			GenerateNew = false;
		}
	}

	public void FindPath()
	{
		if ( Target == null ) return;
		if ( AIPathHandler.PathNodes.Count == 0 ) return;

		// Find closest node to self
		int node_self_index = AIPathHandler.GetClosestNodeIndex( transform.position );
		AIPathNodeScript node_self = AIPathHandler.PathNodes[node_self_index];
		Debug.DrawLine( node_self.transform.position, node_self.transform.position + Vector3.up * 10, Color.red, 1000 );

		// Find closest node to target
		int node_target_index = AIPathHandler.GetClosestNodeIndex( Target.position );
		AIPathNodeScript node_target = AIPathHandler.PathNodes[node_target_index];
		Debug.DrawLine( node_target.transform.position, node_target.transform.position + Vector3.up * 10, Color.yellow, 1000 );

		// Start decision tree with current node connections
		List<AIPathFindStruct> DecisionTree = new List<AIPathFindStruct>();
		DecisionTree.Add( new AIPathFindStruct( node_self ) );
		List<AIPathNodeScript> Removed = new List<AIPathNodeScript>();

		// Try along tree until reached, attempting physical closeness nodes first
		int maxattempts = 1000;
		while ( DecisionTree.Count > 0 )
		{
			int index = GetClosestCurrentNode( DecisionTree );
			{
				if ( index == -1 ) index = 0;
			}

			if ( DecisionTree[index].Node != null )
			{
				Debug.DrawLine( DecisionTree[index].Node.transform.position, DecisionTree[index].Node.transform.position + ( Vector3.up * ( 1001 - maxattempts ) ), Color.cyan, 1000 );
				foreach ( var node in DecisionTree[index].Node.ConnectedTo )
				{
					if ( !Removed.Contains( node ) && !FindInDecision( DecisionTree, node ) )
					{
						AIPathFindStruct path = new AIPathFindStruct( node );
						{
							path.Path.AddRange( DecisionTree[index].Path );
							path.Path.Add( DecisionTree[index].Node );
						}
						DecisionTree.Add( path );
						Removed.Add( node );
					}
				}
			}

			// Found node
			if ( DecisionTree[index].Node == node_target )
			{
				//print( "Path: " );
				//int ind = 0;
				//foreach ( var node in DecisionTree[index].Path )
				//{
				//	Debug.DrawLine( node.transform.position, node.transform.position + Vector3.up * ind, Color.cyan, 1000 );
				//	ind++;
				//}
				DecisionTree[index].Path.Add( DecisionTree[index].Node );
                CurrentPath = DecisionTree[index].Path;
				break;
			}

			Removed.Add( DecisionTree[index].Node );
			DecisionTree.Remove( DecisionTree[index] );

			// Don't infinitely loop
			maxattempts--;
			if ( maxattempts <= 0 )
			{
				break;
			}
        }
	}

	public int GetClosestCurrentNode( List<AIPathFindStruct> DecisionTree )
	{
		int index = 0;
		int closestindex = -1;
		float maxdistance = -1;
		foreach ( var node in DecisionTree )
		{
			if ( node.Node == null ) continue;

			float distance = Vector3.Distance( Target.position, node.Node.transform.position );
			if ( ( distance < maxdistance ) || ( maxdistance == -1 ) )
			{
				closestindex = index;
				maxdistance = distance;
			}
			index++;
		}
		return closestindex;
	}

	protected bool FindInDecision( List<AIPathFindStruct> Decision, AIPathNodeScript node )
	{
		foreach ( var pathnode in Decision )
		{
			if ( pathnode.Node == node )
			{
				return true;
			}
		}
		return false;
	}
}
