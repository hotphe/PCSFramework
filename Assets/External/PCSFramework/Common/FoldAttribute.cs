using System;
namespace PCS.Common
{
    public class FoldAttribute : Attribute
    {
        public string name;
        public FoldAttribute(string name)
        {
            this.name = name;
        }
    }
}