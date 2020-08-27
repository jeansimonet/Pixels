using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDiceRendererRootList : MonoBehaviour
{
    public Camera captureCam;
    public GameObject rotationRoot;
    public GameObject[] roots;

    public bool rotating { get; set; }
    float rotationSpeedDeg;

    // Start is called before the first frame update
    void Start()
    {
        rotationRoot.transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f), Space.Self);
        rotationSpeedDeg = AppConstants.Instance.MultiDiceRootRotationSpeedAvg + Random.Range(-AppConstants.Instance.MultiDiceRootRotationSpeedVar, AppConstants.Instance.MultiDiceRootRotationSpeedVar);
    }


    // Update is called once per frame
    void Update()
    {
        if (rotating)
        {
            rotationRoot.transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeedDeg, Space.Self);
        }
    }
}
