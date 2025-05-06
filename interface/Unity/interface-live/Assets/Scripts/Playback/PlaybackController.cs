using Playback;
using Protobuf;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlaybackController : MonoBehaviour
{

    byte[] bytes = null;

    IEnumerator WebReader(string uri)
    {
        //if (!fileName.EndsWith(PlayBackConstant.ExtendedName))
        //{
        //    fileName += PlayBackConstant.ExtendedName;
        //}
        //var uri = new Uri(Path.Combine(Application.streamingAssetsPath, fileName));
        //Debug.Log(uri.AbsoluteUri);

        //UnityWebRequest request = UnityWebRequest.Get(uri.AbsoluteUri);
        UnityWebRequest request = UnityWebRequest.Get(uri);
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (request.error != null)
        {
            Debug.Log(request.error);
            filename = null;
            playSpeed = 1;
            isMap = true;
            isInitial = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield break;
        }
        bytes = request.downloadHandler.data;
    }

    void Start()
    {
        filename = "E:\\playback.thuaipb";
        StartCoroutine(WebReader(filename));
        timer = frequency;
        isMap = true;
        isInitial = false;
    }

    void Update()
    {
        timer -= Time.deltaTime * playSpeed;

        try
        {
            if (timer < 0 && bytes != null)
            {
                if (reader == null)
                {
                    try
                    {
                        StopAllCoroutines();
                        reader = new MessageReader(bytes);
                    }
                    catch (FileFormatNotLegalException)
                    {
                        filename = null;
                        playSpeed = -1;
                        isMap = true;
                        isInitial = false;
                        SceneManager.LoadScene("Playback");
                    }
                    Debug.Log("reader created");
                }
                timer = frequency;
                var responseVal = reader.ReadOne();
                Debug.Log($"{responseVal}");
                if (responseVal == null)
                {
                    filename = null;
                    playSpeed = -1;
                    isMap = true;
                    isInitial = false;
                    SceneManager.LoadScene("GameEnd");
                }
                else if (isMap)
                {
                    map = responseVal.ObjMessage[0].MapMessage;
                    isMap = false;
                }
                else if (!isInitial)
                {
                    Receive(responseVal);
                    isInitial = true;
                }
                else
                {
                    Receive(responseVal);
                }
            }

        }
        catch (NullReferenceException)
        {
            filename = null;
            playSpeed = 1;
            isMap = true;
            isInitial = false;
            SceneManager.LoadScene("Playback");
        }

    }

    private void Receive(MessageToClient message)
    {
        //Debug.Log(message.ToString());
        time = message.AllMessage.GameTime;
        foreach (var messageOfObj in message.ObjMessage)
        {
            // switch (messageOfObj.MessageOfObjCase)
            // {
            //     case MessageOfObj.MessageOfObjOneofCase.StudentMessage:
            //         Student[studentCnt] = messageOfObj.StudentMessage; studentCnt++; break;
            //     case MessageOfObj.MessageOfObjOneofCase.TrickerMessage:
            //         tricker = messageOfObj.TrickerMessage; break;
            //     case MessageOfObj.MessageOfObjOneofCase.ClassroomMessage:
            //         classroom[classroomCnt] = messageOfObj.ClassroomMessage; classroomCnt++; break;
            //     case MessageOfObj.MessageOfObjOneofCase.PropMessage:
            //         break;
            //     case MessageOfObj.MessageOfObjOneofCase.HiddenGateMessage:
            //         break;
            //     default: break;
            // }
        }
    }

    public static string filename;
    MessageReader reader;
    float frequency = 0.05f;
    float timer;
    public static int playSpeed = 1;

    public static MessageOfMap map;
    public static bool isMap;
    public static bool isInitial;
    public static int time;
}