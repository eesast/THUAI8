using System.Collections;
using System.Collections.Generic;
using Protobuf;
using TMPro;
using UnityEngine;
using Newtonsoft.Json;
public class Debugger : SingletonMono<Debugger>
{
    const string path = @"D:\Projects\THUAI8\interface\Unity\interface-live\Assets\Debugging\";

    public string info;
    public TextMeshProUGUI text;
    public MessageOfCharacter messageOfShip1, messageOfShip2;
    public MessageOfObj messageOfObj1, messageOfObj2;
    public MessageToClient messageToClient;
    public MessageOfAll messageOfAll;
    public int shipx = 1000;
    void Start()
    {

        info = System.IO.File.ReadAllText(path + "message 0.json");
        UpdateManager.Instance.UpdateMessageByJson(info);
        info = System.IO.File.ReadAllText(path + "message 1.json");
        UpdateManager.Instance.UpdateMessageByJson(info);
        /*MessageOfSpring messageOfFort = new MessageOfSpring()
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
        UpdateManager.Instance.UpdateMessageByJson(info);*/
    }

    public void OnDebugPressed()
    {
        info = System.IO.File.ReadAllText(path + "message 2.json");
        UpdateManager.Instance.UpdateMessageByJson(info);
    }
}
