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

    public static string fileName = "https://eesast-1255334966.cos.ap-beijing.myqcloud.com/THUAI8/arena/590d0fa5-0e09-471d-ad0e-5b705a2d2e3f/playback.thuaipb?q-sign-algorithm=sha1&q-ak=AKIDp4_eVwiRtLVJMFEQCCycBWviofp3tnWxq2WN1CY_bsBGLucp6W7EGd06z1almQYT&q-sign-time=1747329944;1747331744&q-key-time=1747329944;1747331744&q-header-list=host&q-url-param-list=&q-signature=282c527a06192c3a2ab3211a79ef91d51da5778d&x-cos-security-token=AFTbNIc8RR2k0kVDO9LQXZcyqSaevCNa5515d9cee0bd04c1ef9cfd1c9ceb53f87kQtbbuMvf2-uCvC8ADVYT5_XLv5CWLFhIZT7BT24tNqdsrSnb65UY3lsyvWKGXgSFXeh3wgDE00RTQKrT_GGdwWZnTvTaAw5_dw8_T5O4PQDhHYtVaveSuc3B0JEDzd1-OsasZQZrumelTFPEmR2VfVLzCsy7U03UOKJW9bJ82vjspLVL1BhiYyJd2c3IsO_YvDORhpjSGzLR7mBmaI4Kyc28HwR6vmJQuo1o_-rpKVQRVzWefB7MI3TZ4onon269PsJnGAbD_rRp6fb-U7Xbno8TaoSCuHqNanpTAqCstPqHf3ryFxsXoWj1V1jhPHRfHdiFJzxIr3frSnFGBYTmqPlB3rLTbvvsEKbo5yahiWCnP1OEwofyfG1qA-kzF4fO-zlnQ9d3RiOzUiMu9GW6ArnCm3SxYlaaAzmz5jPq6wsngLqjEIhXqXKJhAavFZr06mxZM_MLBzk1lBcTPDFM0XBts4FNK4JxmyUx-EcWCEkThK5wP4qyXYQEyZYHyJR1ycRdZiohpZj4d64wjHPw587LcUx7bBxbFf4fEXRkllux9s1-eKWhlO20HEZVnIitXKpGe-tajktPmhQ8iK-1ANcwhmtBZI-4TP6e5A_ZiIgmUiktiaIAELP8uEhMX-YYHa6ycrgRa9sOyiEV0He9b692FVcWGG5a-a1AmKS2nrOpi8H6KoyIovRJBYi7xO8f-v2DpzKmpZGK95Wv3BLtRPUM4kvawIQ34Rb0rgEl3gQoPV4U7xvAe1j56csydC&response-content-disposition=attachment;";
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