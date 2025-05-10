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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            yield break;
        }
        bytes = request.downloadHandler.data;
    }

    void Start()
    {
#if UNITY_EDITOR
        filename = "E:\\playback.thuaipb";
        StartCoroutine(WebReader(filename));
#endif
        timer = frequency;
        isMap = true;
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
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            filename = null;
            playSpeed = 1;
            isMap = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }

    public static string filename;
    MessageReader reader;
    float frequency = 0.05f;
    float timer;
    public static int playSpeed = 1;

    public static bool isMap;
}