using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die3D : MonoBehaviour
{
    public Camera dieCamera;
    public Light dieLight;
    public GameObject dieRoot;
    public GameObject dieObject;
    public MeshRenderer dieMeshRenderer;
    public int RenderTextureSize = 512;
    public float MaxRotationSpeed = 1080.0f; // d/s

    RenderTexture renderTexture;
    Quaternion currentDiceRot;

    public Color pipsColor
    {
        get { return _pipsColor; }
        set
        {
            ChangePipColor(value);
        }
    }
    Color _pipsColor;

    // Use this for initialization
    void Start () {
		
	}

    public RenderTexture Setup(int index)
    {
        int layerIndex = LayerMask.NameToLayer("Dice " + index);
        renderTexture = new RenderTexture(RenderTextureSize, RenderTextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();

        dieCamera.cullingMask = 1 << layerIndex; // only render particle effects
        dieCamera.targetTexture = renderTexture;

        dieLight.cullingMask = 1 << layerIndex;

        dieRoot.layer = layerIndex;
        dieObject.layer = layerIndex;

        return renderTexture;
    }

    public void UpdateAcceleration(Vector3 acc)
    {
        // The accelerometer is right-handed in the following way:
        // z is up
        // x is forward
        // y is left
        // We want unity orientation, which is:
        // y is up
        // z is forward
        // x is right
        Vector3 unityAcc = new Vector3(-acc.y, acc.z, acc.x);

        // acc is gravity (i.e. down)
        // we want the on screen die to have its up vector match exactly the measured up
        // and we don't really know what its forward vec should be, so we just pick
        // the closest to the current forward vector
        Vector3 measuredUp = (-unityAcc).normalized;
        Vector3 right = Vector3.Cross(measuredUp, dieRoot.transform.forward);
        Vector3 newForward = Vector3.Cross(right, measuredUp);
        currentDiceRot = Quaternion.LookRotation(newForward, measuredUp);
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            renderTexture = null;
        }
    }


    // Update is called once per frame
    void Update ()
    {
        float maxDelta = Time.deltaTime * MaxRotationSpeed;
        dieRoot.transform.localRotation = Quaternion.RotateTowards(dieRoot.transform.localRotation, currentDiceRot, maxDelta);
	}

    void ChangePipColor(Color newColor)
    {
        dieMeshRenderer.materials[1].color = newColor;
        _pipsColor = newColor;
    }
}
