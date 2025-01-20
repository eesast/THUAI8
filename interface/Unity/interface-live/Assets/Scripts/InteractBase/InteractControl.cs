using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractControl : Singleton<InteractControl>
{
    public enum InteractType
    {
        NoneType,
        Character,
        Place,
    }
    public enum InteractOption
    {
        None,
        Test,
    }
    /*public readonly Dictionary<InteractType, List<InteractOption>> interactOptions = new Dictionary<InteractType, List<InteractOption>>(){
        {InteractType.NoneType,
            null},
        {InteractType.Base,
            new List<InteractOption>{
                InteractOption.Test
                }},
        {InteractType.Ship,
            new List<InteractOption>{
                InteractOption.Test2,
                }},
    };
    public readonly Dictionary<InteractOption, string> textDic = new Dictionary<InteractOption, string>()
    {
        {InteractOption.None, ""},
        {InteractOption.Test, "test"},
    };
    public readonly Dictionary<InteractOption, string> textCost = new Dictionary<InteractOption, string>()
    {
        {InteractOption.None, ""},
        {InteractOption.Test, "＄400"},
    };*/
}
