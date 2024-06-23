using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squad_Script : MonoBehaviour {
    [SerializeField]
    private Ship_Script[] shipList;

    private Vector3 target;
    private int patrolMode;
    private Vector3 avgSquadPosition;

    [SerializeField]
    private bool isSpawned;
    [SerializeField]
    private bool isVisible;

    // Start is called before the first frame update
    void Start() {
        shipList = transform.parent.GetComponentsInChildren<Ship_Script>(true);

        avgSquadPosition = transform.position;

        foreach(Ship_Script ship in shipList) {
            ship.setMySquad(this);
            ship.newTarget(this.target);
            ship.gameObject.SetActive(false);
            Debug.Log("Bonjour");
        }

        if(isSpawned) spawnSquad();
    }

    // Update is called once per frame
    void Update() {
        
    }

    void FixedUpdate() {
        updateMyPosition();
    }

    public void goTo(Vector3 position, int mode) {
        this.patrolMode = mode;
        goTo(position);
    }

    public void goTo(Vector3 position) {
        foreach(Ship_Script ship in shipList) {
            ship.newTarget(position);
        }
    }

    public void updateMyPosition() {
        updateSquadPosition();
        if(isSpawned) transform.position = avgSquadPosition;
    }

    public void updateSquadPosition() {
        Vector3 newPos = avgSquadPosition;
        int nbOfActiveShip = 0;
        foreach(Ship_Script ship in shipList) {
            if(ship.gameObject.activeSelf) {
                newPos += ship.transform.position;
                nbOfActiveShip++;
            }
        }
        if(nbOfActiveShip != 0) newPos /= nbOfActiveShip;
        avgSquadPosition = newPos;
    }

    public void updateSquadVisibily(bool state) {
        this.isVisible = state;
        foreach(Ship_Script ship in shipList) {
            ship.gameObject.GetComponent<SpriteRenderer>().enabled = isVisible;
        }
    }

    [ContextMenu("Spawn Squad")]
    public void spawnSquad() {
        foreach(Ship_Script ship in shipList) {
            ship.gameObject.SetActive(true);
        }
        this.isSpawned = true;
        updateSquadVisibily(isVisible);
    }

    [ContextMenu("Toggle visible")]
    public void toggleVisible() {
        updateSquadVisibily(!isVisible);
    }

    public Ship_Script[] getShipList() {
        return this.shipList;
    }
}
