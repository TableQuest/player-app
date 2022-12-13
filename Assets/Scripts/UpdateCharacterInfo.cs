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

        _client.On("updateLifePlayer", (data) =>
        {
            var str = data.GetValue<string>(0);
            // Simply wrap your main thread code by wrapping it in a lambda expression
            // which is enqueued to the thread-safe queue
            _initClient.MainThreadhActions.Enqueue(() =>
            {
                _ui.Life.text = str;
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
        _ui.Description = GameObject.Find("Description").GetComponent<TextMeshProUGUI>();

        _ui.Name.text = character.name;
        _ui.Life.text = character.life.ToString();
        _ui.LifeMax.text = character.life.ToString();
        _ui.Description.text = character.description;
    }


    private void InitialisationClient()
    {
        _client = _initClient.Client;
    }
}


public class PlayerUIInfo
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Life;
    public TextMeshProUGUI LifeMax;
    public TextMeshProUGUI Description;
}
