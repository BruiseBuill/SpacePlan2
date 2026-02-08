using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuffEvent : MonoBehaviour
{
    public UnityAction<int> onUnlockBuilding = delegate { };
    CreateShipCP createShipCP;
    PlantBDCP plantBDCP;
    PlayerBrain playerBrain;
    //
    private void Awake()
    {
        createShipCP = GetComponent<CreateShipCP>();
        plantBDCP = GetComponent<PlantBDCP>();
        playerBrain = GetComponent<PlayerBrain>();
    }
    public void ChooseBuff(int id)
    {
        switch (id)
        {
            case 0:
                createShipCP.MoneyMultiple += 0.20f;
                break;
            case 1:
                createShipCP.UnlockShip(1);
                break;
            case 2:
                createShipCP.UnlockShip(2);
                break;
            case 3:
                plantBDCP.UnlockBuilding(0);
                onUnlockBuilding.Invoke(0);
                break;
            case 4:
                plantBDCP.UnlockBuilding(1);
                onUnlockBuilding.Invoke(1);
                break;
            case 5:
                createShipCP.UnlockWeapon(2);
                break;
            case 6:
                createShipCP.UnlockWeapon(3);
                break;
            case 13:
                playerBrain.BuildingPointMultiple += 0.1f;
                break;
            default:
                createShipCP.UnlockWeapon(id - 3);
                break;
        }
    }
}
