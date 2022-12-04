using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SocketIOClient;
using UnityEngine.SceneManagement;

public class SendID : MonoBehaviour
{

    public TMP_InputField idInput;

    InitialisationClient initClient;
    /*[SerializeField]*/ GameObject clientObject;
    SocketIO client;

    public void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
    }

    private void intializeClient()
    {
        initClient = clientObject.GetComponent<InitialisationClient>();
        client = initClient.client;
    }

    public void SendIdOnClick()
    {
        if (client == null)
        {
            intializeClient();
        }
        string json = initClient.IdToJson(idInput.text);
        initClient.clientId = idInput.text;
        client.EmitAsync("playerConnection", json);

        SceneManager.LoadScene("CharacterSelection");
    }
}

