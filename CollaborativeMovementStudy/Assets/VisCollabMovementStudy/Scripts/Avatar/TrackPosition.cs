using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackPosition : MonoBehaviour
{

    public GameObject toFollow;
    public Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(toFollow != null || toFollow)
        {
            gameObject.transform.position = toFollow.transform.position + offset;

            var f_y = toFollow.transform.eulerAngles.y;
            var q_y = Quaternion.AngleAxis(f_y, Vector3.up);
            gameObject.transform.rotation = q_y;
        }

    }

    void ReportSpot(Transform target)
    {

        Vector3 direction = transform.InverseTransformDirection((transform.position - target.position).normalized);

        float azimuth = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;


        float foo = (Mathf.RoundToInt(azimuth) + 180);
        dbg_azimuth = azimuth;
        dbg_foo = foo;
        //Debug.Log("Enemy " + target.gameObject.name + "at azimuth " + foo + "!");
    }

    public float dbg_azimuth = 0f;
    public float dbg_foo = 0f;
}
