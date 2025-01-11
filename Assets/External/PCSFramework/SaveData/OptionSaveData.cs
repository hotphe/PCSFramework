using PCS.UI;
using System;
using UnityEngine;

namespace PCS.SaveData
{
    public enum Country
    {
        English,
        Korean
    }

    public class OptionSaveData : SaveData<OptionSaveData>
    {
        public QualityType ResolutionQuality  = QualityType.Normal;
        public Vector2Int Resolution = Vector2Int.zero;
        public bool IsFullScreen = true;
        public float MasterVolume = 1f;
        public float BGMVolume = 1f;
        public float SFXVolume = 1f;
        public SystemLanguage Language = SystemLanguage.Korean;
        public bool isFirstRun = true;
        public int test;
        public int test2 { get; set; }
    }
}
