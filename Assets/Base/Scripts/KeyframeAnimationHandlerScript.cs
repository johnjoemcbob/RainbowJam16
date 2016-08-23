 // Matthew Cormack (@johnjoemcbob / www.matthewcormack.co.uk)
// 27/07/16
//
// Interview Demo
//
// Keyframe Animation GameObject Handler Script
// Handles communication between Unity scene gameobjects
// and the KeyframeAnimation script
//

using UnityEngine;

public class KeyframeAnimationHandlerScript : ActivatableScript
{
	#region Variable Declaration
	[Header( "Keyframe Animation Handler" )]
	public bool UsePosition = true;
	public bool UseRotation = false;
	public bool UseScale = false;
	public bool UseLocalSpace = true;

	public float AnimationSpeed = 1;
	public float AnimationTime = 1;
	public float AnimationOffset = 0;
	public bool Loop = false;
	public float DeactivateResetTime = 0;
	public bool DeactivateOnEnd = true;
    public bool OnlyDeactivateOnEnd = true;
	public bool ReverseOnDeactivate = false;

	public KeyframeStruct[] Keyframes;

	protected KeyframeAnimationScript Animation = new KeyframeAnimationScript();
	protected float Time_Sample = 0;
	protected int Reverse = 1;
	#endregion

	void Start()
	{
		//Time_Sample = AnimationOffset;

        if ( Keyframes.Length > 0 )
		{
			Animation.ResizeKeyframeArray( Keyframes.Length );
			foreach ( KeyframeStruct keyframe in Keyframes )
			{
				Animation.AddKeyframe( keyframe );
			}
			Animation.SortKeyframeArray();
		}
	}

	protected virtual void Update()
	{
		if ( !Activated ) return;

		// Advance timestamp
		Time_Sample += Time.deltaTime * AnimationSpeed * Reverse;
		{
			if ( Time_Sample > AnimationTime )
			{
				Time_Sample = AnimationTime;
				if ( DeactivateOnEnd )
				{
					OnDeactivate();
				}
			}
			if ( Time_Sample < 0 )
			{
				Time_Sample = 0;
				OnDeactivate();
			}
		}
		float time = Time_Sample;
		{
			if ( AnimationOffset != 0 )
			{
				time = ( time + AnimationOffset ) % AnimationTime;
            }
		}
        KeyframeStruct keyframe = Animation.GetKeyframeAtTime( time );

		// Use keyframe data
		if ( UsePosition )
		{
			if ( UseLocalSpace )
			{
				transform.localPosition = keyframe.Position;
			}
			else
			{
				transform.position = keyframe.Position;
			}
		}
		if ( UseRotation )
		{
			if ( UseLocalSpace )
			{
				transform.localEulerAngles = keyframe.Rotation;
			}
			else
			{
				transform.eulerAngles = keyframe.Rotation;
			}
		}
		if ( UseScale )
		{
			transform.localScale = keyframe.Scale;
		}
	}

	public void SetSampleTime( float time )
	{
		Time_Sample = time;
	}

	public override bool OnActivate()
	{
		bool success = base.OnActivate();
		if ( !success )
		{
			OnDeactivate();
			base.OnActivate();
		}

		return true;
	}

	public override bool OnDeactivate()
	{
		if ( ( Time_Sample < AnimationTime ) && OnlyDeactivateOnEnd )
		{
			return false;
		}

		bool success = base.OnDeactivate();
		if ( !success ) return false;

		float oldsample = Time_Sample;
		Time_Sample = DeactivateResetTime;

		if ( Loop )
		{
			OnActivate();
		}
		if ( ReverseOnDeactivate )
		{
			if ( Reverse == 1 )
			{
				Time_Sample = oldsample;
				Reverse = -1;
				OnActivate();
			}
			else
			{
				Reverse = 1;
				Time_Sample = oldsample;
			}
		}

		return true;
	}
}
