using UnityEngine;
using System;
public class FoldAttribute : Attribute
{
    public string name;
    public FoldAttribute(string name)
    {
        this.name = name;
    }
}