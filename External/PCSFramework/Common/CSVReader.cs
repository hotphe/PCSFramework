using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PCS.Network;
using Cysharp.Threading.Tasks;

namespace PCS.Common
{
    public class CSVReader
    {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static char[] TRIM_CHARS = { '\"' };

        public static Dictionary<string, Dictionary<string, string>> ReadAll(TextAsset data)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);

            if (lines.Length <= 1) 
                return dict;

            //0���� �ּ�, 1���� header������ ����մϴ�.
            var header = Regex.Split(lines[1], SPLIT_RE);

            for (int i = 2; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || string.IsNullOrWhiteSpace(values[0]) 
                    || values[0] == "x") //ù ���� x �� ��� �ش� ���� �����մϴ�.
                    continue;

                var entry = new Dictionary<string, string>();

                //0���� index
                for (int j = 1; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    entry[header[j]] = value;
                }
                //values[0] == index
                dict[values[0]] = entry;
            }
            return dict;
        }
    }
}