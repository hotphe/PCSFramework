using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;

namespace PCS.Common
{
    public class CSVReader
    {
        static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
        static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
        static char[] TRIM_CHARS = { '\"' };

        public static Dictionary<string, Dictionary<string, string>> ReadAll(TextAsset data, int headerRow = 0)
        {
            var dict = new Dictionary<string, Dictionary<string, string>>();

            var lines = Regex.Split(data.text, LINE_SPLIT_RE);

            if (lines.Length <= 1)
                return dict;

            var header = Regex.Split(lines[headerRow], SPLIT_RE);

            for (int i = headerRow+1; i < lines.Length; i++)
            {
                var values = Regex.Split(lines[i], SPLIT_RE);
                if (values.Length == 0 || string.IsNullOrWhiteSpace(values[0])
                    || values[0] == "x") //첫 셀의 값이 x 일 경우 해당 줄은 무시합니다.
                    continue;

                var entry = new Dictionary<string, string>();

                //0번째 column은 Key
                for (int j = 1; j < header.Length && j < values.Length; j++)
                {
                    string value = values[j];
                    value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                    entry[header[j]] = value;
                }
                dict[values[0]] = entry;
            }
            return dict;
        }
    }
}
