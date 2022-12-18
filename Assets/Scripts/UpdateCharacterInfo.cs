using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;
using UnityEngine.SceneManagement;
using System.Net;
using System.IO;
using TMPro;

public class UpdateCharacterInfo : MonoBehaviour
{

    private InitialisationClient _initClient;
    /*[SerializeField]*/
    private GameObject _clientObject;
    private SocketIO _client;
    private GameObject _playerObject;

    private readonly PlayerUIInfo _ui = new();
    
    private void Start()
    {
        _clientObject = GameObject.Find("SocketIOClient");
        _initClient = _clientObject.GetComponent<InitialisationClient>();
        InitPlayer();

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();
    }

    private void SocketThread()
    {
        while (_client == null)
        {
            Debug.Log("Client null");
            InitialisationClient();
            Thread.Sleep(500);
        }
        Debug.Log("client : "+_client.Id);
        Debug.Log("Client updated !");
        _client.On("updateInfoCharacter", (data) =>
        {
            Debug.Log("Receive Message from the server ! ");
            
            // Simply wrap your main thread code by wrapping it in a lambda expression
            // which is enqueued to the thread-safe queue
            _initClient.MainThreadhActions.Enqueue(() =>
            {
                System.Text.Json.JsonElement json = data.GetValue(0);
                var updateInfo = JsonUtility.FromJson<UpdateInfoBody>(json.ToString());
                
                // System.Text.Json.JsonElement playerJson = data.GetValue(0);
                // PlayerInfo playerInfo = JsonUtility.FromJson<PlayerInfo>(playerJson.ToString());
                switch (updateInfo.variable)
                {
                    case "life":
                        Debug.Log("Updating Life");
                        _ui.Life.text = updateInfo.value;
                        break;
                    case "mana":
                        Debug.Log("Updating Mana");
                        _ui.Mana.text = updateInfo.value;
                        break;
                    default:
                        Debug.Log($"Unknown variable : {updateInfo.variable}");
                        break;
                }
            });
        });
    }

    private void InitPlayer()
    {
        _playerObject = GameObject.Find("PlayerInfo");
        var characterID = _playerObject.GetComponent<PlayerInfo>().characterID;
        
        var request = (HttpWebRequest)WebRequest.Create(_initClient.requestUri+ "/characters/" + characterID);
        var response = (HttpWebResponse)request.GetResponse();
        var reader = new StreamReader(response.GetResponseStream());

        var jsonResponse = reader.ReadToEnd();
        var character = JsonUtility.FromJson<Character>(jsonResponse);

        _ui.Name = GameObject.Find("Name").GetComponent<TextMeshProUGUI>();
        _ui.Life = GameObject.Find("Life").GetComponent<TextMeshProUGUI>();
        _ui.LifeMax = GameObject.Find("LifeMax").GetComponent<TextMeshProUGUI>();
        _ui.Mana = GameObject.Find("Mana").GetComponent<TextMeshProUGUI>();
        _ui.ManaMax = GameObject.Find("ManaMax").GetComponent<TextMeshProUGUI>();
        _ui.Description = GameObject.Find("Description").GetComponent<TextMeshProUGUI>();
        
        _ui.Name.text = character.name;
        _ui.Life.text = character.life.ToString();
        _ui.LifeMax.text = character.lifeMax.ToString();
        _ui.Mana.text = character.mana.ToString();
        _ui.ManaMax.text = character.manaMax.ToString();
        _ui.Description.text = character.description;
    }


    private void InitialisationClient()
    {
        _client = _initClient.Client;
    }
}

public class UpdateInfoBody
{
    public string variable;
    public string value;
}

public class PlayerUIInfo
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Life;
    public TextMeshProUGUI LifeMax;
    public TextMeshProUGUI Mana;
    public TextMeshProUGUI ManaMax;
    public TextMeshProUGUI Description;
}
