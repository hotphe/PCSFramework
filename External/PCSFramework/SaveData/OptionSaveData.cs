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
        public SystemLanguage Language { get; set; } = SystemLanguage.English;
        public bool isFirstRun { get; set; } = true;



        private static OptionSaveData _instance;
        public static OptionSaveData Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Load();
                return _instance;
                
            }
        }

        /// <summary>
        /// 세이브 데이터를 기존으로 되돌림.
        /// </summary>
        /// <returns></returns>
        public static OptionSaveData Revert()
        {
            _instance = null;
            return Instance;
        }
    }
}
