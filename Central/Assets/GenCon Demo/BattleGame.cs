using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class BattleGame : MonoBehaviour
{
    [SerializeField]
    BattleGameUI _UI = null;

    [SerializeField]
    CurrentDicePool _CurrentDicePool = null;

    [SerializeField]
    ToggleGroup _diceToggleGroup = null;

    [SerializeField]
    Button _ManualConnectBtn = null;

    [SerializeField]
    Toggle _PlayFaceSounds = null;

    [SerializeField]
    bool _AutoConnect;

    [SerializeField]
    Sounds _Sounds = new Sounds();

    [System.Serializable]
    class Sounds
    {
        public AudioClip[] NewPlayer = null;
        public AudioClip[] DuelStarted = null;
        public AudioClip[] GameCompleted = null;
        public AudioClip[] GameCancelled = null;
        public AudioClip[] FaceValues = null;
    }

    enum Animations
    {
        None, ShowTeam, FaceUp, WaitingForBattle, Duel, DuelWin, DuelLoose, DuelDraw, TeamWin, TeamLoose, TeamDraw
    }

    class BattleDie : System.IDisposable
    {
        BattleGameDieUI _dieUI;
        Animations _anim;
        public delegate void LandedOnFaceHandler(BattleDie die, int value);
        public event LandedOnFaceHandler LandedOnFace;
        public Die Die { get; }
        public int Value => Die.state != Die.State.Idle ? 0 : Die.face + 1;
        public bool IsRolling { get; private set; }
        public bool HasRolled { get; private set; }
        public bool IsBattling { get; private set; }
        public BattleDie(Die die, int team, BattleGameDieUI dieUI)
        {
            if (die == null) throw new System.ArgumentNullException(nameof(die));
            if (team < 0) throw new System.ArgumentException(nameof(team));
            if (dieUI == null) throw new System.ArgumentNullException(nameof(dieUI));
            Die = die;
            _dieUI = dieUI;
            _dieUI.Team = team;
            Die.OnStateChanged += OnStateChanged;
            _PlayAnimation(Animations.None, force: true);
        }
        public void PlayAnimation(Animations anim)
        {
            _PlayAnimation(anim);
        }
        public void EnterBattle()
        {
            if (IsBattling)
            {
                throw new System.InvalidOperationException(Die.name + " already battling");
            }
            HasRolled = false;
            IsBattling = true;
        }
        public void ExitBattle()
        {
            IsBattling = false;
        }
        public void Dispose()
        {
            Die.OnStateChanged -= OnStateChanged;
        }
        void _PlayAnimation(Animations anim, bool force = false)
        {
            if (force || (anim != _anim))
            {
                _anim = anim;
                _dieUI.AnimationName = _anim.ToString();
                Debug.Log(Die.name + " |> " + anim);
            }
        }
        public void OnStateChanged(Die die, Die.State newState)
        {
            Debug.Log(die.name + " => OnStateChanged " + newState);

            if ((newState != Die.State.Idle) && (newState != Die.State.Unknown))
            {
                IsRolling = true;
            }
            else if (IsRolling)
            {
                IsRolling = false;
                HasRolled = true;
                Debug.Log(die.name + " => rolled");

                LandedOnFace?.Invoke(this, Value);
            }
        }
        public override string ToString()
        {
            return Die.name;
        }
    }

    class BattleTeam
    {
        const int _teamNumD6 = 1;
        const int _teamNumD20 = 2;
        const int _teamNumD20s = 2;

        List<BattleDie> _dice = new List<BattleDie>();

        public string Name { get; }
        public int TeamNumber { get; }
        public IReadOnlyList<BattleDie> Dice => _dice;
        public IEnumerable<BattleDie> Dice6 => _dice.Where(d => d.Die.dieType == Die.DieType.SixSided);
        public IEnumerable<BattleDie> Dice20 => _dice.Where(d => d.Die.dieType == Die.DieType.TwentySided);
        public const int RequiredNumberOfDices = _teamNumD6 + _teamNumD20;
        
        public BattleTeam(int teamNumber)
        {
            Name = "team" + teamNumber;
            TeamNumber = teamNumber;
        }
        public BattleDie TryAddDie(Die die, BattleGameDieUI uiDie)
        {
            BattleDie battleDie = null;
            bool add = true;
#if TEAM_BATTLE
            add = (die.dieType == Die.DieType.SixSided) ? (Dice6?.Count() < _teamNumD6) : (Dice20?.Count() < _teamNumD20);
#endif
            if (add)
            {
                battleDie = new BattleDie(die, TeamNumber, uiDie);
                _dice.Add(battleDie);
            }
            return battleDie;
        }
        public BattleDie TryRemoveDie(Die die)
        {
            var battleDice = _dice.FirstOrDefault(d => d.Die == die);
            return _dice.Remove(battleDice) ? battleDice : null;
        }
        public bool TryEnterBattle()
        {
            bool enterBattle = _dice.Count(d => d.HasRolled) >= RequiredNumberOfDices;
            if (enterBattle)
            {
                foreach (var d in _dice)
                {
                    d.EnterBattle();
                }
            }
            return enterBattle;
        }
        public void ExitBattle()
        {
            foreach (var d in _dice)
            {
                d.ExitBattle();
            }
        }
        public void PlayAnimation(Animations anim)
        {
            foreach (var d in _dice)
            {
                d.PlayAnimation(anim);
            }
        }
    }

    // The list of dice that are part of the game
    List<Die> _dice = new List<Die>();

    AudioSource _audioSource;

    BattleTeam _team1 = new BattleTeam(1);
    BattleTeam _team2 = new BattleTeam(2);

    Coroutine _battleCr;

    public void PlayAnimation(string eventName)
    {
        if (System.Enum.TryParse(eventName, out Die.AnimationEvent anim))
        {
            foreach (var dieUI in _diceToggleGroup.GetComponentsInChildren<Toggle>()
                .Where(t => t.isOn)
                .Select(t => t.GetComponentInParent<BattleGameDieUI>()))
            {
                Debug.Log($"Playing {eventName} on {dieUI.die.name}");
                dieUI.die.PlayAnimationEvent(anim);            
            }
        }
        else
        {
            Debug.LogWarning("Bad event name: " + eventName);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_AutoConnect)
        {
            _ManualConnectBtn.interactable = false;
            StartCoroutine(PeriodicScanAndConnectCr());
        }
        else
        {
            _CurrentDicePool.DiceAdded += AddDieToGame;
            _CurrentDicePool.DiceRemoved += RemoveDieFromGame;
        }
    }

    // Update is called once per frame
    void Update()
    {
#if TEAM_BATTLE
        if (_battleCr == null)
        {
            bool team1EnterBattle = CheckTeamEnterBattle(_team1);
            bool team2EnterBattle = false;
            if (!team1EnterBattle)
            {
                team2EnterBattle = CheckTeamEnterBattle(_team2);
            }
            if (team1EnterBattle || team2EnterBattle)
            {
                _battleCr = StartCoroutine(BattleCr());
            }
        }
        else if ((_team1.Dice.Any(d => d.IsBattling) && _team1.Dice.Any(d => d.IsRolling || d.HasRolled))
              || (_team2.Dice.Any(d => d.IsBattling) && _team2.Dice.Any(d => d.IsRolling || d.HasRolled)))
        {
            CancelBattle(BattleCancelledReason.DieRoll);
        }
#endif
    }

    enum BattleCancelledReason { DieRoll, DieJoined, DieLeft, }

    void CancelBattle(BattleCancelledReason reason)
    {
        if (_battleCr != null)
        {
            StopCoroutine(_battleCr);
            _battleCr = null;
            Debug.Log($"<color=red>Battle canceled! ({reason})</color>");

            PlaySound(_Sounds.GameCancelled);
        }
        _team1.ExitBattle();
        _team2.ExitBattle();
    }

    bool CheckTeamEnterBattle(BattleTeam team, bool otherTeamInBattle = false)
    {
        bool enteredBattle = team.TryEnterBattle();
        if (enteredBattle)
        {
            team.PlayAnimation(Animations.FaceUp);
        }
        else
        {
            team.PlayAnimation(otherTeamInBattle ? Animations.WaitingForBattle : Animations.ShowTeam);
        }
        return enteredBattle;
    }

    IEnumerator BattleCr()
    {
        //TODO LIST
        // Alternate ShowTeam and something else when not in battle?
        // Stop show team has soon as not idle

        //
        // Wait for both teams to enter battle
        //

        PlaySound(_Sounds.NewPlayer);

        bool team1EnterBattle = _team1.Dice.Any(d => d.IsBattling);
        bool team2EnterBattle = _team2.Dice.Any(d => d.IsBattling);
        while ((!team1EnterBattle) || (!team2EnterBattle))
        {
            yield return null;

            team1EnterBattle = team1EnterBattle || CheckTeamEnterBattle(_team1, team2EnterBattle);
            team2EnterBattle = team2EnterBattle || CheckTeamEnterBattle(_team2, team1EnterBattle);
        }

        PlaySound(_Sounds.NewPlayer);

        Debug.Log("<color=red>Battle started!</color>");

        // Show faces for a little while
        yield return new WaitForSecondsRealtime(3);

        // And then stop animations
        _team1.PlayAnimation(Animations.None);
        _team2.PlayAnimation(Animations.None);
        yield return new WaitForSecondsRealtime(1);

        //
        // Pair dice for battle
        //

        BattleDie GetHighestValue(IEnumerable<BattleDie> dice)
        {
            var dieType = dice.FirstOrDefault(d => d.Die.dieType == Die.DieType.TwentySided)?.Die.dieType ?? Die.DieType.SixSided;
            return dice.Where(d => d.Die.dieType == dieType).OrderByDescending(d => d.Value).FirstOrDefault();
        }

        int battleScore = 0;
        int numPairs = Mathf.Min(_team1.Dice.Count, _team2.Dice.Count);
        var pairs = new List<System.ValueTuple<BattleDie, BattleDie>>(numPairs);
        for (int i = 0; i < numPairs; ++i)
        {
            // Find new pair (goes with D20 first)
            var die1 = GetHighestValue(_team1.Dice.Except(pairs.Select(p => p.Item1)));
            var die2 = GetHighestValue(_team2.Dice.Except(pairs.Select(p => p.Item2)));

            if ((die1 == null) || (die2 == null))
            {
                break;
            }

            // Keep pair (they might be of different type)
            pairs.Add((die1, die2));
            Debug.Log($"<color=green>Duel between {die1} ({die1.Die.dieType}) and {die2} ({die2.Die.dieType})</color>");

            die1.PlayAnimation(Animations.Duel);
            die2.PlayAnimation(Animations.Duel);
            PlaySound(_Sounds.DuelStarted);
    
            yield return new WaitForSecondsRealtime(2);

            // Find winner
            var winner = die1.Value > die2.Value ? die1 : (die2.Value > die1.Value ? die2 : null);
            if (winner != null)
            {
                var looser = winner == die1 ? die2 : die1;
                looser.PlayAnimation(Animations.DuelLoose);
                winner.PlayAnimation(Animations.DuelWin);
                battleScore += winner == die1 ? 1 : -1;
            }
            else
            {
                die1.PlayAnimation(Animations.DuelDraw);
                die2.PlayAnimation(Animations.DuelDraw);
            }

            yield return new WaitForSecondsRealtime(2);
        }

        //
        // Battle results
        //

        // Stop animations
        _team1.PlayAnimation(Animations.None);
        _team2.PlayAnimation(Animations.None);
        yield return new WaitForSecondsRealtime(1);

        if (pairs.Count >= numPairs)
        {
            if (battleScore != 0)
            {
                var winner = battleScore > 0 ? _team1 : _team2;
                var looser = winner == _team1 ? _team2 : _team1;
                Debug.Log($"<color=green>Winner: {winner.Name}</color>");
                looser.PlayAnimation(Animations.TeamLoose);
                winner.PlayAnimation(Animations.TeamWin);
            }
            else
            {
                Debug.Log("<color=green>Battle draw</color>");
                _team1.PlayAnimation(Animations.TeamDraw);
                _team2.PlayAnimation(Animations.TeamDraw);
            }

            PlaySound(_Sounds.GameCompleted);

            yield return new WaitForSecondsRealtime(5);
        }

        _battleCr = null;
        Debug.Log("<color=red>Battle finished!</color>");
    }

    void PlaySound(AudioClip[] clips)
    {
        if (clips != null)
        {
            int index = Random.Range(0, clips.Length - 1);
            _audioSource.PlayOneShot(clips[index]);
        }
    }

    void OnDieLandedOnFace(BattleDie die, int value)
    {
        if (_PlayFaceSounds.isOn)
        {
            Debug.Log("OnDieLandedOnFace " + value);

            if (_Sounds.FaceValues?.Length >= value)
            {
                _audioSource.PlayOneShot(_Sounds.FaceValues[value - 1]);
            }
            else
            {
                Debug.LogWarning("No sound for face value " + value);
            }
        }
    }

    IEnumerator PeriodicScanAndConnectCr()
    {
        var central = Central.Instance;

        yield return new WaitUntil(() => central.state == Central.State.Idle);
        central.onDieDiscovered += onDieDiscovered;

        while (true)
        {
            central.BeginScanForDice();
            yield return new WaitForSeconds(3.0f);
            central.StopScanForDice();
            yield return new WaitForSeconds(3.0f);
        }
    }

    void onDieDiscovered(Die die)
    {
        // Connect to the die!

        // Here we should check against a list of dice that we know are the
        // wrong type so we don't connect for no reason. For now we connect all the time

        die.OnConnectionStateChanged += onDieConnectionStateChanged;
        die.Connect();
    }

    void onDieConnectionStateChanged(Die die, Die.ConnectionState newState)
    {
        switch (newState)
        {
            case Die.ConnectionState.Ready:
                // Now we can check the type of the die
                if (die.dieType != Die.DieType.Unknown)
                {
                    // Yay, good die!

                    // We shouldn't need to filter double discovering the same die, but there is a bug
                    // in the virtual bluetooth interface...
                    if (!_dice.Contains(die))
                    {
                        AddDieToGame(die);
                    }
                }
                // Else we probably should disconnect and then keep a list
                // of invalid dice so we don't reconnected automatically
                break;
            case Die.ConnectionState.Unavailable:
                RemoveDieFromGame(die);
                break;
            default:
                break;
        }
    }

    void AddDieToGame(Die die)
    {
        _dice.Add(die);
        _UI.AddDie(die);

#if TEAM_BATTLE
        var team = _team1.Dice.Count <= _team2.Dice.Count ? _team1 : _team2;
        AddDieToTeam(team, die);
#else
        AddDieToTeam(_team2, die); // Add to blue team only
#endif
    }

    void RemoveDieFromGame(Die die)
    {
        Debug.Log("Removing die " + die.name);
        _UI.RemoveDie(die);
        _dice.Remove(die);

        RemoveDieFromTeam(_team1, die);
        RemoveDieFromTeam(_team2, die);
    }

    void AddDieToTeam(BattleTeam team, Die die)
    {
        var battleDie = team.TryAddDie(die, _UI.FindDie(die));
        if (battleDie != null)
        {
            battleDie.LandedOnFace += OnDieLandedOnFace;

            Debug.Log($"<color=blue>{team.Name} has a new member: {die.name} of type {die.dieType} ({team.Dice.Count}/{BattleTeam.RequiredNumberOfDices})</color>");
            CancelBattle(BattleCancelledReason.DieJoined);
        }
    }

    void RemoveDieFromTeam(BattleTeam team, Die die)
    {
        var battleDie = team.TryRemoveDie(die);
        if (battleDie != null)
        {
            battleDie.LandedOnFace -= OnDieLandedOnFace;

            Debug.Log($"<color=blue>{team.Name} lost a member: {die.name} of type {die.dieType} ({team.Dice.Count}/{BattleTeam.RequiredNumberOfDices})</color>");
            CancelBattle(BattleCancelledReason.DieLeft);

            var pendingDie = _dice.FirstOrDefault(d => _team1.Dice.Concat(_team2.Dice).All(bd => bd.Die != d));
            if (pendingDie != null)
            {
                AddDieToTeam(team, pendingDie);
            }
        }
    }
}
