using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleGameDieUI : MonoBehaviour
{
    public Die die { get; set; }

    [SerializeField]
    Text _NameField = null;

    [SerializeField]
    Image _VisibleFace = null;

    [SerializeField]
    Image _Background = null;

    [SerializeField]
    Text _AnimationName = null;

    [SerializeField]
    Sprite[] _AllFaces = null;

    [SerializeField]
    Sprite _NotSettled = null;
    [SerializeField]
    Color[] _TeamColors = null;

    int _Team;

    public int Team
    {
        get => _Team;
        set => _SetTeam(value);
    }

    public string AnimationName
    {
        get => _AnimationName.text;
        set => _AnimationName.text = value;
    }

    public void Setup(Die die)
    {
        this.die = die;
    }

    void _SetTeam(int team)
    {
        _Team = Mathf.Max(0, team);
        _Background.color = _Team == 0 ? Color.white : _TeamColors[Mathf.Min(team, _TeamColors.Length) - 1];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _NameField.text = $"{die?.name}\n{die.state}";
        _VisibleFace.sprite = die?.state == Die.State.Idle ? _AllFaces[die.face] : _NotSettled;
    }
}
