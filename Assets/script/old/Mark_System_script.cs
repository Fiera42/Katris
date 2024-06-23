using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mark_System_script : MonoBehaviour {
    [SerializeField]
    private Mark_data_script[] markList;

    [SerializeField]
    private GameObject[] prefabList;

    public void newMark(float x, float y, string type, GameObject source, GameObject destination) {
        bool exist = false;
        foreach(Mark_data_script mark in markList) {
            if(mark.source == source && mark.destination == destination) {
                exist = true;
                break;
            }
        }
        if(!exist)
        foreach(GameObject markType in prefabList) {
            if(markType.name == type) {
                extendArraySize();
                markList[markList.Length - 1] = Instantiate(markType, new Vector3(x,y, transform.position.y), transform.rotation).GetComponent<Mark_data_script>();
                markList[markList.Length - 1].source = source;
                markList[markList.Length - 1].destination = destination;
            }
        }
    }
    
    public void removeMark(GameObject targetSource, GameObject targetDestination) {
        for(int i = 0; i < markList.Length; i++) {
            while(i < markList.Length && markList[i].source == targetSource && markList[i].destination == targetDestination) removeAt(i);
        }
    }

    private void removeAt(int index) {
        Mark_data_script[] newMarkList = new Mark_data_script[markList.Length - 1];

        Destroy(markList[index].gameObject);

        for(int i = 0; i < index; i++) {
            newMarkList[i] = markList[i];
        }

        for(int i = index + 1; i < markList.Length; i++) {
            newMarkList[i - 1] = markList[i];
        }

        markList = newMarkList;
    }

    private void extendArraySize() {
        Mark_data_script[] newMarkList = new Mark_data_script[markList.Length + 1];

        for(int i = 0; i < markList.Length; i++) {
            newMarkList[i] = markList[i];
        }

        markList = newMarkList;
    }

    void Start() {
        newMark(10, 10, "Ally", gameObject, gameObject);
        newMark(-10, -10, "Danger", gameObject, gameObject);
        removeMark(gameObject, gameObject);
    }
}
