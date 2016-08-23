using UnityEngine;
using System.Collections;

public class SpawnAtTransformScript : ActivatableScript
{
	public Transform SpawnAt;

	public override bool OnActivate()
	{
		if ( !base.OnActivate() ) return false;

		transform.position = SpawnAt.position;
		OnDeactivate();

		return true;
    }
}
