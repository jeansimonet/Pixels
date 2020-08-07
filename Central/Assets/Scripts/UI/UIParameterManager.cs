using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

public class UIParameterManager : SingletonMonoBehaviour<UIParameterManager>
{
    [Header("Parameter Prefabs")]
    public List<UIParameter> parameterPrefabs;

    public delegate void ObjectParameterChanged(object parentObject, UIParameter parameter, object newValue);

    public interface IObjectParameterList
    {
        object reflectedObject { get; }
        RectTransform root { get; }
        ObjectParameterChanged onParameterChanged { get; set; }
    }

    [System.Serializable]
    class ObjectParameterList
        : IObjectParameterList
    {
        public object reflectedObject { get; set; }
        public RectTransform root { get; set; }
        public ObjectParameterChanged onParameterChanged { get; set; }
        public List<UIParameter> parameters = new List<UIParameter>();
    }

    List<ObjectParameterList> trackedObjects = new List<ObjectParameterList>();

    void Awake()
    {
    }

    public IObjectParameterList CreateControls(object objectToReflect, RectTransform root)
    {
        var reflectedObj = new ObjectParameterList();
        reflectedObj.reflectedObject = objectToReflect;
        reflectedObj.root = root;

        // List all public fields
        var objType = objectToReflect.GetType();
        var fields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            // Find a parameter ui for this field's type
            var prefab = parameterPrefabs.FirstOrDefault(pp => field.FieldType == pp.parameterType || field.FieldType.IsSubclassOf(pp.parameterType));
            if (prefab != null)
            {
                // Create the UI
                var uiparam = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, root);
                uiparam.Setup(field, objectToReflect);
                uiparam.onParameterModified += (ui, val) => reflectedObj.onParameterChanged?.Invoke(objectToReflect, ui, val);
                reflectedObj.parameters.Add(uiparam);
            }
        }
        trackedObjects.Add(reflectedObj);
        return reflectedObj;
    }

    public void DestroyControls(object objectToReflect)
    {
        int reflectedObjIndex = trackedObjects.FindIndex(obj => obj.reflectedObject == objectToReflect);
        if (reflectedObjIndex != -1)
        {
            var reflectedObj = trackedObjects[reflectedObjIndex];
            foreach (var uiparam in reflectedObj.parameters)
            {
                GameObject.Destroy(uiparam.gameObject);
            }
            trackedObjects.RemoveAt(reflectedObjIndex);
        }
    }
}
