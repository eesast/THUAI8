using System.Collections;
using System.Collections.Generic;
using Protobuf;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class UpdateManager : SingletonMono<UpdateManager>
{
    JsonSerializerSettings jSetting = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
    };

    public void UpdateMessageByJson(string jsonInfo)
    {
        jsonInfo = jsonInfo.Replace("List\"", "\"");
        if (!CoreParam.initialized && jsonInfo.Contains("mapMessage"))
            CoreParam.firstFrame = JsonConvert.DeserializeObject<MessageToClient>(jsonInfo, jSetting);
        else
            CoreParam.frameQueue.Add(JsonConvert.DeserializeObject<MessageToClient>(jsonInfo, jSetting));
        CoreParam.cnt++;
        Debug.Log("UpdateManager.UpdateMessageByJson()");
    }

    public void UpdateMessageByBytes(byte[] bytes)
    {
        var frame = MessageToClient.Parser.ParseFrom(bytes);
        if (!CoreParam.initialized && frame.ObjMessage.Any(x => x.MapMessage != null))
            CoreParam.firstFrame = frame;
        else
            CoreParam.frameQueue.Add(frame);
        CoreParam.cnt++;
        Debug.Log("UpdateManager.UpdateMessageByBytes()");
    }
}