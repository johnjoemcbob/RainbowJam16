using System;
using System.IO;
using ChilliConnect;

public class ChilliConnectHandlerScript
{
	protected static ChilliConnectHandlerScript Instance;

	protected ChilliConnectSdk ChilliConnect;
	protected bool Started = false;
	protected bool Started_Metrics = false;
	protected bool Started_LoggedIn = false;

	protected string Player_ID = "";
	protected string ConnectID = "";
	protected string ConnectSecret = "";

	public void Start()
	{
		if ( Started ) return;

		ChilliConnect = new ChilliConnectSdk( "IDtfJT7fQPPl13x9eeAxdlVP7C66j5a4", false );
		Started = true;
    }

	public void CreatePlayer( string name = "Developer" )
	{
		Action<CreatePlayerRequest, CreatePlayerResponse> successcallback = ( CreatePlayerRequest request, CreatePlayerResponse response ) =>
		{
			ConnectID = response.ChilliConnectId;
			ConnectSecret = response.ChilliConnectSecret;

			SavePlayer();

			UnityEngine.Debug.Log( "Player (" + name + ") created with ChilliConnectID: " + ConnectID );
		};

		Action<CreatePlayerRequest, CreatePlayerError> errorcallback = ( CreatePlayerRequest request, CreatePlayerError error ) =>
		{
			UnityEngine.Debug.Log( "An error occurred while creating a new player (" + name + "): " + error.ErrorDescription );
		};

		var desc = new CreatePlayerRequestDesc();
		{
			desc.DisplayName = name;
			desc.UserName = name;
			Player_ID = name;
        }
		ChilliConnect.PlayerAccounts.CreatePlayer( desc, successcallback, errorcallback );
	}

	public void LoginPlayer( string id, string secret )
	{
		Action<LogInUsingChilliConnectRequest> successcallback = ( LogInUsingChilliConnectRequest request ) =>
		{
			UnityEngine.Debug.Log( "Successfully logged in!" );
			Started_LoggedIn = true;
        };

		Action<LogInUsingChilliConnectRequest, LogInUsingChilliConnectError> errorcallback = ( LogInUsingChilliConnectRequest request, LogInUsingChilliConnectError error ) =>
		{
			UnityEngine.Debug.Log( "An error occurred while logging in: " + error.ErrorDescription + "(" + id + ") (" + secret + ")" );
			Started_LoggedIn = false;
        };

		ChilliConnect.PlayerAccounts.LogInUsingChilliConnect( id, secret, successcallback, errorcallback );
	}

	public void LoginPlayer()
	{
		FindPlayer();
		LoginPlayer( ConnectID, ConnectSecret );
	}

	// Try to load the player if they already exist, otherwise create a now one
	public void FindPlayer( string name = "Developer" )
	{
		if ( ConnectID != "" ) return;

		if ( !LoadPlayer() )
		{
			CreatePlayer( name );
		}
	}

	public void StartMetricsSession()
	{
		Action<StartSessionRequest> successcallback = ( StartSessionRequest request ) =>
		{
			UnityEngine.Debug.Log( "Successfully started session!" );
			Started_Metrics = true;
        };

		Action<StartSessionRequest, StartSessionError> errorcallback = ( StartSessionRequest request, StartSessionError error ) =>
		{
			UnityEngine.Debug.Log( "An error occurred while starting the session: " + error.ErrorDescription );
			Started_Metrics = false;
        };

		ChilliConnect.Metrics.StartSession( Player_ID, "Unity-v1.0", successcallback, errorcallback );
	}

	public void EndMetricsSession()
	{
		Action successcallback = () =>
		{
			UnityEngine.Debug.Log( "Successfully ended session!" );
			Started_Metrics = false;
        };

		Action<EndSessionError> errorcallback = ( EndSessionError error ) =>
		{
			UnityEngine.Debug.Log( "An error occurred while ending the session: " + error.ErrorDescription );
			Started_Metrics = false;
        };

		ChilliConnect.Metrics.EndSession( successcallback, errorcallback );
    }

	public void TrackEvent( AddEventRequestDesc desc )
	{
		if ( !Started_Metrics )
		{
			UnityEngine.Debug.Log( "Metrics session hasn't been started!" );
			return;
		}

		Action<AddEventRequest> successcallback = ( AddEventRequest request ) =>
		{
			UnityEngine.Debug.Log( "Successfully send event!" );
		};

		Action<AddEventRequest, AddEventError> errorcallback = ( AddEventRequest request, AddEventError error ) =>
		{
			UnityEngine.Debug.Log( "An error occurred while sending the event (" + desc.Type + "): " + error.ErrorDescription );
		};

		ChilliConnect.Metrics.AddEvent( desc, successcallback, errorcallback );
	}

	public void End()
	{
		if ( !Started ) return;

		ChilliConnect.Dispose();
		UnityEngine.Debug.Log( "ChilliConnect disposed." );
		Started = false;
    }

	public void SavePlayer()
	{
		string dir = GetFilePath();

		File.WriteAllText( dir, ConnectID + "|" + ConnectSecret + "|" + Player_ID );
    }

	public bool LoadPlayer()
	{
		string dir = GetFilePath();

		if ( File.Exists( dir ) )
		{
			string data = File.ReadAllText( dir );
			string[] datasplit = data.Split( '|' );
			if ( datasplit.Length == 3 )
			{
				ConnectID = datasplit[0];
				ConnectSecret = datasplit[1];
				Player_ID = datasplit[2];
				return true;
            }
		}

		return false;
	}

	public string GetFilePath()
	{
		return Path.Combine( UnityEngine.Application.persistentDataPath, "chilli.txt" );
	}

	public bool GetSystemStarted()
	{
		return Started;
	}

	public bool GetPlayerExists()
	{
		return ConnectID != "";
	}

	public bool GetLoggedIn()
	{
		return Started_LoggedIn;
	}

	public bool GetMetricSessionStarted()
	{
		return Started_Metrics;
	}

	// Get (or create if none exists) the instance of this script
	public static ChilliConnectHandlerScript GetInstance()
	{
		if ( Instance == null )
		{
			Instance = new ChilliConnectHandlerScript();
		}
		return Instance;
	}
}
