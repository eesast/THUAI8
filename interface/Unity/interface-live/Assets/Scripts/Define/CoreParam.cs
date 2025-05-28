using System;
using System.Collections;
using System.Collections.Generic;
using Protobuf;
using UnityEngine;

public class CoreParam
{
    public class FrameQueue<T>
    {
        public FrameQueue()
        {
            valQueue = new Queue<T>();
        }
        private Queue<T> valQueue;
        public void Add(T val)
        {
            valQueue.Enqueue(val);
        }
        public T GetValue()
        {
            if (valQueue.Count == 0)
                return default;
            return valQueue.Dequeue();
        }
        public int GetSize()
        {
            return valQueue.Count;
        }
        public void Clear()
        {
            valQueue.Clear();
        }
    };
    public static FrameQueue<MessageToClient> frameQueue = new FrameQueue<MessageToClient>();
    public static MessageToClient firstFrame, currentFrame;
    public static MessageOfMap map;
    public static MessageOfTeam[] teams = new MessageOfTeam[2];
    public static Dictionary<long, MessageOfCharacter> characters = new();
    public static Dictionary<long, GameObject> charactersG = new();
    public static Dictionary<Tuple<int, int>, MessageOfBarracks> barracks = new();
    public static Dictionary<Tuple<int, int>, GameObject> barracksG = new();
    public static Dictionary<Tuple<int, int>, MessageOfSpring> springs = new();
    public static Dictionary<Tuple<int, int>, GameObject> springsG = new();
    public static Dictionary<Tuple<int, int>, MessageOfFarm> farms = new();
    public static Dictionary<Tuple<int, int>, GameObject> farmsG = new();
    public static Dictionary<Tuple<int, int>, MessageOfTrap> traps = new();
    public static Dictionary<Tuple<int, int>, GameObject> trapsG = new();
    public static Dictionary<Tuple<int, int>, MessageOfEconomyResource> economyResources = new();
    public static Dictionary<Tuple<int, int>, GameObject> economyResourcesG = new();
    public static Dictionary<Tuple<int, int>, MessageOfAdditionResource> additionResources = new();
    public static Dictionary<Tuple<int, int>, GameObject> additionResourcesG = new();
    public static bool initialized;
    public static int cnt = 0;
    public static void Reset()
    {
        frameQueue.Clear();
        map = null;
        firstFrame = currentFrame = null;
        teams[0] = teams[1] = null;
        characters.Clear();
        charactersG.Clear();
        barracks.Clear();
        barracksG.Clear();
        springs.Clear();
        springsG.Clear();
        farms.Clear();
        farmsG.Clear();
        traps.Clear();
        trapsG.Clear();
        economyResources.Clear();
        economyResourcesG.Clear();
        additionResources.Clear();
        additionResourcesG.Clear();
        cnt = 0;
        initialized = false;
    }
}
