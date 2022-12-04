using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;

public class SendTest : MonoBehaviour
{
    
    InitialisationClient initClient;
    [SerializeField] GameObject clientObject;
    SocketIO client;

    public GameObject buttonPrefab;
    private void initialisationClient()
    {
        initClient = clientObject.GetComponent<InitialisationClient>();
        client = initClient.client;
    }

    public void testClient()
    {
        if (client == null)
        {
            initialisationClient();
        }
        //client.EmitAsync("hello", "coucou");
        createButton();
    }

    public void createButton() 
    {
        GameObject go = Instantiate(buttonPrefab);
        go.transform.position = gameObject.transform.position;
        go.GetComponent<RectTransform>().SetParent(gameObject.transform);
        go.GetComponent<Button>().onClick.AddListener(FooOnClick);

    }

    void FooOnClick()
    {
        if (client == null)
        {
            initialisationClient();
        }
        client.EmitAsync("hello", "coucou");
    }
}
