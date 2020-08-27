using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DicePoolRefresher : MonoBehaviour
{
    void Start()
    {
        DicePool.Instance.StartCoroutine(ScanLoopCr());
    }

    IEnumerator ScanLoopCr()
    {
        while (true)
        {
            yield return new WaitUntil(() => gameObject.activeSelf);
            foreach (var die in DiceManager.Instance.allDice)
            {
                bool dieConnected = false;
                yield return DiceManager.Instance.ConnectDie(die.editDie, (_, d, errorMsg) => dieConnected = d != null);
                if (dieConnected)
                {
                    // Fetch battery level
                    bool battLevelReceived = false;
                    die.die.GetBatteryLevel((d, f) => battLevelReceived = true);
                    yield return new WaitUntil(() => battLevelReceived == true);

                    // Fetch rssi
                    bool rssiReceived = false;
                    die.die.GetRssi((d, i) => rssiReceived = true);
                    yield return new WaitUntil(() => rssiReceived == true);
                }
                DiceManager.Instance.DisconnectDie(die.editDie);
            }

            yield return new WaitForSeconds(AppConstants.Instance.DicePoolViewScanDelay);
        }
    }
}
