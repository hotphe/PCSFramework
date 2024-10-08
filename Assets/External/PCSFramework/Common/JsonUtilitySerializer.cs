using System.Text;
using UnityEngine;

namespace PCS.Common
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] data);
    }

    public class JsonUtilitySerializer : ISerializer
    {
        public byte[] Serialize<T>(T data)
        {
            var json = JsonUtility.ToJson(data);
            return Encoding.UTF8.GetBytes(json);
        }
        public T Deserialize<T>(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return JsonUtility.FromJson<T>(json);
        }

    }
}
