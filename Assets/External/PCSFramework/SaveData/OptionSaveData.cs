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
        public QualityType ResolutionQuality { get; set; } = QualityType.Normal;
        public bool IsFullScreen { get; set; } = true;
        public float MasterVolume { get; set; } = 1f;
        public float BGMVolume { get; set; } = 1f;
        public float SFXVolume { get; set; } = 1f;
        public SystemLanguage Language { get; set; } = SystemLanguage.Korean;
        public bool isFirstRun { get; set; } = true;
    }
}
