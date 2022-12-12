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

    InitialisationClient initClient;
    /*[SerializeField]*/
    GameObject clientObject;
    SocketIO client;
    GameObject playerObject;

    PlayerUIInfo ui = new PlayerUIInfo();

    private readonly ConcurrentQueue<Action> _mainThreadhActions = new ConcurrentQueue<Action>();

    private void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        initClient = clientObject.GetComponent<InitialisationClient>();
        InitPlayer();

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();

        StartCoroutine(myUpdate());
    }

    private IEnumerator myUpdate()
    {
        while(true)
        {
            // Wait until a callback action is added to the queue
            yield return new WaitUntil(() => _mainThreadhActions.Count > 0);
            // If this fails something is wrong ^^
            // simply get the first added callback
            if (!_mainThreadhActions.TryDequeue(out var action))
            {
                Debug.LogError("Something Went Wrong ! ", this);
                yield break;
            }

            // Execute the code of the added callback
            action?.Invoke();
        }
    }

    void SocketThread()
    {
        while (client == null)
        {

            Debug.Log("Client null");
            initialisationClient();
            Thread.Sleep(500);
        }

        client.On("updateLifePlayer", (data) =>
        {
            string str = data.GetValue<string>(0);
            // Simply wrap your main thread code by wrapping it in a lambda expression
            // which is enqueued to the thread-safe queue
            _mainThreadhActions.Enqueue(() =>
            {
                ui.life.text = str;
            });
        });
    }

    private void InitPlayer()
    {
        playerObject = GameObject.Find("PlayerInfo");
        int characterID = playerObject.GetComponent<PlayerInfo>().characterID;
        Debug.Log(initClient.requestURI);
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(initClient.requestURI+ "/characters/" + characterID);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());

        string jsonResponse = reader.ReadToEnd();
        Character character = JsonUtility.FromJson<Character>(jsonResponse);

        ui.name = GameObject.Find("Name").GetComponent<TextMeshProUGUI>();
        ui.life = GameObject.Find("Life").GetComponent<TextMeshProUGUI>();
        ui.lifeMax = GameObject.Find("LifeMax").GetComponent<TextMeshProUGUI>();
        ui.description = GameObject.Find("Description").GetComponent<TextMeshProUGUI>();

        ui.name.text = character.name;
        ui.life.text = character.life.ToString();
        ui.lifeMax.text = character.life.ToString();
        ui.description.text = character.description;
    }


    private void initialisationClient()
    {
        client = initClient.client;
    }
}


public class PlayerUIInfo
{
    public TextMeshProUGUI name;
    public TextMeshProUGUI life;
    public TextMeshProUGUI lifeMax;
    public TextMeshProUGUI description;
}
