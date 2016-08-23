using UnityEngine;
using System.Collections;

// NOTE: Class type for instance field initialisation
[System.Serializable]
public class KeyframeTransformStruct
{
	public float Time = -1;
	public Transform Transform;
}

public class KeyframeTransformAnimationHandlerScript : KeyframeAnimationHandlerScript
{
	public KeyframeTransformStruct[] TransformKeyframes;

	void Start()
	{
		if ( Keyframes.Length > 0 )
		{
			Animation.ResizeKeyframeArray( Keyframes.Length );
			foreach ( KeyframeTransformStruct keyframe in TransformKeyframes )
			{
				AddTransformKeyframe( keyframe );
			}
		}
	}

	protected override void Update()
	{
		// Update keyframe information before animating
		int index = 0;
		foreach ( KeyframeTransformStruct keyframe in TransformKeyframes )
		{
			AddTransformKeyframe( keyframe, index );
			index++;
		}

		// Base animation code
		base.Update();
	}

	protected void AddTransformKeyframe( KeyframeTransformStruct transformkeyframe, int index = -1 )
	{
		KeyframeStruct keyframe = new KeyframeStruct();
		{
			keyframe.Time = transformkeyframe.Time;
			keyframe.Position = transformkeyframe.Transform.position;
			keyframe.Rotation = transformkeyframe.Transform.eulerAngles;
			keyframe.Scale = transformkeyframe.Transform.localScale;
		}
		if ( index == -1 )
		{
			Animation.AddKeyframe( keyframe );
		}
		else
		{
			Animation.SetKeyframe( index, keyframe );
		}
	}
}
