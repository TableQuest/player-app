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

    private InitialisationClient _initClient;
    /*[SerializeField]*/
    private GameObject _clientObject;
    private SocketIO _client;

    private GameObject _playerObject;

    void Start()
    {
        _clientObject = GameObject.Find("SocketIOClient");
        _playerObject = GameObject.Find("PlayerInfo");

        if (_client == null)
        {
            InitializeClient();
        }

        var request = (HttpWebRequest) WebRequest.Create(_initClient.requestUri+"/characters");
        var response = (HttpWebResponse)request.GetResponse();
        var reader = new StreamReader(response.GetResponseStream());
        var jsonResponse = reader.ReadToEnd();

        Debug.Log(jsonResponse);

        var rootCharacter = JsonUtility.FromJson<RootCharacter>(jsonResponse);

        foreach (var t in rootCharacter.characters)
        {
            var entry = Instantiate(prefabCharacter, scrollViewContent, true);

            var character = t;
            var id = character.id;

            entry.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = character.name;
            entry.transform.Find("LifeMax").GetComponent<TextMeshProUGUI>().text = character.lifeMax.ToString();
            entry.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = character.description;
            entry.GetComponent<Button>().onClick.AddListener(delegate { ChooseCharacter(id); });
        }
    }

    private async void ChooseCharacter(int id)
    {
        await _client.EmitAsync("characterSelection", id);
        _playerObject.GetComponent<PlayerInfo>().characterID = id;
        SceneManager.LoadScene("MainApp");
    }

    private void InitializeClient()
    {
        _initClient = _clientObject.GetComponent<InitialisationClient>();
        _client = _initClient.Client;
    }
}

[System.Serializable]
public class RootCharacter
{
    public Character[] characters;
}

[System.Serializable]
public class Skill
{
    public int id;
    public string name;
    public int manaCost;
    public int range;
    public int maxTarget;
    public string type;
    public int statModifier;
    public bool healing;
    public string image;
}

[System.Serializable]
public class Character
{
    public int id;
    public string name;
    public int lifeMax;
    public int life;
    public int manaMax;
    public int mana;
    public int speed;
    public string description;
    public Skill[] skills;
}
