using System.Collections;
using System.Collections.Generic;
using Protobuf;
using TMPro;
using UnityEngine;
using Newtonsoft.Json;
public class Debugger : SingletonMono<Debugger>
{
    // const string path = @"D:\Projects\THUAI8\interface\Unity\interface-live\Assets\Debugging\";

    // public string info;
    public GameObject debugPanel;
    public string state = "##########";
    public static bool debug = false;

    void Start()
    {
        debugPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D)) state = state[1..] + "D";
        if (Input.GetKeyDown(KeyCode.E)) state = state[1..] + "E";
        if (Input.GetKeyDown(KeyCode.B)) state = state[1..] + "B";
        if (Input.GetKeyDown(KeyCode.U)) state = state[1..] + "U";
        if (Input.GetKeyDown(KeyCode.G)) state = state[1..] + "G";
        if (state.EndsWith("DEBUG"))
        {
            debug = !debug;
            debugPanel.SetActive(debug);
            state = "##########";
        }

    }

    public void PrintCurrentJSON()
    {
        if (CoreParam.currentFrame != null)
        {
            Debug.Log(CoreParam.currentFrame.ToString());
        }
        else
        {
            Debug.Log("No frames in the queue.");
        }
    }
    /*void Start()
    {

        info = System.IO.File.ReadAllText(path + "message 0.json");
        UpdateManager.Instance.UpdateMessageByJson(info);
        info = System.IO.File.ReadAllText(path + "message 1.json");
        UpdateManager.Instance.UpdateMessageByJson(info);
        MessageOfSpring messageOfSpring = new MessageOfSpring()
        {
            X = 45,
            Y = 50,
            Hp = 100,
            TeamId = 1,
        };
        MessageOfObj messageOfObj = new MessageOfObj()
        {
            SpringMessage = messageOfFort,
        };
        messageToClient = JsonConvert.DeserializeObject<MessageToClient>(info, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        });
        messageToClient.ObjMessage.Add(messageOfObj);
        info = JsonConvert.SerializeObject(messageToClient);
        UpdateManager.Instance.UpdateMessageByJson(info);
    }

    public void OnDebugPressed()
    {
        info = System.IO.File.ReadAllText(path + "message 2.json");
        UpdateManager.Instance.UpdateMessageByJson(info);
    }*/
}
