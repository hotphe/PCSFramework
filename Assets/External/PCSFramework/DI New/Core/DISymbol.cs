using UnityEditor;
using PCS.Common;

[InitializeOnLoad]
public static class DISymbol
{
    static DISymbol()
    {
        DefineSymbolAdder.SetDefineSymbol("PCS_DI");
    }
}
