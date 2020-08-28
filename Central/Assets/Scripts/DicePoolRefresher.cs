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
            yield return DiceManager.Instance.RefreshPool();
            yield return new WaitForSeconds(AppConstants.Instance.DicePoolViewScanDelay);
        }
    }
}
