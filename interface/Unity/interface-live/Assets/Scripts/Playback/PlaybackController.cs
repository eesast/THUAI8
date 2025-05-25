using Playback;
using Protobuf;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlaybackController : SingletonMono<PlaybackController>
{

    byte[] bytes = null;
    List<MessageToClient> frames;
    int currentFrame;
#if !UNITY_EDITOR
    public static string fileName;
#else
    public static string fileName = "http://localhost/playback.thuaipb";
#endif
    public static bool fileNameFlag;
    float deltaTime = 0.05f;
    float timer;
    public static float playSpeed = 1;

    public static bool isMap = true;
    public TextMeshProUGUI speedText;
    public Slider progressBar;

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

        if (request.error != null)
        {
            Debug.LogError(request.error);
            yield break;
        }
        bytes = request.downloadHandler.data;
        print(System.Text.Encoding.UTF8.GetString(bytes));
        var reader = new MessageReader(bytes);
        frames = reader.ReadAll();
        if (frames.Count == 0)
        {
            Debug.LogError("Playback file is empty or not valid.");
            frames = null;
            yield break;
        }
        progressBar.maxValue = frames.Count - 1;
    }

    void Start()
    {
        StartCoroutine(WebReader());
        currentFrame = 0;
        progressBar.onValueChanged.AddListener((value) => SetProgress((int)value));
    }

    void Update()
    {
        timer -= Time.deltaTime * playSpeed;

        try
        {
            if (timer < 0 && bytes != null && frames != null)
            {
                if (isMap)
                {
                    CoreParam.firstFrame = frames[currentFrame++];
                    isMap = false;
                    return;
                }
                int frameToSkip = Mathf.FloorToInt(-timer / deltaTime);
                currentFrame += frameToSkip;
                timer = deltaTime;
                progressBar.SetValueWithoutNotify(currentFrame);
                if (currentFrame < frames.Count)
                {
                    CoreParam.frameQueue.Add(frames[currentFrame++]);
                    CoreParam.cnt++;
                }
            }
            else if (playSpeed == 0)
            {
                if (currentFrame < frames.Count)
                {
                    CoreParam.frameQueue.Add(frames[currentFrame]);
                    CoreParam.cnt++;
                }
            }

        }
        catch (NullReferenceException)
        {
            fileName = null;
            playSpeed = 1;
            currentFrame = 0;
            isMap = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

    }

    public void SetSpeed(float speed)
    {
        playSpeed = speed;
        // Time.timeScale = speed;
        speedText.text = $"{speed:0}x";
        CoreParam.frameQueue.Clear();
        CoreParam.currentFrame = frames[currentFrame];
    }

    public void SetProgress(int frameCount)
    {
        if (frames == null || frames.Count == 0)
            return;
        if (frameCount < 0 || frameCount >= frames.Count)
        {
            Debug.LogWarning("Frame count out of bounds.");
            return;
        }
        currentFrame = frameCount;
        timer = 0;
        CoreParam.frameQueue.Clear();
        CoreParam.currentFrame = frames[currentFrame];
    }
}