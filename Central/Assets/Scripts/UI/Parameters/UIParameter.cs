using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;


public class NameAttribute
    : System.Attribute
{
    public string name;
    public NameAttribute(string name)
    {
        this.name = name;
    }
}

public abstract class UIParameter : MonoBehaviour
{
    public abstract bool CanEdit(System.Type parameterType, IEnumerable<object> attributes);

    public delegate void ParameterModifiedEvent(UIParameter ui, object newValue);
    public ParameterModifiedEvent onParameterModified;

    protected abstract void SetupControls(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null);

    public void Setup(string name, System.Func<object> getterFunc, System.Action<object> setterAction, IEnumerable<object> attributes = null)
    {
        SetupControls(
            name,
            getterFunc,
            (val) =>
            {
                setterAction(val);
                onParameterModified?.Invoke(this, val);
            },
            attributes);
    }

    public void Setup(System.Reflection.FieldInfo fieldInfo, object parentObject)
    {
        var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                (?<=[^A-Z])(?=[A-Z]) |
                (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        string fieldName = r.Replace(fieldInfo.Name.Substring(0, 1).ToUpper() + fieldInfo.Name.Substring(1), " ");
        var nameAttribute = fieldInfo.GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault() as NameAttribute;
        if (nameAttribute != null)
        {
            fieldName = nameAttribute.name;
        }

        SetupControls(
            fieldName,
            () => fieldInfo.GetValue(parentObject),
            (val) =>
            {
                fieldInfo.SetValue(parentObject, val);
                onParameterModified?.Invoke(this, val);
            },
            fieldInfo.GetCustomAttributes(false));
    }

    public void Setup(System.Reflection.PropertyInfo propertyInfo, object parentObject)
    {
        var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                (?<=[^A-Z])(?=[A-Z]) |
                (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        string propName = r.Replace(propertyInfo.Name.Substring(0, 1).ToUpper() + propertyInfo.Name.Substring(1), " ");
        var nameAttribute = propertyInfo.GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault() as NameAttribute;
        if (nameAttribute != null)
        {
            propName = nameAttribute.name;
        }

        SetupControls(
            propName,
            () => propertyInfo.GetValue(parentObject),
            (val) =>
            {
                propertyInfo.SetValue(parentObject, val);
                onParameterModified?.Invoke(this, val);
            },
            propertyInfo.GetCustomAttributes(false));
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
