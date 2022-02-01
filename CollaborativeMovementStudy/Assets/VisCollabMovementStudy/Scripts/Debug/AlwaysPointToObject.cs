using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysPointToObject : MonoBehaviour
{
    public GameObject Target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //var rotation = Quaternion.LookRotation(Target.transform.position);
        gameObject.transform.LookAt(Target.transform);
    }
}
