using System.Collections.Generic;
using UnityEngine;
using ChilliConnect;

public class ChilliConnectTestScript : MonoBehaviour
{
	private bool RunCreatePlayer = false;
	private bool RunLoginPlayer = false;
	private bool RunMetricSession = false;
	private bool RunSendMessage = false;

	void Start()
	{
		ChilliConnectHandlerScript chilli = ChilliConnectHandlerScript.GetInstance();
		chilli.Start();
	}

	void Update()
	{
		ChilliConnectHandlerScript chilli = ChilliConnectHandlerScript.GetInstance();
		if ( !chilli.GetSystemStarted() )
		{

		}
		else if ( !chilli.GetPlayerExists() )
		{
			if ( !RunCreatePlayer )
			{
				chilli.FindPlayer( "Developer13" );
				RunCreatePlayer = true;
            }
		}
		else if ( !chilli.GetLoggedIn() )
		{
			if ( !RunLoginPlayer )
			{
				chilli.LoginPlayer();
				RunLoginPlayer = true;
            }
		}
		else if ( !chilli.GetMetricSessionStarted() )
		{
			if ( !RunMetricSession )
			{
				chilli.StartMetricsSession();
				RunMetricSession = true;
            }
		}
		else if ( !RunSendMessage )
		{
			AddEventRequestDesc desc = new AddEventRequestDesc( "TEST" );
			{
				desc.Parameters = new Dictionary<string, string>();
				desc.Parameters.Add( "Test Parameter", Random.Range( 0, 20 ).ToString() );
			}
			chilli.TrackEvent( desc );
			RunSendMessage = true;
        }
	}

	void OnDestroy()
	{
		ChilliConnectHandlerScript chilli = ChilliConnectHandlerScript.GetInstance();
		chilli.EndMetricsSession();
		chilli.End();
	}
}
