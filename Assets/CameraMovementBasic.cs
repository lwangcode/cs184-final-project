using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementBasic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Camera.main.transform.position += new Vector3(-0.04f, 0.0f, 0.04f);
        //Camera.main.transform.RotateAround(new Vector3(0,41,0), Vector3.up, 1f * Time.deltaTime);

       // Camera.main.transform.position += new Vector3(0.0f, -0.007f, 0f);
    }
}
