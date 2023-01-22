using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Net;
using System.IO;
using System;
using UnityEngine.Networking;

public class UpdateCharacterInfo : MonoBehaviour
{

    private InitialisationClient _initClient;
    /*[SerializeField]*/
    private GameObject _clientObject;
    private SocketIO _client;
    private GameObject _playerObject;

    public readonly PlayerUIInfo _ui = new();

    [SerializeField]
    GameObject skillPanelPrefab;

    [SerializeField]
    GameObject logPanelPrefab;

    public TextMeshProUGUI helpText;

    private void Start()
    {
        _clientObject = GameObject.Find("SocketIOClient");
        _initClient = _clientObject.GetComponent<InitialisationClient>();
        StartCoroutine(InitPlayer());
        StartCoroutine(loadLogsFromServer());

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();
        StartCoroutine(InitPlayer());
    }

    private void SocketThread()
    {
        while (_client == null)
        {
            InitialisationClient();
            Thread.Sleep(500);
        }
        _client.On("updateInfoCharacter", (data) =>
        {
            Debug.Log("Receive updateInfoCharacter from the server ! ");
            
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

        _client.On("log", (data) =>
        {
            _initClient.MainThreadhActions.Enqueue(() =>
            {
                LogInfo logInfo = JsonUtility.FromJson<LogInfo>(data.GetValue(0).ToString());
                createLogPanel(logInfo);
            });
        });
        
        _client.On("helpTurn", (data) =>
        {
            // Debug.Log("Receive Help Turn !"+data.GetValue().ToString());
            _initClient.MainThreadhActions.Enqueue(() =>
            {
                var help = JsonUtility.FromJson<HelpTurn>(data.GetValue().ToString());
                if (help.isTurn)
                {
                    Handheld.Vibrate();
                }
                helpText.text = help.text;
            });
        });
    }

    private IEnumerator InitPlayer()
    {
        _playerObject = GameObject.Find("PlayerInfo");
        var characterID = _playerObject.GetComponent<PlayerInfo>().characterID;
        
        // var request = (HttpWebRequest)WebRequest.Create(_initClient.requestUri+ "/characters/" + characterID);
        // var response = (HttpWebResponse)request.GetResponse();
        // var reader = new StreamReader(response.GetResponseStream());
        //
        // var jsonResponse = reader.ReadToEnd();

        var www = UnityWebRequest.Get(_initClient.requestUri + "/characters/" + characterID);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            var jsonResponse = www.downloadHandler.text;
            var character = JsonUtility.FromJson<Character>(jsonResponse);

            Sprite sprite = Resources.Load<Sprite>("Images/Dwarf");
            if (character.name == "Elf")
            {
                sprite = Resources.Load<Sprite>("Images/Elf");

            }
        
            _ui.image = gameObject.transform.Find("GlobalPanel").Find("Image").GetComponent<Image>();
            _ui.image.sprite = sprite;

            // Basic Info Panel
            Transform basicInfoPanel = gameObject.transform.Find("GlobalPanel").Find("BasicInfoPanel");

            _ui.Name = basicInfoPanel.Find("Name").GetComponent<TextMeshProUGUI>();
            _ui.Life = basicInfoPanel.Find("Life").GetComponent<TextMeshProUGUI>();
            _ui.LifeMax = basicInfoPanel.Find("LifeMax").GetComponent<TextMeshProUGUI>();
            _ui.Mana = basicInfoPanel.Find("Mana").GetComponent<TextMeshProUGUI>();
            _ui.ManaMax = basicInfoPanel.Find("ManaMax").GetComponent<TextMeshProUGUI>();
            _ui.Description = basicInfoPanel.Find("Description").GetComponent<TextMeshProUGUI>();

            _ui.Name.text = character.name;
            _ui.Life.text = (!_initClient.isNewCharacter ? _initClient.reloadedCharacterInfo.life.ToString() : character.life.ToString());
            _ui.LifeMax.text = character.lifeMax.ToString();
            _ui.Mana.text = (!_initClient.isNewCharacter ? _initClient.reloadedCharacterInfo.mana.ToString() : character.mana.ToString());
            _ui.ManaMax.text = character.manaMax.ToString();
            _ui.Description.text = character.description;

            // Skills Panel
            Transform SkillsPanel = gameObject.transform.Find("GlobalPanel").Find("SkillsPanel");

            foreach (Skill s in character.skills)
            {
                GameObject skillPanel = Instantiate(skillPanelPrefab);

                skillPanel.transform.SetParent(SkillsPanel.Find("ScrollView").Find("Viewport").Find("Content"));
                setSkillPanel(s, skillPanel.transform);
            }   
        }
    }

    private void setSkillPanel(Skill skill, Transform skillPanel)
    {
        // Image 
        Sprite sprite = Resources.Load<Sprite>(skill.image);
        skillPanel.Find("Image").GetComponent<Image>().sprite = sprite;

        // Name
        skillPanel.Find("Name").GetComponent<TextMeshProUGUI>().text = skill.name;

        // Mana
        TextMeshProUGUI inputFieldMana = skillPanel.Find("ManaCost").GetComponent<TextMeshProUGUI>();
        inputFieldMana.text = skill.manaCost.ToString();

        // Range
        TextMeshProUGUI inputFieldRange = skillPanel.Find("Range").GetComponent<TextMeshProUGUI>();
        inputFieldRange.text = skill.range.ToString();

        // Damage
        TextMeshProUGUI inputFieldDamage = skillPanel.Find("Damage").GetComponent<TextMeshProUGUI>();
        inputFieldDamage.text = skill.statModifier.ToString();

        skillPanel.transform.localScale = new Vector3(1, 1, 1);
    }

    IEnumerator loadLogsFromServer() {
        var www = UnityWebRequest.Get(_initClient.requestUri+"/logs");
    
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log(www.error);
        }
        else {
            // Show results as text
            var jsonResponse = www.downloadHandler.text;
            var logInfos = JsonUtility.FromJson<LogList>(jsonResponse);

            Debug.Log(jsonResponse);
            Debug.Log(logInfos.listOfLogs);
            foreach (LogInfo logInfo in logInfos.listOfLogs)
            {
                createLogPanel(logInfo);
            }
        }
    }

    private void createLogPanel(LogInfo logInfo)
    {
        Debug.Log("Logging " + logInfo.logText);

        GameObject logPanel = Instantiate(logPanelPrefab);
        Transform logScrollView = gameObject.transform.Find("GlobalPanel").Find("LogsPanel").Find("ScrollView");
        logPanel.transform.SetParent(logScrollView.Find("Viewport").Find("Content"));

        fillLogPanel(logInfo, logPanel.transform);
        
        logScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
    }

    private void fillLogPanel(LogInfo log, Transform logPanelTransform)
    {
        // Image 
        Sprite sprite = Resources.Load<Sprite>(log.imagePath);
        logPanelTransform.Find("HeaderGroup").Find("Image").GetComponent<Image>().sprite = sprite;

        //Title
        logPanelTransform.Find("HeaderGroup").Find("NameText").GetComponent<TextMeshProUGUI>().text = log.title;

        //Content
        logPanelTransform.Find("LogText").GetComponent<TextMeshProUGUI>().text = log.logText;

        logPanelTransform.transform.localScale = Vector3.one;
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
    public Image image;
}

[Serializable]
public class LogInfo
{
    public string imagePath;
    public string title;
    public string logText;
}

[Serializable]
public class LogList
{
    public List<LogInfo> listOfLogs;
}
public class HelpTurn
{
    public bool isTurn;
    public string text;
}
