using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class VirtualBluetoothInterface : MonoBehaviour, ICoroutineManager
{
    public List<VirtualDie> virtualDice
    {
        get;
        private set;
    }

    int globalAddressCounter;
    IEnumerator currentCoroutine;
    bool scanning = false;

    string dieServiceGUID = "fe84";
    string subscribeCharacteristic = "2d30c082-f39f-4ce6-923f-3484ea480596";
    string writeCharacteristic = "2d30c083-f39f-4ce6-923f-3484ea480596";

    string[] fakeServices =
    {
        "Service1",
        "Don't connect me bro!",
        "fe88", // Close but no cigar
        "3c659e52-5577-40b1-be8c-65d15dc09ed8"
    };

    string[] fakeCharacteristics =
    {
        "Characteristic1",
        "Characteristic2",
        "Characteristic3",
        "2c251def-2312-43ae-b4cf-ef20aa2d2f1f"
    };

    void Awake()
    {
        virtualDice = new List<VirtualDie>();
        globalAddressCounter = Random.Range(0, 1000);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddDie()
    {
        VirtualDie newDie = new VirtualDie("VDie" + virtualDice.Count, "@VDie" + globalAddressCounter, this);
        virtualDice.Add(newDie);
        globalAddressCounter++;
    }

    public void RemoveDie(VirtualDie die)
    {
        die.Die();
        virtualDice.Remove(die);
    }

    public bool IsVirtualDie(string address)
    {
        return virtualDice.Any(d => d.address == address);
    }

    public void ScanForPeripheralsWithServices(string[] serviceUUIDs, System.Action<string, string> action, System.Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF)
    {
        //if (currentCoroutine != null)
        //{
        //    StopCoroutine(currentCoroutine);
        //}
        StartCoroutine(currentCoroutine = ScanForPeripheralsWithServicesCr(serviceUUIDs, action));
    }


    public void StopScan()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
    }

    public void ConnectToPeripheral(string name, System.Action<string> connectAction, System.Action<string, string> serviceAction, System.Action<string, string, string> characteristicAction, System.Action<string> disconnectAction = null)
    {
        //if (currentCoroutine != null)
        //{
        //    StopCoroutine(currentCoroutine);
        //}
        StartCoroutine(currentCoroutine = ConnectToPeripheralCr(name, connectAction, serviceAction, characteristicAction, disconnectAction));
    }


    public void SubscribeCharacteristic(string name, string service, string characteristic, System.Action<string> notificationAction, System.Action<string, byte[]> action)
    {
        //if (currentCoroutine != null)
        //{
        //    StopCoroutine(currentCoroutine);
        //}
        StartCoroutine(currentCoroutine = SubscribeCharacteristicCr(name, service, characteristic, notificationAction, action));
    }


    public void DisconnectPeripheral(string name, System.Action<string> action)
    {
        //if (currentCoroutine != null)
        //{
        //    StopCoroutine(currentCoroutine);
        //}
        StartCoroutine(currentCoroutine = DisconnectPeripheralCr(name, action));
    }


    public void DisconnectAll()
    {
        //if (currentCoroutine != null)
        //{
        //    StopCoroutine(currentCoroutine);
        //}
        foreach (var die in virtualDice)
        {
            die.Die();
        }
    }


    public void ReadCharacteristic(string name, string service, string characteristic, System.Action<string, byte[]> action)
    {
        //if (currentCoroutine != null)
        //{
        //    StopCoroutine(currentCoroutine);
        //}
        StartCoroutine(currentCoroutine = ReadCharacteristicCr(name, service, characteristic, action));
    }


    public void WriteCharacteristic(string name, string service, string characteristic, byte[] data, int length, bool withResponse, System.Action<string> action)
    {
        //if (currentCoroutine != null)
        //{
        //    StopCoroutine(currentCoroutine);
        //}
        StartCoroutine(currentCoroutine = WriteCharacteristicCr(name, service, characteristic, data, length, withResponse, action));
    }

    IEnumerator ScanForPeripheralsWithServicesCr(string[] serviceUUIDs, System.Action<string, string> action)
    {
        scanning = true;
        // Shuffle the current dice so we don't always "discover" them in the same order
        List<VirtualDie> shuffledDice = new List<VirtualDie>(virtualDice);
        shuffledDice.Shuffle();

        // Check if they are indeed the right kind of devices
        foreach (var die in shuffledDice)
        {
            if (!scanning)
                break;
            // Todo: Handle long GUIDS!!!
            if (serviceUUIDs.Any(uuid => uuid.ToLower() == dieServiceGUID))
            {
                yield return new WaitForSecondsRealtime(Random.Range(0, 0.25f));
                if (action != null)
                {
                    action(die.address, die.name);
                }
            }
        }

        // No we wait for new dice maybe
        int currentListSize = shuffledDice.Count;
        while (scanning)
        {
            if (virtualDice.Count > currentListSize)
            {
                var newListCopy = new List<VirtualDie>(virtualDice.Where(d => !shuffledDice.Contains(d)));
                foreach (var die in newListCopy)
                {
                    if (serviceUUIDs.Any(uuid => uuid.ToLower() == dieServiceGUID))
                    {
                        yield return new WaitForSecondsRealtime(Random.Range(0, 0.25f));
                        if (action != null)
                        {
                            action(die.address, die.name);
                        }
                    }
                }

                shuffledDice.AddRange(newListCopy);
                currentListSize = shuffledDice.Count;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator ConnectToPeripheralCr(string address, System.Action<string> connectAction, System.Action<string, string> serviceAction, System.Action<string, string, string> characteristicAction, System.Action<string> disconnectAction)
    {
        // Find the die
        var die = virtualDice.First(d => d.address == address);
        if (die != null)
        {
            yield return new WaitForSecondsRealtime(Random.Range(0, 0.1f));
            die.Connect(() => disconnectAction(die.address));
            if (connectAction != null)
            {
                connectAction(die.address);
            }

            // Pretend-discover services and characteristics
            foreach (var s in fakeServices)
            {
                yield return new WaitForSecondsRealtime(Random.Range(0, 0.1f));
                if (serviceAction != null)
                {
                    serviceAction(die.address, s);
                }

                foreach (var c in fakeCharacteristics)
                {
                    yield return new WaitForSecondsRealtime(Random.Range(0, 0.05f));
                    characteristicAction(die.address, s, c);
                }
            }

            // Also discover the right service
            if (serviceAction != null)
            {
                serviceAction(die.address, dieServiceGUID);
            }

            // Spam with some fake characteristics
            foreach (var c in fakeCharacteristics)
            {
                yield return new WaitForSecondsRealtime(Random.Range(0, 0.05f));
                if (characteristicAction != null)
                {
                    characteristicAction(die.address, dieServiceGUID, c);
                }
            }

            // But also pass in the right ones
            yield return new WaitForSecondsRealtime(Random.Range(0, 0.05f));
            if (characteristicAction != null)
            {
                characteristicAction(die.address, dieServiceGUID, subscribeCharacteristic);
                characteristicAction(die.address, dieServiceGUID, writeCharacteristic);
            }
        }
    }

    IEnumerator SubscribeCharacteristicCr(string name, string service, string characteristic, System.Action<string> notificationAction, System.Action<string, byte[]> action)
    {
        var die = virtualDice.First(d => d.address == name);
        if (die != null && service == dieServiceGUID && characteristic == subscribeCharacteristic)
        {
            // Pretend it take a little bit to register
            yield return new WaitForSecondsRealtime(Random.Range(0, 0.05f));

            // Hook up to the die!
            die.Subscribe((data) =>
            {
                if (notificationAction != null)
                {
                    notificationAction(die.address);
                }
                if (action != null)
                {
                    action(die.address, data);
                }
            });
        }
    }

    IEnumerator DisconnectPeripheralCr(string name, System.Action<string> action)
    {
        var die = virtualDice.First(d => d.address == name);
        if (die != null)
        {
            yield return new WaitForSecondsRealtime(Random.Range(0, 0.05f));
            die.Die();
            if (action != null)
            {
                action(die.name);
            }
        }
    }

    IEnumerator ReadCharacteristicCr(string name, string service, string characteristic, System.Action<string, byte[]> action)
    {
        var die = virtualDice.First(d => d.address == name);
        if (die != null && service == dieServiceGUID && characteristic == subscribeCharacteristic)
        {
            yield return new WaitForSecondsRealtime(Random.Range(0, 0.05f));
            if (action != null)
            {
                action(die.address, die.Read());
            }
        }
    }

    IEnumerator WriteCharacteristicCr(string name, string service, string characteristic, byte[] data, int length, bool withResponse, System.Action<string> action)
    {
        var die = virtualDice.First(d => d.address == name);
        if (die != null && service == dieServiceGUID && characteristic == writeCharacteristic)
        {
            yield return new WaitForSecondsRealtime(Random.Range(0, 0.05f));
            die.Write(data, length);
            if (withResponse)
            {
                yield return new WaitForSecondsRealtime(Random.Range(0, 0.1f));
            }
            if (action != null)
            {
                action(die.address);
            }
        }
    }

    Coroutine ICoroutineManager.StartCoroutine(IEnumerator en)
    {
        return base.StartCoroutine(en);
    }

    void ICoroutineManager.StopCoroutine(Coroutine cor)
    {
        base.StopCoroutine(cor);
    }

}


public static class ShuffleExtension
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
