using System;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class EditorChangeableAttribute : Attribute
{
    public string DisplayName { get; set; }
    
    public EditorChangeableAttribute(string displayName = null)
    {
        DisplayName = displayName;
    }
}