using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public abstract class UIParameter : MonoBehaviour
{
    public abstract System.Type parameterType { get; }

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

        SetupControls(
            r.Replace(fieldInfo.Name.Substring(0, 1).ToUpper() + fieldInfo.Name.Substring(1), " "),
            () => fieldInfo.GetValue(parentObject),
            (val) =>
            {
                fieldInfo.SetValue(parentObject, val);
                onParameterModified?.Invoke(this, val);
            },
            fieldInfo.GetCustomAttributes(false));
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
