using UnityEngine;
using UnityEngine.UI;

public class UnityInterface : MonoBehaviour {

    #region FIELDS

    private PlayerManager playerManager;
    private Text[] text_clients;

    public Text text_ip;
    public GameObject clients_wrapper;

    #endregion

    #region METHODS

    // --------------------------------------- Private methods ---------------------------------------

    /// <summary>
    /// 
    /// </summary>
    private void Update() {
        text_ip.text = playerManager.ConnectionManager.LocalIp;

        for (int i = 0; i < playerManager.ConnectionManager.MAX_CONNECTIONS; i++) {
            text_clients[i].text = playerManager.AllPlayers[i].Name;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start() {
        playerManager = PlayerManager.instance;

        playerManager.InitializePlayerArray();
        text_clients = clients_wrapper.GetComponentsInChildren<Text>();
    }

    // --------------------------------------- Public methods ---------------------------------------
    #endregion
}