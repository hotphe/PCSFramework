using UnityEditor;
using UnityEngine;

public class DeletePlayerPrefs : MonoBehaviour
{
    [MenuItem("PCS/SaveData/DeleteAll")]
    static void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
