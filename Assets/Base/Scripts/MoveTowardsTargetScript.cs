// Matthew Cormack (@johnjoemcbob / www.matthewcormack.co.uk)
// 24/07/16
//
// The Gods Are Wanting
//
// Move Towards Target Script
// Move the attached gameobject's transform to a
// target set by other scripts
//

using UnityEngine;

public class MoveTowardsTargetScript : MonoBehaviour
{
    public float MoveSpeed = 1;
	public float TurnSpeed = 1;

	public bool UsePosition = true;
	public bool UseRotation = true;
	public bool UseScale = true;

	public Transform Target;

	void Update()
	{
		if ( UsePosition )
		{
			transform.position += ( Target.position - transform.position ) * Time.deltaTime * MoveSpeed;
		}
		if ( UseRotation )
		{
			transform.rotation = Quaternion.RotateTowards( transform.rotation, Target.rotation, Time.deltaTime * TurnSpeed );
		}
	}
}
