using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleGame : MonoBehaviour
{
    [SerializeField]
    BattleGameUI _UI;

    [SerializeField]
    Central _Central;

    // The list of dice that are part of the game
    List<Die> _dice = new List<Die>();

    enum Animations
    {
        None, ShowTeam, FaceUp, WaitingForBattle, Duel, DuelWin, DuelLoose, DuelDraw, TeamWin, TeamLoose, TeamDraw
    }

    class BattleDie : System.IDisposable
    {
        BattleGameDieUI _dieUI;
        Animations _anim;
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
            if ((newState != Die.State.Idle) && (newState != Die.State.Unknown))
            {
                IsRolling = true;
            }
            else if (IsRolling)
            {
                IsRolling = false;
                HasRolled = true;
                Debug.Log(die.name + " => rolled");
            }
        }
    }

    class BattleTeam
    {
        public string Name { get; }
        public List<BattleDie> Dice = new List<BattleDie>();
        public BattleTeam(string name)
        {
            Name = name;
        }
        public void PlayAnimation(Animations t)
        {
            foreach (var d in Dice)
            {
                d.PlayAnimation(t);
            }
        }
    }

    BattleTeam _team1 = new BattleTeam("team1");
    BattleTeam _team2 = new BattleTeam("team2");

    const int _teamSize = 3;
    Coroutine _battleCr;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PeriodicScanAndConnectCr());
    }

    // Update is called once per frame
    void Update()
    {
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
            CancelBattle("die roll");
        }
    }

    void CancelBattle(string reason)
    {
        if (_battleCr != null)
        {
            StopCoroutine(_battleCr);
            _battleCr = null;
            Debug.Log($"BATTLE CANCELLED ({reason})");
        }
        foreach (var d in _team1.Dice)
        {
            d.ExitBattle();
        }
        foreach (var d in _team1.Dice)
        {
            d.ExitBattle();
        }
    }

    bool CheckTeamEnterBattle(BattleTeam team, bool otherTeamInBattle = false)
    {
        bool enterBattle = team.Dice.Count(d => d.HasRolled) >= _teamSize;
        if (enterBattle)
        {
            foreach (var d in team.Dice)
            {
                d.EnterBattle();
            }
            team.PlayAnimation(Animations.FaceUp);
        }
        else
        {
            team.PlayAnimation(otherTeamInBattle ? Animations.WaitingForBattle : Animations.ShowTeam);
        }
        return enterBattle;
    }

    IEnumerator BattleCr()
    {
        //TODO LIST
        // Alternate ShowTeam and something else when not in battle?
        // Stop show team has soon as not idle
        // Stop show fqce up when duel starts
        // On die lost, allow delay to re-integrate same die

        //
        // Wait for both teams to enter battle
        //

        bool team1EnterBattle = _team1.Dice.Any(d => d.IsBattling);
        bool team2EnterBattle = _team2.Dice.Any(d => d.IsBattling);
        while ((!team1EnterBattle) || (!team2EnterBattle))
        {
            yield return null;

            team1EnterBattle = team1EnterBattle || CheckTeamEnterBattle(_team1, team2EnterBattle);
            team2EnterBattle = team2EnterBattle || CheckTeamEnterBattle(_team2, team1EnterBattle);
        }

        //
        // Pair dice for battle
        //

        BattleDie FindHighestValue(BattleTeam team, IEnumerable<BattleDie> excluded)
        {
            var dice = team.Dice.Except(excluded).OrderByDescending(d => d.Value).FirstOrDefault();
            if (dice != null)
            {
                dice.PlayAnimation(Animations.Duel);
            }
            return dice;
        }

        int battleScore = 0;
        var pairs = new List<System.ValueTuple<BattleDie, BattleDie>>(_teamSize);
        int numPairs = Mathf.Min(_team1.Dice.Count, _team2.Dice.Count);
        for (int i = 0; i < numPairs; ++i)
        {
            // Find pair
            var die1 = FindHighestValue(_team1, pairs.Select(p => p.Item1));
            var die2 = FindHighestValue(_team2, pairs.Select(p => p.Item2));

            if ((die1 == null) || (die2 == null))
            {
                break;
            }

            pairs.Add((die1, die2));

            yield return new WaitForSecondsRealtime(3);

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

            yield return new WaitForSecondsRealtime(3);
        }

        if (pairs.Count >= numPairs)
        {
            if (battleScore != 0)
            {
                var winner = battleScore > 0 ? _team1 : _team2;
                var looser = winner == _team1 ? _team2 : _team1;
                looser.PlayAnimation(Animations.TeamLoose);
                winner.PlayAnimation(Animations.TeamWin);
            }
            else
            {
                _team1.PlayAnimation(Animations.TeamDraw);
                _team2.PlayAnimation(Animations.TeamDraw);
            }

            yield return new WaitForSecondsRealtime(3);
        }

        Debug.Log("BATTLE FINISHED");
    }

    IEnumerator PeriodicScanAndConnectCr()
    {
        yield return new WaitUntil(() => _Central.state == Central.State.Idle);
        _Central.onDieDiscovered += onDieDiscovered;

        while (true)
        {
            _Central.BeginScanForDice();
            yield return new WaitForSeconds(3.0f);
            _Central.StopScanForDice();
            yield return new WaitForSeconds(3.0f);
        }
    }

    public void onDieDiscovered(Die die)
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
                if (die.dieType == Die.DieType.SixSided)
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

        var team = _team1.Dice.Count <= _team2.Dice.Count ? _team1 : _team2;
        AddDieToTeam(team, die);
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
        if (team.Dice.Count < _teamSize)
        {
            team.Dice.Add(new BattleDie(die, team == _team1 ? 1 : 2, _UI.FindDie(die)));
            Debug.Log($">> {team.Name} has a new member: {die.name} ({team.Dice.Count}/{_teamSize})");
            CancelBattle("die joined");
        }
    }

    void RemoveDieFromTeam(BattleTeam team, Die die)
    {
        foreach (var battleDie in team.Dice)
        {
            if (battleDie.Die == die)
            {
                Debug.Log($">> {team.Name} lost a member: {die.name} ({team.Dice.Count}/{_teamSize})");
                team.Dice.Remove(battleDie);
                CancelBattle("die left");

                var pendingDie = _dice.FirstOrDefault(d => _team1.Dice.Concat(_team2.Dice).All(bd => bd.Die != d));
                if (pendingDie != null)
                {
                    AddDieToTeam(team, pendingDie);
                }
                break;
            }
        }
    }
}
