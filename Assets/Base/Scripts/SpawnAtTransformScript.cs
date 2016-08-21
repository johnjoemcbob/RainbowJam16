using UnityEngine;
using System.Collections;

public class SpawnAtTransformScript : MonoBehaviour
{
	public Transform SpawnAt;

	void Start()
	{
		transform.position = SpawnAt.position;
    }
}
