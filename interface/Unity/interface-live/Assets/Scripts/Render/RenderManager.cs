using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protobuf;
using System;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

public class RenderManager : SingletonMono<RenderManager>
{
    // int cnt = 0;
    bool callTimeOver = false;
    public TextMeshProUGUI scoreBud, economyBud, hpBud;
    public TextMeshProUGUI scoreMon, economyMon, hpMon;
    public Slider hpBarBud, hpBarMon;
    public TextMeshProUGUI gameTime, fps;
    public delegate void RenderManagerCallback();
    public RenderManagerCallback onRender, onFirstFrame;

    void Start()
    {
        StartCoroutine(UpdateFrame());
    }

    IEnumerator UpdateFrame()
    {
        if (CoreParam.frameQueue.GetSize() < 50)
        {
            StartCoroutine(CalTimems(25));
            fps.text = "FPS: " + 25;
        }
        else
        {
            while (CoreParam.frameQueue.GetSize() > 100)
            {
                var frame = CoreParam.frameQueue.GetValue();
                if (frame.ToString().Contains("ATTACK"))
                {
                    Debug.Log("ATTACK");
                }
            }
            StartCoroutine(CalTimems(1250 / CoreParam.frameQueue.GetSize()));
            fps.text = "FPS: " + (1250 / CoreParam.frameQueue.GetSize());
        }
        if (!CoreParam.initialized && CoreParam.firstFrame != null)
        {
            DealFrame(CoreParam.firstFrame);
            ShowFrame();
            // onFirstFrame();
        }
        else
        {
            CoreParam.currentFrame = CoreParam.frameQueue.GetValue();
            if (CoreParam.currentFrame != null)
            {
                DealFrame(CoreParam.currentFrame);
                ShowFrame();
                // onRender();
            }
        }
        while (!callTimeOver)
                yield return 0;
        StartCoroutine(UpdateFrame());
    }

    IEnumerator CalTimems(int count)
    {
        callTimeOver = false;
        yield return new WaitForSeconds((float)count / 1000);
        callTimeOver = true;
    }

    void DealFrame(MessageToClient info)
    {
        CoreParam.characters.Clear();
        foreach (MessageOfObj obj in info.ObjMessage)
        {
            DealObj(obj);
        }
    }

    void DealObj(MessageOfObj obj)
    {
        switch (obj.MessageOfObjCase)
        {
            case MessageOfObj.MessageOfObjOneofCase.MapMessage:
                CoreParam.map = obj.MapMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.CharacterMessage:
                CoreParam.characters[obj.CharacterMessage.TeamId * 6 + obj.CharacterMessage.PlayerId - 1] = obj.CharacterMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.TeamMessage:
                CoreParam.teams[obj.TeamMessage.TeamId] = obj.TeamMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.BarracksMessage:
                CoreParam.barracks[new(obj.BarracksMessage.X, obj.BarracksMessage.Y)] = obj.BarracksMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.SpringMessage:
                CoreParam.springs[new(obj.SpringMessage.X, obj.SpringMessage.Y)] = obj.SpringMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.FarmMessage:
                CoreParam.farms[new(obj.FarmMessage.X, obj.FarmMessage.Y)] = obj.FarmMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.TrapMessage:
                CoreParam.traps[new(obj.TrapMessage.X, obj.TrapMessage.Y)] = obj.TrapMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.EconomyResourceMessage:
                CoreParam.economyResources[new(obj.EconomyResourceMessage.X, obj.EconomyResourceMessage.Y)] = obj.EconomyResourceMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.AdditionResourceMessage:
                CoreParam.additionResources[new(obj.AdditionResourceMessage.X, obj.AdditionResourceMessage.Y)] = obj.AdditionResourceMessage;
                break;
            case MessageOfObj.MessageOfObjOneofCase.NewsMessage:
                break;
            default:
                break;
        }
    }
    void ShowFrame()
    {
        if (!CoreParam.initialized)
        {
            ShowMap(CoreParam.map);
            ShowAllMessage(CoreParam.firstFrame);
            CoreParam.initialized = true;
        }
        else
        {
            ShowCharacter(CoreParam.characters);
            ShowObject(CoreParam.barracks, ref CoreParam.barracksG);
            ShowObject(CoreParam.springs, ref CoreParam.springsG);
            ShowObject(CoreParam.farms, ref CoreParam.farmsG);
            ShowObject(CoreParam.traps, ref CoreParam.trapsG);
            ShowObject(CoreParam.economyResources, ref CoreParam.economyResourcesG);
            ShowObject(CoreParam.additionResources, ref CoreParam.additionResourcesG);
            ShowAllMessage(CoreParam.currentFrame);
            // global hp bar logic in CharacterBase.cs
        }
    }
    void ShowMap(MessageOfMap map)
    {
        for (int row = 0; row < map.Height; row++)
            for (int col = 0; col < map.Width; col++)
                ObjCreater.Instance.CreateObj(map.Rows[row].Cols[col], Tool.CellToUxy(row, col));
    }
    void ShowCharacter(Dictionary<long, MessageOfCharacter> characters)
    {
        foreach (var character in characters)
        {
            if (character.Value != null)
            {
                if (!CoreParam.charactersG.ContainsKey(character.Key))
                {
                    var characterG =
                        ObjCreater.Instance.CreateObj(character.Value.CharacterType,
                            Tool.GridToUxy(character.Value.X, character.Value.Y),
                            /*Quaternion.Euler(0, 0, (float)character.Value.FacingDirection)*/
                            Quaternion.identity);
                    CoreParam.charactersG[character.Key] = characterG;
                    if (!characterG.TryGetComponent<CharacterInteract>(out var characterInteract)
                     || !characterG.TryGetComponent<CharacterBase>(out var characterBase))
                    {
                        Debug.LogError("Character prefab is missing core components.");
                        continue;
                    }
                    CharacterManager.Instance.AddCharacter(new CharacterInfo
                    {
                        ID = character.Key,
                        playerId = character.Value.PlayerId,
                        teamId = character.Value.TeamId,
                        type = character.Value.CharacterType,
                        characterBase = characterBase,
                        characterInteract = characterInteract
                    });
                }
                else
                {
                    CoreParam.charactersG[character.Key].transform.position =
                        Tool.GridToUxy(character.Value.X, character.Value.Y);
                    /*CoreParam.charactersG[character.Key].transform.rotation =
                        Quaternion.AngleAxis((float)character.Value.FacingDirection * Mathf.Rad2Deg + 180, Vector3.forward);*/
                }
            }
        }
        for (int i = 0; i < CoreParam.charactersG.Count; i++)
        {
            KeyValuePair<long, GameObject> characterG = CoreParam.charactersG.ElementAt(i);
            if (characterG.Value != null)
            {
                if (!CoreParam.characters.ContainsKey(characterG.Key))
                {
                    Destroy(characterG.Value);
                    CoreParam.charactersG.Remove(characterG.Key);
                }
            }
        }
    }
    void ShowObject<MessageOfObject>(Dictionary<Tuple<int, int>, MessageOfObject> objects, ref Dictionary<Tuple<int, int>, GameObject> objectsG)
    {
        foreach (KeyValuePair<Tuple<int, int>, MessageOfObject> obj in objects)
        {
            if (obj.Value != null)
            {
                if (!objectsG.ContainsKey(obj.Key))
                {
                    var gameObject = objectsG[obj.Key] = CreateObject(obj.Value);
                    if (gameObject.TryGetComponent(out TileInteract tileInteract))
                        tileInteract.pos = obj.Key;
                }
                else
                {
                    objectsG[obj.Key].transform.position = GetUxy(obj.Value);
                    // CoreParam.factoriesG[object.Key].transform.rotation =
                    //     Quaternion.AngleAxis((float)obj.Value.FacingDirection * Mathf.Rad2Deg + 180, Vector3.forward);
                }
            }
        }
        for (int i = 0; i < objectsG.Count; i++)
        {
            KeyValuePair<Tuple<int, int>, GameObject> objectG = objectsG.ElementAt(i);
            if (objectG.Value != null)
            {
                if (!objects.ContainsKey(objectG.Key))
                {
                    Destroy(objectG.Value);
                    objectsG.Remove(objectG.Key);
                }
            }
        }
    }

    GameObject CreateObject<MessageOfObject>(MessageOfObject obj)
    {
        var creater = ObjCreater.Instance;
        return obj switch
        {
            MessageOfBarracks barracks => creater.CreateObj(ConstructionType.Barracks,
                Tool.GridToUxy(barracks.X, barracks.Y)),
            MessageOfFarm farm => creater.CreateObj(ConstructionType.Farm,
                Tool.GridToUxy(farm.X, farm.Y)),
            MessageOfTrap trap => creater.CreateObj(trap.TrapType,
                Tool.GridToUxy(trap.X, trap.Y)),
            MessageOfEconomyResource resourceE => creater.CreateObj(resourceE.EconomyResourceType,
                Tool.GridToUxy(resourceE.X, resourceE.Y)),
            MessageOfAdditionResource resourceA => creater.CreateObj(resourceA.AdditionResourceType,
                Tool.GridToUxy(resourceA.X, resourceA.Y)),
            _ => null,
        };
    }

    Vector3 GetUxy<MessageOfObject>(MessageOfObject obj)
    {
        return obj switch
        {
            MessageOfBarracks barracks => Tool.GridToUxy(barracks.X, barracks.Y),
            MessageOfSpring spring => Tool.GridToUxy(spring.X, spring.Y),
            MessageOfFarm farm => Tool.GridToUxy(farm.X, farm.Y),
            MessageOfTrap trap => Tool.GridToUxy(trap.X, trap.Y),
            MessageOfEconomyResource resourceE => Tool.GridToUxy(resourceE.X, resourceE.Y),
            MessageOfAdditionResource resourceA => Tool.GridToUxy(resourceA.X, resourceA.Y),
            _ => Vector3.zero,
        };
    }

    void ShowAllMessage(MessageToClient messageToClient)
    {
        if (messageToClient == null) return;
        int time = Mathf.Min(messageToClient.AllMessage.GameTime, 600000);
        gameTime.text = $"{time / 60000:00} : {time % 60000 / 1000:00}.{time % 1000:000}";
        scoreBud.text = "得分：" + messageToClient.AllMessage.BuddhistsTeamScore;
        scoreMon.text = "得分：" + messageToClient.AllMessage.MonstersTeamScore;

        economyBud.text = "经济：" + messageToClient.AllMessage.BuddhistsTeamEconomy;
        economyMon.text = "经济：" + messageToClient.AllMessage.MonstersTeamEconomy;
    }

}
