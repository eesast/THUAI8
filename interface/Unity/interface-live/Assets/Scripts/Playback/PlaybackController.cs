using Playback;
using Protobuf;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PlaybackController : SingletonMono<PlaybackController>
{
    float GameTime = 0;

    byte[] bytes = null;
    MessageToClient responseVal;
    MessageReader reader;
    public static bool isInitial;

    public static string fileName = "E://playback.thuaipb";
    // public static string fileName;
    float frequency = 0.05f;
    float timer;
    public static float playSpeed = 1;

    public static bool isMap;

    IEnumerator WebReader()
    {
        while (fileName == "")
            yield return 0;
        // if (!CoreParam.fileName.EndsWith(PlayBackConstant.ExtendedName))
        //     CoreParam.fileName += PlayBackConstant.ExtendedName;
        UnityWebRequest request = UnityWebRequest.Get(fileName);
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
            yield break;
        }
        bytes = request.downloadHandler.data;
        reader = new MessageReader(bytes);
        responseVal = reader.ReadOne();
        while (responseVal != null)
        {
            // ChartControl.GetInstance().score1.Add(new Tuple<int, int>(responseVal.AllMessage.GameTime, responseVal.AllMessage.RedTeamScore));
            // ChartControl.GetInstance().score2.Add(new Tuple<int, int>(responseVal.AllMessage.GameTime, responseVal.AllMessage.BlueTeamScore));
            // ChartControl.GetInstance().energy1.Add(new Tuple<int, int>(responseVal.AllMessage.GameTime, responseVal.AllMessage.RedTeamEnergy));
            // ChartControl.GetInstance().energy2.Add(new Tuple<int, int>(responseVal.AllMessage.GameTime, responseVal.AllMessage.BlueTeamEnergy));
            responseVal = reader.ReadOne();
        }
        // Debug.Log(ChartControl.GetInstance().score[0]);
        reader = new MessageReader(bytes);
    }

    void Start()
    {
        while (reader == null)
            StartCoroutine(WebReader());
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
                        fileName = null;
                        playSpeed = -1;
                        isMap = true;
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    }
                    Debug.Log("reader created");
                }
                timer = frequency;
                var responseVal = reader.ReadOne();
                Debug.Log($"{responseVal}");
                if (responseVal == null)
                {
                    fileName = null;
                    playSpeed = -1;
                    isMap = true;
                    SceneManager.LoadScene("GameEnd");
                }
                else if (isMap)
                {
                    CoreParam.firstFrame = responseVal;
                    isMap = false;
                }
                else
                {
                    CoreParam.frameQueue.Add(responseVal);
                    CoreParam.cnt++;
                }
            }

        }
        catch (NullReferenceException)
        {
            fileName = null;
            playSpeed = 1;
            isMap = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }
}