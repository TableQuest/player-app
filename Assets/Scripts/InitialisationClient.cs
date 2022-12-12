using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;

public class InitialisationClient : MonoBehaviour
{

    private readonly ConcurrentQueue<Action> _mainThreadhActions = new ConcurrentQueue<Action>();
    public SocketIO client;

    public string requestURI;

    public string clientId;
    // Start is called before the first frame update
    private IEnumerator Start()
    {



        DontDestroyOnLoad(gameObject);
        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(InitSocketThread);
        // start the thread
        thread.Start();


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


    async void InitSocketThread()
    {
        if (requestURI == null || requestURI == "")
        {
            requestURI = "http://localhost:3000";
        }

        if (client == null)
        {
            Debug.Log("requestURI");
            Debug.Log(requestURI);
            client = new SocketIO(requestURI);
            await client.ConnectAsync();
            if (clientId != null)
            {
                var json = IdToJson(clientId);
                await client.EmitAsync("playerConnection", json);
            }

        }
    }

    public string IdToJson(string id)
    {
        var menu = id.Substring(0, 2);
        var pawn = id.Substring(2);

        IdInfo idInfo = new IdInfo();
        idInfo.menuCode = menu;
        idInfo.pawnCode = pawn;

        Debug.Log(idInfo.menuCode);

        var json = JsonUtility.ToJson(idInfo);

        Debug.Log(json);
        return json;
    }
}

class IdInfo
{
    public string pawnCode;
    public string menuCode;
}
