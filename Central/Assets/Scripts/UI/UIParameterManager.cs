using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

public class IgnoreParameterAttribute : System.Attribute
{
}

public class UIParameterManager : SingletonMonoBehaviour<UIParameterManager>
{
    [Header("Parameter Prefabs")]
    public List<UIParameter> parameterPrefabs;

    public delegate void ObjectParameterChanged(EditObject parentObject, UIParameter parameter, object newValue);

    public class ObjectParameterList
    {
        public object reflectedObject;
        public ObjectParameterChanged onParameterChanged;
        public List<UIParameter> parameters = new List<UIParameter>();
    }

    public ObjectParameterList CreateControls(EditObject objectToReflect, RectTransform root)
    {
        var reflectedObj = new ObjectParameterList();
        reflectedObj.reflectedObject = objectToReflect;

        // List all public fields
        var objType = objectToReflect.GetType();

        var props = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            // Find a parameter ui for this property type
            var prefab = parameterPrefabs.FirstOrDefault(pp => pp.CanEdit(prop.PropertyType, prop.GetCustomAttributes(false)));
            if (prefab != null)
            {
                // Create the UI
                var uiparam = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, root);
                uiparam.Setup(prop, objectToReflect);
                uiparam.onParameterModified += (ui, val) =>
                {
                    reflectedObj.onParameterChanged?.Invoke(objectToReflect, ui, val);
                };
                reflectedObj.parameters.Add(uiparam);
            }
        }

        var fields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            // Find a parameter ui for this field's type
            var att = field.GetCustomAttributes(false);
            if (!att.Any(a => a.GetType() == typeof(IgnoreParameterAttribute)))
            {
                var prefab = parameterPrefabs.FirstOrDefault(pp => pp.CanEdit(field.FieldType, att));
                if (prefab != null)
                {
                    // Create the UI
                    var uiparam = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, root);
                    uiparam.Setup(field, objectToReflect);
                    uiparam.onParameterModified += (ui, val) =>
                    {
                        reflectedObj.onParameterChanged?.Invoke(objectToReflect, ui, val);
                    };
                    reflectedObj.parameters.Add(uiparam);
                }
            }
        }
        return reflectedObj;
    }
}
