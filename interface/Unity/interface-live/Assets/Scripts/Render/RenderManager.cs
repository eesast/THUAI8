using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protobuf;
using System;
using Newtonsoft.Json;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using TMPro;
using System.Linq;
using UnityEngine.Rendering;
using UnityEditor.VersionControl;

public class RenderManager : SingletonMono<RenderManager>
{
    // int cnt = 0;
    bool callTimeOver = false;
    public TextMeshProUGUI gameTime, score, economy, fi;

    void Start()
    {
        StartCoroutine(UpdateFrame());
    }

    IEnumerator UpdateFrame()
    {
        if (CoreParam.frameQueue.GetSize() < 50)
        {
            StartCoroutine(CalTimems(25));
            fi.text = "fi: " + 25;
        }
        else
        {
            while (CoreParam.frameQueue.GetSize() > 100)
            {
                CoreParam.frameQueue.GetValue();
            }
            StartCoroutine(CalTimems(1250 / CoreParam.frameQueue.GetSize()));
            fi.text = "fi: " + (1250 / CoreParam.frameQueue.GetSize());
        }
        if (!CoreParam.initialized && CoreParam.firstFrame != null)
        {
            DealFrame(CoreParam.firstFrame);
            ShowFrame();
        }
        CoreParam.currentFrame = CoreParam.frameQueue.GetValue();
        if (CoreParam.currentFrame != null)
        {
            DealFrame(CoreParam.currentFrame);
            ShowFrame();
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
                CoreParam.characters[obj.CharacterMessage.TeamId * 4 + obj.CharacterMessage.PlayerId - 1] = obj.CharacterMessage;
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
            ShowObject(CoreParam.barracks, CoreParam.barracksG);
            ShowObject(CoreParam.springs, CoreParam.springsG);
            ShowObject(CoreParam.farms, CoreParam.farmsG);
            ShowObject(CoreParam.traps, CoreParam.trapsG);
            ShowObject(CoreParam.economyResources, CoreParam.economyResourcesG);
            ShowObject(CoreParam.additionResources, CoreParam.additionResourcesG);
            ShowAllMessage(CoreParam.currentFrame);
        }
    }
    void ShowMap(MessageOfMap map)
    {
        for (int row = 0; row < map.Height; row++)
            for (int col = 0; col < map.Width; col++)
                ObjCreater.GetInstance().CreateObj(map.Rows[row].Cols[col], Tool.CellToUxy(row, col));
    }
    void ShowCharacter(Dictionary<long, MessageOfCharacter> characters)
    {
        foreach (var character in characters)
        {
            if (character.Value != null)
            {
                if (!CoreParam.charactersG.ContainsKey(character.Key))
                {
                    CoreParam.charactersG[character.Value.TeamId * 4 + character.Value.PlayerId - 1] =
                        ObjCreater.GetInstance().CreateObj(character.Value.CharacterType,
                            Tool.GridToUxy(character.Value.X, character.Value.Y),
                            /*Quaternion.Euler(0, 0, (float)character.Value.FacingDirection)*/
                            Quaternion.identity);
                    /*RendererControl.GetInstance().SetColToChild((PlayerTeam)(character.Value.TeamId + 1),
                        CoreParam.charactersG[character.Value.TeamId * 4 + character.Value.PlayerId - 1].transform);*/
                }
                else
                {
                    CoreParam.charactersG[character.Value.TeamId * 4 + character.Value.PlayerId - 1].transform.position =
                        Tool.GridToUxy(character.Value.X, character.Value.Y);
                    /*CoreParam.charactersG[character.Value.TeamId * 4 + character.Value.PlayerId - 1].transform.rotation =
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
                }
            }
        }
    }
    void ShowObject<MessageOfObject>(Dictionary<Tuple<int, int>, MessageOfObject> objects, Dictionary<Tuple<int, int>, GameObject> objectsG)
    {
        foreach (KeyValuePair<Tuple<int, int>, MessageOfObject> obj in objects)
        {
            if (obj.Value != null)
            {
                if (!objectsG.ContainsKey(obj.Key))
                {
                    objectsG[obj.Key] = CreateObject(obj.Value);
                    // RendererControl.GetInstance().SetColToChild((PlayerTeam)(obj.Value.TeamId + 1),
                    //     CoreParam.factoriesG[obj.Key].transform, 5);
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
        var creater = ObjCreater.GetInstance();
        return obj switch
        {
            MessageOfBarracks barracks => creater.CreateObj(ConstructionType.Barracks,
                Tool.GridToUxy(barracks.X, barracks.Y)),
            MessageOfSpring spring => creater.CreateObj(ConstructionType.Spring,
                Tool.GridToUxy(spring.X, spring.Y)),
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
    /*void ShowBarracks(Dictionary<Tuple<int, int>, MessageOfBarracks> barracks)
    {
        foreach (KeyValuePair<Tuple<int, int>, MessageOfBarracks> barrack in barracks)
        {
            if (barrack.Value != null)
            {
                if (!CoreParam.barracksG.ContainsKey(barrack.Key))
                {
                    CoreParam.barracksG[barrack.Key] =
                        ObjCreater.GetInstance().CreateObj(ConstructionType.Barracks,
                            Tool.GridToUxy(barrack.Value.X, barrack.Value.Y));
                    // RendererControl.GetInstance().SetColToChild((PlayerTeam)(barrack.Value.TeamId + 1),
                    //     CoreParam.factoriesG[barrack.Key].transform, 5);

                }
                else
                {
                    CoreParam.barracksG[barrack.Key].transform.position = Tool.GridToUxy(barrack.Value.X, barrack.Value.Y);
                    // CoreParam.factoriesG[barrack.Key].transform.rotation =
                    //     Quaternion.AngleAxis((float)barrack.Value.FacingDirection * Mathf.Rad2Deg + 180, Vector3.forward);
                }
            }
        }
        for (int i = 0; i < CoreParam.barracksG.Count; i++)
        {
            KeyValuePair<Tuple<int, int>, GameObject> barrackG = CoreParam.barracksG.ElementAt(i);
            if (barrackG.Value != null)
            {
                if (!CoreParam.barracks.ContainsKey(barrackG.Key))
                {
                    Destroy(barrackG.Value);
                    CoreParam.barracksG.Remove(barrackG.Key);
                }
            }
        }
    }
    void ShowSpring(Dictionary<Tuple<int, int>, MessageOfSpring> springs)
    {
        foreach (KeyValuePair<Tuple<int, int>, MessageOfSpring> spring in springs)
        {
            if (spring.Value != null)
            {
                if (!CoreParam.springsG.ContainsKey(spring.Key))
                {
                    CoreParam.springsG[spring.Key] =
                        ObjCreater.GetInstance().CreateObj(ConstructionType.Spring,
                            Tool.GridToUxy(spring.Value.X, spring.Value.Y));
                    // RendererControl.GetInstance().SetColToChild((PlayerTeam)(spring.Value.TeamId + 1),
                    //     CoreParam.springsG[spring.Key].transform);

                }
                else
                {
                    CoreParam.springsG[spring.Key].transform.position = Tool.GridToUxy(spring.Value.X, spring.Value.Y);
                    // CoreParam.springsG[spring.Key].transform.rotation =
                    //     Quaternion.AngleAxis((float)spring.Value.FacingDirection * Mathf.Rad2Deg + 180, Vector3.forward);
                }
            }
        }
        for (int i = 0; i < CoreParam.springsG.Count; i++)
        {
            KeyValuePair<Tuple<int, int>, GameObject> springG = CoreParam.springsG.ElementAt(i);
            if (springG.Value != null)
            {
                if (!CoreParam.springs.ContainsKey(springG.Key))
                {
                    Destroy(springG.Value);
                    CoreParam.springsG.Remove(springG.Key);
                }
            }
        }
    }*/
    void ShowAllMessage(MessageToClient messageToClient)
    {
        gameTime.text = "GameTime:" + messageToClient.AllMessage.GameTime;
        score.text = "Score(Buddhists:Monsters):" + messageToClient.AllMessage.BuddhistsTeamScore + ":" + messageToClient.AllMessage.MonstersTeamScore;
        economy.text = "Economy(Buddhists:Monsters):" + messageToClient.AllMessage.BuddhistsTeamEconomy + ":" + messageToClient.AllMessage.MonstersTeamEconomy;
    }
}
