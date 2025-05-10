using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractControl : Singleton<InteractControl>
{
    public enum InteractType
    {
        NoneType,
        Buddhists,
        Monsters,
        Character,
        CharacterSP
    }

    public enum InteractOption
    {
        None,
        SummonBuddhistsCharacter1,
        SummonBuddhistsCharacter2,
        SummonBuddhistsCharacter3,
        SummonBuddhistsCharacter4,
        SummonBuddhistsCharacterSP,
        SummonMonstersCharacter1,
        SummonMonstersCharacter2,
        SummonMonstersCharacter3,
        SummonMonstersCharacter4,
        SummonMonstersCharacterSP,
        PurchaseSmallHealthPotion,
        PurchaseMediumHealthPotion,
        PurchaseLargeHealthPotion,
        PurchaseSmallShield,
        PurchaseMediumShield,
        PurchaseLargeShield,
        PurchaseSpeedboots,
        PurchasePurificationPotion,
        PurchaseInvisibilityPotion,
        PurchaseBerserkPotion,
        Produce,
        ConstructBarracks,
        ConstructFarm,
        ConstructTrap,
        ConstructCage
    }
    public readonly Dictionary<InteractType, List<InteractOption>> interactOptions = new Dictionary<InteractType, List<InteractOption>>(){
        {InteractType.NoneType,
            null},
        {InteractType.Buddhists,
            new List<InteractOption>{
                InteractOption.SummonBuddhistsCharacter1,
                InteractOption.SummonBuddhistsCharacter2,
                InteractOption.SummonBuddhistsCharacter3,
                InteractOption.SummonBuddhistsCharacter4,
                InteractOption.SummonBuddhistsCharacterSP,
                }},
        {InteractType.Monsters,
            new List<InteractOption>{
                InteractOption.SummonMonstersCharacter1,
                InteractOption.SummonMonstersCharacter2,
                InteractOption.SummonMonstersCharacter3,
                InteractOption.SummonMonstersCharacter4,
                InteractOption.SummonMonstersCharacterSP,
                }},
        {InteractType.Character,
            new List<InteractOption>{
                InteractOption.PurchaseSmallHealthPotion,
                InteractOption.PurchaseMediumHealthPotion,
                InteractOption.PurchaseLargeHealthPotion,
                InteractOption.PurchaseSmallShield,
                InteractOption.PurchaseMediumShield,
                InteractOption.PurchaseLargeShield,
                InteractOption.PurchaseSpeedboots,
                InteractOption.PurchasePurificationPotion,
                InteractOption.PurchaseInvisibilityPotion,
                InteractOption.PurchaseBerserkPotion,
                InteractOption.Produce,
                }},
        {InteractType.CharacterSP,
            new List<InteractOption>{
                InteractOption.ConstructBarracks,
                InteractOption.ConstructFarm,
                InteractOption.ConstructTrap,
                InteractOption.ConstructCage
                }},
    };
    public readonly Dictionary<InteractOption, string> textDic = new Dictionary<InteractOption, string>()
    {
        {InteractOption.None, ""},
        {InteractOption.SummonBuddhistsCharacter1, "召唤孙悟空"},
        {InteractOption.SummonBuddhistsCharacter2, "召唤猪八戒"},
        {InteractOption.SummonBuddhistsCharacter3, "召唤沙悟净"},
        {InteractOption.SummonBuddhistsCharacter4, "召唤白龙马"},
        {InteractOption.SummonBuddhistsCharacterSP, "召唤猴子猴孙"},
        {InteractOption.SummonMonstersCharacter1, "召唤红孩儿"},
        {InteractOption.SummonMonstersCharacter2, "召唤牛魔王"},
        {InteractOption.SummonMonstersCharacter3, "召唤铁扇公主"},
        {InteractOption.SummonMonstersCharacter4, "召唤蜘蛛精"},
        {InteractOption.SummonMonstersCharacterSP, "召唤无名小妖"},
        {InteractOption.PurchaseSmallHealthPotion, "购买小型血瓶"},
        {InteractOption.PurchaseMediumHealthPotion, "购买中型血瓶"},
        {InteractOption.PurchaseLargeHealthPotion, "购买大型血瓶"},
        {InteractOption.PurchaseSmallShield, "购买小型护盾"},
        {InteractOption.PurchaseMediumShield, "购买中型护盾"},
        {InteractOption.PurchaseLargeShield, "购买大型护盾"},
        {InteractOption.PurchaseSpeedboots, "购买鞋子"},
        {InteractOption.PurchasePurificationPotion, "购买净化药水"},
        {InteractOption.PurchaseInvisibilityPotion, "购买隐身药水"},
        {InteractOption.PurchaseBerserkPotion, "购买狂暴药水"},
        {InteractOption.Produce, "采集资源"},
        {InteractOption.ConstructBarracks, "建造兵营"},
        {InteractOption.ConstructFarm, "建造农场"},
        {InteractOption.ConstructTrap, "建造坑洞陷阱"},
        {InteractOption.ConstructCage, "建造牢笼陷阱"}
    };
    public readonly Dictionary<InteractOption, string> textCost = new Dictionary<InteractOption, string>()
    {
        {InteractOption.None, ""},
        {InteractOption.SummonBuddhistsCharacter1, "＄5000"},
        {InteractOption.SummonBuddhistsCharacter2, "＄4000"},
        {InteractOption.SummonBuddhistsCharacter3, "＄3000"},
        {InteractOption.SummonBuddhistsCharacter4, "＄4000"},
        {InteractOption.SummonBuddhistsCharacterSP, "＄1000"},
        {InteractOption.SummonMonstersCharacter1, "＄5000"},
        {InteractOption.SummonMonstersCharacter2, "＄4000"},
        {InteractOption.SummonMonstersCharacter3, "＄3000"},
        {InteractOption.SummonMonstersCharacter4, "＄3000"},
        {InteractOption.SummonMonstersCharacterSP, "＄1000"},
        {InteractOption.PurchaseSmallHealthPotion, "＄1500"},
        {InteractOption.PurchaseMediumHealthPotion, "＄3000"},
        {InteractOption.PurchaseLargeHealthPotion, "＄4500"},
        {InteractOption.PurchaseSmallShield, "＄2000"},
        {InteractOption.PurchaseMediumShield, "＄3500"},
        {InteractOption.PurchaseLargeShield, "＄5000"},
        {InteractOption.PurchaseSpeedboots, "＄1500"},
        {InteractOption.PurchasePurificationPotion, "＄2000"},
        {InteractOption.PurchaseInvisibilityPotion, "＄4000"},
        {InteractOption.PurchaseBerserkPotion, "＄10000"},
        {InteractOption.Produce, ""},
        {InteractOption.ConstructBarracks, "＄10000"},
        {InteractOption.ConstructFarm, "＄8000"},
        {InteractOption.ConstructTrap, "＄1000"},
        {InteractOption.ConstructCage, "＄1000"}
    };
}
