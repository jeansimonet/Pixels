using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DicePoolRefresher : MonoBehaviour
{
    public delegate void RefreshPoolEvent();
    public RefreshPoolEvent onBeginRefreshPool;
    public RefreshPoolEvent onEndRefreshPool;

    void Start()
    {
        DicePool.Instance.StartCoroutine(ScanLoopCr());
    }

    IEnumerator ScanLoopCr()
    {
        while (true)
        {
            yield return new WaitUntil(() => gameObject.activeSelf);
            onBeginRefreshPool?.Invoke();

            // Did we have any dice that needed to be rechecked?
            if (DicePool.Instance.allDice.Any(d => d.connectionState == Die.ConnectionState.Unknown))
            {
                float startTime = Time.time;
                float endTime = startTime + AppConstants.Instance.ScanTimeout;
                DicePool.Instance.RequestBeginScanForDice();

                yield return new WaitUntil(() => !DicePool.Instance.allDice.Any(d => d.connectionState == Die.ConnectionState.Unknown) || Time.time >= endTime);

                DicePool.Instance.RequestStopScanForDice();
            }

            // Any connected or available die, refresh battery level
            foreach (var die in DicePool.Instance.allDice)
            {
                yield return StartCoroutine(IdentifyOneDieCr(die));
            }

            onEndRefreshPool?.Invoke();

            yield return new WaitForSeconds(AppConstants.Instance.DicePoolViewScanDelay);
        }
    }

    IEnumerator IdentifyOneDieCr(Die die)
    {
        if (die.connectionState == Die.ConnectionState.Available || die.connectionState == Die.ConnectionState.Ready)
        {
            DicePool.Instance.RequestConnectDie(die);

            yield return new WaitUntil(() => die.connectionState == Die.ConnectionState.Ready || die.connectionState == Die.ConnectionState.CommError);

            if (die.connectionState == Die.ConnectionState.Ready)
            {
                // Fetch battery level
                bool battLevelReceived = false;
                die.GetBatteryLevel((d, f) => battLevelReceived = true);
                yield return new WaitUntil(() => battLevelReceived == true);
            }

            DicePool.Instance.RequestDisconnectDie(die);
        }
    }
}
