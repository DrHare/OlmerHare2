using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetButtonDown("Horizontal"))
        {
            //this.gameObject.transform = new Vector3(transform.position.x + Input.GetButtonDown("Horizontal"), this, transform.position.y, transform.position.z + Input("Horizontal"));
        }
	}
}
