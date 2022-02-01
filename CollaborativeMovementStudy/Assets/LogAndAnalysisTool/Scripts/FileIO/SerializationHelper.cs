using System;
using System.Linq;
using System.Reflection;

public static class SerializationHelper
{
    public const string CsvDelimiter = ";";

    public static string FieldNamesToCsvString<T>(T a_serializableObject)
    {
        FieldInfo[] fields = a_serializableObject.GetType().GetFields();

        var csvString = "";
        var propertyNames = fields.Select((field) => field.Name);
        csvString = string.Join(CsvDelimiter, propertyNames);

        return csvString;
    }
    /// <summary>
    /// Does not escape the csv delimiter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="a_serializableObject"></param>
    /// <returns></returns>
    public static string FieldValuesToCsvString<T>(T a_serializableObject)
    {
        FieldInfo[] fields = a_serializableObject.GetType().GetFields();

        var csvString = "";
        var propertyValues = fields.Select((field) => field.GetValue(a_serializableObject)?.ToString());
        csvString = string.Join(CsvDelimiter, propertyValues);

        return csvString;
    }

    public static string FieldsToString<T>(T a_serializableObject)
    {
        FieldInfo[] fields = a_serializableObject.GetType().GetFields();

        var csvString = "";
        var propertyNames = fields.Select((field) => $"{field.Name}: {field.GetValue(a_serializableObject)?.ToString()}");
        csvString = string.Join(Environment.NewLine, propertyNames);

        return csvString;
    }
}
