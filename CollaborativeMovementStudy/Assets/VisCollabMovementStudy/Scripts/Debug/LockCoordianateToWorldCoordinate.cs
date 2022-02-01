using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCoordianateToWorldCoordinate : MonoBehaviour
{

    public bool X = false;
    public bool Y = false;
    public bool Z = false;

    public float lockXToWorld = 0;
    public float lockYToWorld = 0;
    public float lockZToWorld = 0;

    // Start is called before the first frame update
    void Start() { 

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 currentPos = gameObject.transform.position;

        gameObject.transform.position = new Vector3(
            X ? lockXToWorld : currentPos.x,
            Y ? lockYToWorld : currentPos.y,
            Z ? lockZToWorld : currentPos.z
        );
    //Debug.Log("x "+X +" " + (X ? lockXToWorld : currentPos.x));
    //Debug.Log("y "+Y + " " + (Y ? lockYToWorld : currentPos.y));
    //Debug.Log("z "+Z + " " + (Z ? lockZToWorld : currentPos.z));

}
}
