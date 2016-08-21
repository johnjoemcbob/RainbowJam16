// Matthew Cormack (@johnjoemcbob / www.matthewcormack.co.uk)
// 20/07/16
//
// The Gods Are Wanting
//
// Keyframe Animation Script
// Handles arrays of object transformation keyframes with
// functionality to interpolate between them and sample at
// specific times
//

using UnityEngine;

// NOTE: Class type for instance field initialisation
[System.Serializable]
public class KeyframeStruct
{
	public float Time = -1;
	public Vector3 Position = Vector3.zero;
	public Vector3 Rotation = Vector3.zero;
	public Vector3 Scale = new Vector3( 1, 1, 1 );

	// Constructor
	public KeyframeStruct()
	{
	}
	// Copy Constructor
	public KeyframeStruct( KeyframeStruct other )
	{
		Time = other.Time;
		Position = other.Position;
		Rotation = other.Rotation;
		Scale = other.Scale;
	}
};

public class KeyframeAnimationScript
{
	private KeyframeStruct[] Keyframes = new KeyframeStruct[1];

	#region Array Modification
	// Add a new keyframe at the first available empty array element
	// NOTE: Calls to SetKeyframe()
	// IN: (keyframe) The keyframe data struct
	// OUT: N/A
	public void AddKeyframe( KeyframeStruct keyframe )
	{
		// Loop through and find first empty
		for ( int index = 0; index < Keyframes.Length; index++ )
		{
			if ( ( Keyframes[index] == null ) || ( Keyframes[index].Time == -1 ) )
			{
				SetKeyframe( index, keyframe );
				break;
			}
		}
	}

	// Add a new keyframe at the specified array element
	// IN: (index) The element index to place the keyframe at, (keyframe) The keyframe data struct
	// OUT: N/A
	public void SetKeyframe( int index, KeyframeStruct keyframe )
	{
		Keyframes[index] = keyframe;
	}

	// Remove the keyframe at the specified array element
	// NOTE: If no index is provided, the last element will be removed
	// IN: (index) The element index to remove
	// OUT: N/A
	public void RemoveKeyframe( int index = -1 )
	{
		// If no index is provided, the last element will be removed
		if ( index == -1 ) index = Keyframes.Length - 1;

		// If it is otherwise out of bounds, return
		if ( ( index < 0 ) || ( index >= Keyframes.Length ) ) return;

		// Set the element to be null
		Keyframes[index] = null;

		// Resize the array to be one smaller (resize ignores null elements)
		ResizeKeyframeArray( Keyframes.Length - 1 );
	}

	// Resize the keyframe array
	// NOTE:
	// - Creates a new array of specified size and copies the old elements
	// - Ignores null array elements when copying
	// IN: (count) The size of the new array
	// OUT: N/A
	public void ResizeKeyframeArray( int count = 1 )
	{
		KeyframeStruct[] temp = new KeyframeStruct[count];
		{
			int index = 0;
			foreach ( KeyframeStruct frame in Keyframes )
			{
				if ( frame != null )
				{
					temp[index] = frame;
					index++;
				}
			}
		}
		Keyframes = temp;
	}

	// Sort the keyframe array by ascending time
	// NOTE: Uses a bubble sort algorithm
	// IN: N/A
	// OUT: N/A
	public void SortKeyframeArray()
	{
		for ( int start = 0; start < Keyframes.Length; start++ )
		{
			bool altered = false;
			for ( int sort = start; sort < Keyframes.Length - 1; sort++ )
			{
				if ( Keyframes[sort].Time > Keyframes[sort + 1].Time )
				{
					KeyframeStruct temp = Keyframes[sort + 1];
					Keyframes[sort + 1] = Keyframes[sort];
					Keyframes[sort] = temp;
					altered = true;
				}
			}
			// Break out early if the array is sorted
			if ( !altered )
			{
				break;
			}
		}
	}
	#endregion

	#region Interpolation
	// Animation Interpolation - Linear
	// IN: (index) The closest lower keyframe, (time) The current time to sample at
	// OUT: (KeyframeStruct) The object translation keyframe at the sampled time
	private KeyframeStruct Interpolate_Linear( int index, float time )
	{
		KeyframeStruct keyframedata = new KeyframeStruct( Keyframes[index] );
		{
			int index_last = Keyframes.Length - 1;
			if ( index != index_last )
			{
				KeyframeStruct keyframe_lower = Keyframes[index];
				KeyframeStruct keyframe_upper = Keyframes[index + 1];
				float time_dif = keyframe_upper.Time - keyframe_lower.Time;
				float time_along = time - keyframe_lower.Time;
				float time_actual = time_along / time_dif;

				keyframedata.Time = time;
				keyframedata.Position = Vector3.Lerp( keyframe_lower.Position, keyframe_upper.Position, time_actual );
				keyframedata.Rotation = Vector3.Lerp( keyframe_lower.Rotation, keyframe_upper.Rotation, time_actual );
				keyframedata.Scale = Vector3.Lerp( keyframe_lower.Scale, keyframe_upper.Scale, time_actual );
			}
		}
		return keyframedata;
	}
	#endregion

	#region Getters
	// Getter for the keyframe information at a specific time
	// NOTE: First searches for the keyframes closest to the time, then
	//       interpolates to sample
	// IN: (time) The time stamp to sample at
	// OUT: (KeyframeStruct) The transform data at that time
	public KeyframeStruct GetKeyframeAtTime( float time )
	{
		// If time is below/above range then clamp to beginning/end of animation
		int index_last = Keyframes.Length - 1;
		float time_first = Keyframes[0].Time;
		float time_last = Keyframes[index_last].Time;
		time = Mathf.Clamp( time, time_first, time_last );

		// Find closest time
		float distance = -1;
		int index = 0;
		for ( int keyframe = 0; keyframe < Keyframes.Length; keyframe++ )
		{
			float dist = time - Keyframes[keyframe].Time;
			if ( ( distance == -1 ) || ( ( dist < distance ) && ( dist > 0 ) ) )
			{
				distance = dist;
				index = keyframe;
			}
		}

		// Interpolate between this and the next time (if there is a next keyframe)
		KeyframeStruct keyframedata = Interpolate_Linear( index, time );

		return keyframedata;
	}

	// Getter for the keyframes array
	// IN: N/A
	// OUT: (KeyframeStruct[]) A reference to the Keyframes array instance
	public KeyframeStruct[] GetAllKeyframes()
	{
		return Keyframes;
	}
	#endregion
}
