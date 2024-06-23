using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship_Script : MonoBehaviour {

    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float accelerationSpeed;
    [SerializeField]
    private float rotationSpeed;
    private Vector2 target;
    private int patrolMode;
    [SerializeField]
    private Squad_Script mySquad;
    private Rigidbody2D myBody;

    // Start is called before the first frame update
    void Start() {
        myBody = transform.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update() {
        
        if(Input.GetMouseButtonDown(0)) {
            target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        
        moveTowardTarget();
    }

    public void newTarget(Vector3 position) {
        this.target = position;
    }

    public void setMySquad(Squad_Script myNewSquad) {
        this.mySquad = myNewSquad;
    }

    public void moveTowardTarget() {
        Vector2 targetSpeed = transform.up * maxSpeed;
        if((Mathf.Abs(myBody.velocity.x) + Mathf.Abs(myBody.velocity.y)) > Vector3.Distance(transform.position, target) - 1) targetSpeed = transform.up * (Vector3.Distance(transform.position, target)-1);
        if(Vector3.Distance(transform.position, target) < 1) targetSpeed = Vector2.zero;

        float remainingAngle = rotateTowardTarget();
        if (remainingAngle < 10) myBody.velocity = Vector2.MoveTowards(myBody.velocity, targetSpeed, accelerationSpeed * Time.deltaTime);
    }

    public float rotateTowardTarget() { //return the remaining rotation to execute
        float targetAngle = Vector2.SignedAngle(transform.up, target - (Vector2)transform.position);

        float newAngle = targetAngle * rotationSpeed * Time.deltaTime;

        Vector3 rotation = new Vector3(0,0, newAngle);
        transform.Rotate(rotation);

        return Vector2.Angle(transform.up, target - (Vector2)transform.position);

        /*
        rotation = Vector3.up;
        transform.Rotate(rotation);

        //Pour faire tourner le vaisseau sur lui-même
        */
    }
}
