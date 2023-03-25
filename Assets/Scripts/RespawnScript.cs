using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnScript : MonoBehaviour
{

    public Vector3 respawnPoint1 = new Vector3(175f,101f,-151f);
    public Vector3 respawnPoint2 = new Vector3(205f,93f,-188f);
    public Vector3 respawnPoint3 = new Vector3(245f,90f,-202f);

    private Collider currentTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            currentTrigger = this.GetComponent<BoxCollider>();

            if(currentTrigger.gameObject.name == "TriggerRespawn1")
            {
                PlayerMovement pm = other.GetComponent<PlayerMovement>();
                pm.Respawn(respawnPoint1);
            }
            else if(currentTrigger.gameObject.name == "TriggerRespawn2")
            {
                PlayerMovement pm = other.GetComponent<PlayerMovement>();
                pm.Respawn(respawnPoint2);
            }
            else if(currentTrigger.gameObject.name == "TriggerRespawn3")
            {
                PlayerMovement pm = other.GetComponent<PlayerMovement>();
                pm.Respawn(respawnPoint3);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Clear the reference to the current trigger box
            currentTrigger = null;
        }
    }
}