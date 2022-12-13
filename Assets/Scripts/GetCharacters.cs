using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using TMPro;
using SocketIOClient;
using UnityEditor.PackageManager;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GetCharacters : MonoBehaviour
{
    [SerializeField] private GameObject prefabCharacter;
    [SerializeField] private Transform scrollViewContent;

    InitialisationClient initClient;
    /*[SerializeField]*/
    GameObject clientObject;
    SocketIO client;

    GameObject playerObject;

    void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        playerObject = GameObject.Find("PlayerInfo");

        if (client == null)
        {
            IntializeClient();
        }

        HttpWebRequest request = (HttpWebRequest) WebRequest.Create(initClient.requestURI+"/characters");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string jsonResponse = reader.ReadToEnd();

        Debug.Log(jsonResponse);

        RootCharacter rootCharacter = JsonUtility.FromJson<RootCharacter>(jsonResponse);

        for (int i = 0; i < rootCharacter.characters.Length; i++)
        {
            var entry = Instantiate(prefabCharacter);

            Character character = rootCharacter.characters[i];
            var id = character.id;

            entry.transform.SetParent(scrollViewContent);
            entry.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = character.name;
            entry.transform.Find("LifeMax").GetComponent<TextMeshProUGUI>().text = character.lifeMax.ToString();
            entry.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = character.description;
            entry.GetComponent<Button>().onClick.AddListener(delegate { ChooseCharacter(id); });
        }
    }

    private async void ChooseCharacter(int id)
    {
        await client.EmitAsync("characterSelection", id);
        playerObject.GetComponent<PlayerInfo>().characterID = id;
        SceneManager.LoadScene("MainApp");
    }

    private void IntializeClient()
    {
        initClient = clientObject.GetComponent<InitialisationClient>();
        client = initClient.client;
    }
}



[System.Serializable]
public class RootCharacter
{
    public Character[] characters;
}

[System.Serializable]
public class Character
{
    public int id;
    public string name;
    public int lifeMax;
    public int life;
    public string description;
}
