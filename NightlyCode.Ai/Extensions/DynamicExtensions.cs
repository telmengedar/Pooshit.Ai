using System.ComponentModel;
using NightlyCode.Ai.Extern;

namespace NightlyCode.Ai.Extensions;

public static class DynamicExtensions {
    
    public static Dictionary<string, T> ToDictionary<T>(dynamic dynObj)
    {
        Dictionary<string, T> dictionary = new();
        foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynObj))
        {
            object obj = propertyDescriptor.GetValue(dynObj);
            dictionary.Add(propertyDescriptor.Name, Converter.Convert<T>(obj));
        }
        return dictionary;
    }
}