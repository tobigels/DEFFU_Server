using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    #region FIELDS

    private int gameFrame;
    private ConnectionManager connectionManager;
    private Player[] allPlayers;
    private int playerCount;
    private bool gameStarted;
    private float accumulatedTime;

    private float gameTurn_length = 0.1f;

    public static PlayerManager instance = null;

    #endregion

    #region METHODS

    // --------------------------------------- Private methods ---------------------------------------

    private void Update() {
        connectionManager.CheckForIncomingData();
        if(gameStarted) {
            accumulatedTime += Time.deltaTime;

            while(accumulatedTime > gameTurn_length) {

                connectionManager.ExecuteGameTurn();
                
                gameFrame++;

                accumulatedTime -= gameTurn_length;
            }
        }
    }
    
    private void Awake() {
        connectionManager = new ConnectionManager(this);
        playerCount = 0;
        allPlayers = new Player[connectionManager.MAX_CONNECTIONS];
        gameStarted = false;
        accumulatedTime = 0f;

        if (instance == null) {
            instance = this;
        } else {
            if (instance != this) {
                Destroy(this);
            }
        }

        DontDestroyOnLoad(gameObject);
    }

    // --------------------------------------- Public methods ---------------------------------------

    /// <summary>
    /// 
    /// </summary>
    public int GameFrame {
        get {
            return gameFrame;
        }
        set {
            gameFrame = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public Player[] AllPlayers {
        get {
            return allPlayers;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public  ConnectionManager ConnectionManager {
        get {
            return connectionManager;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool GameStarted {
        get {
            return gameStarted;
        }
        set {
            gameStarted = value;
        }
    }

    public float GameTurn_length {
        get {
            return gameTurn_length;
        }
        set {
            gameTurn_length = value;
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    public bool AddPlayer(int id, string name) {
        if (allPlayers[id - 1].Id == 0) {
            allPlayers[id - 1] = new Player(id, name);
            playerCount++;

            return true;
        } else {
            Debug.Log("ERROR: Id already set; can't add player");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public bool RemovePlayer(int id) {
        if(allPlayers[id - 1].Id != 0) {
            allPlayers[id - 1].Id = 0;
            allPlayers[id - 1].Name = "";
            playerCount--;

            return true;
        } else {
            Debug.Log("ERROR: Id not set; can't remove player");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void InitializePlayerArray() {

        for(int i = 0; i < allPlayers.Length; i++) {
            allPlayers[i] = new Player(0, "");
        }

        connectionManager.StartServer();
    }

    #endregion
}