using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTPS : MonoBehaviour
{
    public bool lockCursor;         // permet de verrouiller le curseur lorsque la camera se deplace
    public float mouseSensitivity = 10;         // sensibilite de la souris pour la rotation de la camera
    public Transform target;        // l'objet cible que la camera suit
    public float targetOffset = 2;      // distance entre la camera et la cible
    public Vector2 xMinMax = new Vector2(-40, 85);      // limites de rotation verticale de la camera (en degres)

    public float rotationSmoothTime = 0.1f;         // temps pour que la rotation de la camera soit douce
    Vector3 rotationSmoothVelocity;         // vitesse de la rotation douce
    Vector3 currentRotation;        // rotation actuelle de la camera

    float yAxis;        // rotation horizontale de la camera
    float xAxis;        // rotation verticale de la camera

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;       // verrouille le curseur
            Cursor.visible = false;             // cache le curseur
        }
    }

    void LateUpdate()
    {
        yAxis += Input.GetAxis("Mouse X") * mouseSensitivity;       // obtient l'entree de rotation horizontale de la souris
        xAxis -= Input.GetAxis("Mouse Y") * mouseSensitivity;       // obtient l'entree de rotation verticale de la souris
        xAxis = Mathf.Clamp(xAxis, xMinMax.x, xMinMax.y);           // limite la rotation verticale de la camera entre les limites definies

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(xAxis, yAxis), ref rotationSmoothVelocity, rotationSmoothTime);       // rotation douce de la camera vers la rotation cible
        transform.eulerAngles = currentRotation;        // applique la rotation a la camera
        Vector3 desiredPos = target.position - transform.forward * targetOffset;        // positionne la camera derriere la cible a une distance definie
                                                                                        // Evitement d'obstacle
        RaycastHit hit;
        if (Physics.Linecast(target.position, desiredPos, out hit))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = desiredPos;
        }
    }
}