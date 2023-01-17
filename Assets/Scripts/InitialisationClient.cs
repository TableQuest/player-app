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
using UnityEngine.Serialization;

public class InitialisationClient : MonoBehaviour
{

    public readonly ConcurrentQueue<Action> MainThreadhActions = new ConcurrentQueue<Action>();
    public SocketIO Client;

    public string requestUri;
    
    public TMP_InputField idInput;
    public TMP_InputField hostInput;
    
    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        // Create a new thread in order to run the InitSocketThread method
        
        StartCoroutine(UpdateCoroutine());
    }

    private IEnumerator UpdateCoroutine()
    {
        while(true)
        {
            // Wait until a callback action is added to the queue
            yield return new WaitUntil(() => MainThreadhActions.Count > 0);
            // If this fails something is wrong ^^
            // simply get the first added callback
            if (!MainThreadhActions.TryDequeue(out var action))
            {
                Debug.LogError("Something Went Wrong ! ", this);
                yield break;
            }

            // Execute the code of the added callback
            action?.Invoke();
        }
    }

    private async void InitSocketThread()
    {
        if (!string.IsNullOrEmpty(requestUri))
        {
            Debug.Log("Connection to : " + requestUri);
            
            Client = new SocketIO(requestUri);
            await Client.ConnectAsync();
            
            var json = IdToJson(idInput.text);
            await Client.EmitAsync("playerConnection", json);
            MainThreadhActions.Enqueue(() =>
            {
                SceneManager.LoadScene("CharacterSelection");
            });
        }
    }
    
    private static string IdToJson(string id)
    {
        var menu = id.Substring(0, 2);
        var pawn = id.Substring(2);

        var idInfo = new IdInfo
        {
            menuCode = menu,
            pawnCode = pawn
        };

        var json = JsonUtility.ToJson(idInfo);
        return json;
    }
    
    public void SendIdOnClick()
    {
        Handheld.Vibrate();
        if (!string.IsNullOrEmpty(hostInput.text) && !string.IsNullOrEmpty(idInput.text))
        {
            requestUri = "http://" + hostInput.text + ":3000";
            var thread = new Thread(InitSocketThread);
            // start the thread
            thread.Start();
        }
    }
}

class IdInfo
{
    public string pawnCode;
    public string menuCode;
}
