using UnityEngine;
using System.Collections;

public class DisableOnStartScript : MonoBehaviour
{
	public bool Activate = false;

	void Start()
	{
		gameObject.SetActive( Activate );
	}
}
