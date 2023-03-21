using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnScript : MonoBehaviour
{

    private Vector3 respawnPoint = new Vector3(175f,101f,-151f);

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("EnterTrigger " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Moving " + other.name);
            Debug.Log(other.transform.position + other.name + "transform no1");
            other.transform.position = respawnPoint;
            Debug.Log(other.transform.position + other.name+ "transform no2 voilaa");
        }
        Debug.Log(other.transform.position + other.name + "jnhgfds");

    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
