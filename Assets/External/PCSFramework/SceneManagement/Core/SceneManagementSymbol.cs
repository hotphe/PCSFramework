using UnityEditor;
using PCS.Common;

[InitializeOnLoad]
public static class SceneManagementSymbol
{
    static SceneManagementSymbol()
    {
        DefineSymbolAdder.SetDefineSymbol("PCS_SceneManagement");
    }
}
