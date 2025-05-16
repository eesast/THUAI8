using Newtonsoft.Json;
using Playback;
using Protobuf;
using Spine;
using System;
using System.Collections;
using System.IO;
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
#if !UNITY_EDITOR
    public static string fileName;
    public static bool fileNameFlag;
#else
    public static string fileName = "http://localhost/playback.thuaipb";
#endif
    float frequency = 0.05f;
    float timer;
    public static float playSpeed = 1;

    public static bool isMap;

    IEnumerator WebReader()
    {
#if !UNITY_EDITOR
        while (fileName == "" || fileName == null || !fileNameFlag)
            yield return 0;
#endif
        // if (!CoreParam.fileName.EndsWith(PlayBackConstant.ExtendedName))
        //     CoreParam.fileName += PlayBackConstant.ExtendedName;
        Debug.Log("WebReader(), fileName = " + fileName);
        UnityWebRequest request = UnityWebRequest.Get(fileName);
        request.timeout = 10;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogError(request.error);
            yield break;
        }
        bytes = request.downloadHandler.data;
        print(System.Text.Encoding.UTF8.GetString(bytes));
        reader = new MessageReader(bytes);
        responseVal = reader.ReadOne();
        while (responseVal != null)
        {
            responseVal = reader.ReadOne();
        }
        reader = new MessageReader(bytes);
    }

    void Start()
    {
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