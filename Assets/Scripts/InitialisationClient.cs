using System.Collections;
using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System.Net;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class InitialisationClient : MonoBehaviour
{

    public readonly ConcurrentQueue<Action> MainThreadhActions = new ConcurrentQueue<Action>();
    public SocketIO Client;

    public string requestUri;
    
    public string playerId;

    public bool isNewCharacter = true;
    public CharacterInfo reloadedCharacterInfo = new CharacterInfo();
    
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

            var json = IdToJson(playerId);
            await Client.EmitAsync("playerConnection", json);

            Debug.Log("Sending " + requestUri+"/players/"+playerId+"/characterInfo");
            var requestCheckIfCharExist = (HttpWebRequest) WebRequest.Create(requestUri+"/players/"+playerId+"/characterInfo");
            var response = (HttpWebResponse)requestCheckIfCharExist.GetResponse();
            int responseCode = (int)response.StatusCode;

            Debug.Log("Received code: " + responseCode);

            MainThreadhActions.Enqueue(() =>
            {
                if (responseCode == 204) {
                    isNewCharacter = true;
                    SceneManager.LoadScene("CharacterSelection");
                }
                else {
                    Debug.Log("Reading Character information.");

                    
                    var reader = new StreamReader(response.GetResponseStream());
                    var jsonResponse = reader.ReadToEnd();
                    reloadedCharacterInfo = JsonUtility.FromJson<CharacterInfo>(jsonResponse);
                    isNewCharacter = false;

                    GameObject _playerObject = GameObject.Find("PlayerInfo");
                    _playerObject.GetComponent<PlayerInfo>().characterID = reloadedCharacterInfo.id;

                    Debug.Log("Loading MainApp");

                    SceneManager.LoadScene("MainApp");
                }
            });

        }
    }

    public void closeApp()
    {
        Debug.Log("Closing application.");
        //thanks to http://answers.unity.com/answers/1157271/view.html
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    async void OnApplicationQuit() {
        await Client.DisconnectAsync();
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
    
    public void SendIdOnClick(string scannedtext)
    {
        string scannedUri = scannedtext.Split(" ")[0];
        string scannedId = scannedtext.Split(" ")[1];
        Handheld.Vibrate();
        if (!string.IsNullOrEmpty(scannedId) && !string.IsNullOrEmpty(scannedUri))
        {
            requestUri = scannedUri;
            playerId = scannedId;
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

public class CharacterInfo {
    public int id;
    public int life;
    public int mana;
}
