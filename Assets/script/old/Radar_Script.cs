using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar_Script : MonoBehaviour {

    [SerializeField]
    private GameObject[] detectableList;
    //[SerializeField]
    //private GameObject[] ships;
    [SerializeField]
    private float radarRange;
    [SerializeField]
    private float viewRange;
    //[SerializeField]
    //private float radioRange;
    [SerializeField]
    private Mark_System_script markSystem;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update(){
        detectableList = GameObject.FindGameObjectsWithTag("Detectable");
        //ships = GameObject.FindGameObjectsWithTag("Ship");

        scan();
    }

    private void scan() {
        foreach(GameObject scan in detectableList) {
            if(scan != gameObject)
            if(Vector3.Distance(transform.position, scan.transform.position) < radarRange) {
                markSystem.newMark(scan.transform.position.x, scan.transform.position.y, "Danger", scan, gameObject);
            }
            else markSystem.removeMark(scan, gameObject);

            if(Vector3.Distance(transform.position, scan.transform.position) < viewRange) {
                markSystem.removeMark(scan, gameObject);
                scan.GetComponent<SpriteRenderer>().enabled = true;
            }
            else scan.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    /*
    private void communicate(GameObject information) {
        foreach(GameObject ship in ships) {
            if(ship != gameObject)
            if(Vector3.Distance(transform.position, ship.transform.position) < radioRange) {
                ship.GetComponent<Radar_Script>().receive(information);
            }
        }
    }

    public void receive(GameObject information) {
        if(markSystem != null) {
            markSystem.newMark(information.transform.position.x, information.transform.position.y, "Danger", information, gameObject);
        }
    }
    */
}
