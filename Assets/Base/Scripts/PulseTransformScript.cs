// Matthew Cormack (@johnjoemcbob / www.matthewcormack.co.uk)
// 20/07/16
//
// The Gods Are Wanting
//
// Pulse Transform Script
// Pulsate the desired transform variables (in local space) with time
// (i.e. position/rotation/scale)

using UnityEngine;

public class PulseTransformScript : ActivatableScript
{
	// Public inspector
	public Vector3 PulsePosition;
	public Vector3 PulseRotation;
	public Vector3 PulseScale;

	public float PulseSpeed = 1;
	public float PulseLimit = 1;
	public float PulseOffset = 0;

	public bool AutoLoop = true;

	// Private
	protected float CurrentTime = 0;
	protected Vector3 CurrentPosition;
	protected Vector3 CurrentRotation;
	protected Vector3 CurrentScale;

	protected bool Enabled = false;
	protected float LastExact = 0;
	protected int EnabledCycles = 0;

	void Update()
	{
		if ( !Activated ) return;

		if ( AutoLoop || Enabled )
		{
			float templastexact = (float) LastExact;

			CurrentTime += Time.deltaTime;
			Update_Pulse( 1 );

			if ( Enabled )
			{
				// Pass over zero, has completed a cycle
				if ( Mathf.Sign( templastexact ) != Mathf.Sign( LastExact ) )
				{
					EnabledCycles--;
					if ( EnabledCycles <= 0 )
					{
						Update_Pulse_Exact( 0 );
						Enabled = false;
						EnabledCycles = 0;
                    }
				}
            }
		}
    }

	protected void Update_Pulse( float speed )
	{
        Update_Pulse_Exact( Mathf.Sin( ( CurrentTime * PulseSpeed * speed ) ) );
	}

	protected void Update_Pulse_Exact( float exact )
	{
		// Remove old
		transform.localPosition -= CurrentPosition;
		transform.localEulerAngles -= CurrentRotation;
		transform.localScale -= CurrentScale;

		// Pulsate
		float change = ( exact + PulseOffset ) * PulseLimit;
		{
			CurrentPosition = PulsePosition * change;
			CurrentRotation = PulseRotation * change;
			CurrentScale = PulseScale * change;
		}

		// Add back on
		transform.localPosition += CurrentPosition;
		transform.localEulerAngles += CurrentRotation;
		transform.localScale += CurrentScale;

		// Store this setting
		LastExact = exact;
	}

	public void Enable( int cycles )
	{
		Enabled = true;
		EnabledCycles = cycles;
	}
}
