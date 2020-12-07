using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Animations;
using Behaviors;

public class DiceRendererDice : MonoBehaviour
{
    public MeshRenderer[] FaceRenderers;
    public Light[] FaceLights;
    public Color[] FaceColors;
    public float deceleration = 180.0f; // deg/s/s
    MaterialPropertyBlock[] propertyBlocks;

    public float delay { get; set; } = 1.0f;
    float rotationSpeedDeg;

    float _currentAngle = 0.0f;
    public float currentAngle
    {
        get { return _currentAngle; }
    }

    float _currentSpeed = 0.0f;

    DataSet dataSet;
    List<EditAnimation> animations = new List<EditAnimation>();
    AnimationInstance currentInstance;
    int currentAnimationIndex;

    enum State
    {
        Idle,
        Playing,
        Waiting,
    }
    State currentState = State.Idle;
    float timeLeft;

    public enum RotationState
    {
        Auto,
        Drag,
        Idle
    }
    public RotationState rotationState { get; private set; } = RotationState.Idle;

    public delegate void RotationStateEvent(RotationState newState);
    public RotationStateEvent onRotationStateChange;

    public void SetAnimations(IEnumerable<EditAnimation> animations)
    {
        this.animations.Clear();
        this.animations.AddRange(animations);
        if (this.animations.Count > 0)
        {
            if (currentAnimationIndex >= this.animations.Count)
            {
                currentAnimationIndex = 0;
            }

            if (currentInstance != null)
            {
                // We're switching the animation from underneath the playback
                SetupInstance(currentAnimationIndex, currentInstance.startTime, currentInstance.remapFace);
            }
        }
        else
        {
            Stop();
        }
    }

    public void SetAnimation(Animations.EditAnimation editAnimation)
    {
        if (editAnimation != null)
        {
            animations.Clear();
            animations.Add(editAnimation);
            currentAnimationIndex = 0;
            if (currentInstance != null)
            {
                // We're switching the animation from underneath the playback
                SetupInstance(currentAnimationIndex, currentInstance.startTime, currentInstance.remapFace);
            }
        }
        else
        {
            ClearAnimations();
        }
    }

    public void ClearAnimations()
    {
        // Shouldn't have an instance if we don't have an animation
        currentInstance = null;
        dataSet = null;
        currentState = State.Idle;

        // Clear the animation
        animations.Clear();
    }

    public void Play(bool loop)
    {
        currentState = State.Waiting;
        timeLeft = 0.0f;
    }

    public void Stop()
    {
        currentInstance = null;
        dataSet = null;
    }

    void Awake()
    {
        propertyBlocks = new MaterialPropertyBlock[20];
        for (int i = 0; i < FaceColors.Length; ++i)
        {
            FaceColors[i] = Color.black;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_GlowColor", FaceColors[i]);

            propertyBlocks[i] = block;
            FaceRenderers[i].SetPropertyBlock(block);
            FaceLights[i].color = FaceColors[i];
        }
        currentState = State.Idle;
        timeLeft = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        _currentAngle = Random.Range(0.0f, 360.0f);
        transform.Rotate(Vector3.up, currentAngle, Space.Self);
        rotationSpeedDeg = AppConstants.Instance.DiceRotationSpeedAvg + Random.Range(-AppConstants.Instance.DiceRotationSpeedVar, AppConstants.Instance.DiceRotationSpeedVar);
    }

    public void SetCurrentAngle(float newAngle)
    {
        _currentSpeed = (newAngle - _currentAngle) / Time.deltaTime;
        _currentAngle = newAngle;
        rotationState = RotationState.Drag;
        onRotationStateChange?.Invoke(rotationState);
    }

    public void SetAuto(bool auto)
    {
        if (auto)
        {
            rotationState = RotationState.Auto;
        }
        else
        {
            rotationState = RotationState.Idle;
        }
        onRotationStateChange?.Invoke(rotationState);
    }

    // Update is called once per frame
    void Update()
    {
        if (FaceColors.Length == 20)
        {
            for (int i = 0; i < 20; ++i)
            {
                FaceColors[i] = Color.black;
            }
            switch (currentState)
            {
                case State.Waiting:
                    {
                        if (animations.Count > 0)
                        {
                            timeLeft -= Time.deltaTime;
                            if (timeLeft <= 0.0f)
                            {
                                currentState = State.Playing;
                                currentAnimationIndex++;
                                if (currentAnimationIndex >= animations.Count)
                                {
                                    currentAnimationIndex = 0;
                                }
                                if (animations[currentAnimationIndex] != null)
                                {
                                    SetupInstance(currentAnimationIndex, (int)(Time.time * 1000), 0xFF);
                                }
                                else
                                {
                                    currentState = State.Waiting;
                                }
                            }
                        }
                        // Else don't switch to playing state
                    }
                    break;
                case State.Playing:
                    {
                        Debug.Assert(currentInstance != null);
                        // Update the animation time
                        int time = (int)(Time.time * 1000);
                        if (time > (currentInstance.startTime + currentInstance.animationPreset.duration))
                        {
                            for (int i = 0; i < 20; ++i)
                            {
                                FaceColors[i] = Color.black;
                            }
                            currentInstance = null;
                            currentState = State.Waiting;
                            timeLeft = delay;
                        }
                        else
                        {
                            int [] retIndices = new int[20];
                            uint[] retColors = new uint[20];
                            int ledCount = currentInstance.updateLEDs(time, retIndices, retColors);
                            for (int t = 0; t < ledCount; ++t)
                            {
                                uint color = retColors[t];
                                Color32 color32 = new Color32(
                                    ColorUtils.getRed(color),
                                    ColorUtils.getGreen(color),
                                    ColorUtils.getBlue(color),
                                    255);
                                FaceColors[retIndices[t]] = color32;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            UpdateColors();
            switch (rotationState)
            {
                case RotationState.Auto:
                    {
                        _currentAngle += Time.deltaTime * rotationSpeedDeg;
                    }
                    break;
                case RotationState.Drag:
                    {
                        // Angle is set externally;
                    }
                    break;
                case RotationState.Idle:
                    {
                        if (_currentSpeed > 0.0f)
                        {
                            _currentSpeed -= Time.deltaTime * deceleration;
                            if (_currentSpeed < 0.0f)
                            {
                                _currentSpeed = 0.0f;
                            }
                        }
                        else if (_currentSpeed < 0.0f)
                        {
                            _currentSpeed += Time.deltaTime * deceleration;
                            if (_currentSpeed > 0.0f)
                            {
                                _currentSpeed = 0.0f;
                            }
                        }
                        _currentAngle += Time.deltaTime * _currentSpeed;
                    }
                    break;
            }
        }

        var quat = Quaternion.AngleAxis(_currentAngle, Vector3.up);
        transform.localRotation = quat;
    }

    void UpdateColors()
    {
        if (FaceColors != null)
        {
            if (propertyBlocks == null)
            {
                propertyBlocks = new MaterialPropertyBlock[20];
                for (int i = 0; i < FaceColors.Length; ++i)
                {
                    propertyBlocks[i] = new MaterialPropertyBlock();
                }
            } 

            for (int i = 0; i < FaceColors.Length; ++i)
            {
                var reverseGammaCorrectedColor = ColorUtils.reverseGamma(FaceColors[i]);
                var block = propertyBlocks[i];
                block.SetColor("_GlowColor", reverseGammaCorrectedColor);
                FaceRenderers[i].SetPropertyBlock(block);
                FaceLights[i].color = reverseGammaCorrectedColor;
            }
        }
    }

    void OnValidate()
    {
        UpdateColors();
    }

    void SetupInstance(int animationIndex, int startTime, byte remapFace)
    {
        currentAnimationIndex = animationIndex;
        EditDataSet tempEditSet = AppDataSet.Instance.ExtractEditSetForAnimation(animations[animationIndex]);
        dataSet = tempEditSet.ToDataSet();
        currentInstance = dataSet.animations[0].CreateInstance(dataSet.animationBits);
        currentInstance.start(startTime, remapFace, false);
    }

}
