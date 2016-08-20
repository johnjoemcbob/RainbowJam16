using UnityEngine;
using UnityEditor;
using System.Collections;

[ExecuteInEditMode]
public class PrefabScript : MonoBehaviour
{
	public GameObject Prefab;
	public bool Reload = false;

	void Update()
	{
		if ( Reload )
		{
			foreach ( var prefab in GetComponentsInChildren<PrefabScript>() )
			{
				if ( prefab.Prefab.name == Prefab.name )
				{
					// Store old transform
					Transform old = prefab.transform;

					// Create new prefab instance
					GameObject newobject = Instantiate( Prefab ) as GameObject;

					// Load old transform
					if ( newobject )
					{
						newobject.transform.SetParent( old.parent );
						newobject.transform.name = old.name;
						newobject.transform.localPosition = old.localPosition;
						newobject.transform.localRotation = old.localRotation;
						newobject.transform.localScale = old.localScale;
					}

					Reload = false;

					// Destroy old
					if ( newobject )
					{
						DestroyImmediate( prefab.gameObject );
					}
				}
			}
        }
	}
}
