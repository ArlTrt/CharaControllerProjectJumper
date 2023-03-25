using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //movement
    public float walkSpeed = 2;             // la vitesse a laquelle le joueur se deplace en marchant
    public float runSpeed = 6;              // la vitesse a laquelle le joueur se deplace en courant
    public float turnSmoothTime = 0.2f;         // le temps qu'il faut pour que le joueur tourne en douceur
    float turnSmoothVelocity;                   // la vitesse de la rotation
    public float speedSmoothTime = 0.2f;        // le temps qu'il faut pour que la vitesse du joueur change en douceur
    float speedSmoothVelocity;                  // la vitesse du changement de vitesse doux
    float currentSpeed;                         // la vitesse actuelle du joueur
    float velocityY;                            // la vitesse verticale actuelle du joueur

    //jump
    public float gravity = -12;             // la force de la gravite appliquee au joueur
    public float jumpHeight = 1;            // la hauteur du saut du joueur
    [Range(0, 1)]                       // restreint la variable entre 0 et 1
    public float airControlPercent;         // la quantite de controle que le joueur a en l'air, en pourcentage
    public int remainingJumps = 1;          // le nombre de jumps qui reste au player
    private bool canJump = true;            // bool qui indique si on peu sauter ou non
    private bool isFalling = false;         // indique si le player tombe ou non

    //wallrun
    private RaycastHit leftWallhit;         // raycast qui detecte les murs a gauche
    private RaycastHit rightWallhit;        // raycast qui detecte les murs a droite
    private float wallRunDuration = 1.0f;       // definie la duree maximal d'un wallrun
    private float wallRunStartTime;             // recupere le moment ou commence le wallrun
    private Vector3 originalGravity;            
    private bool isWallRunning = false;         // bool qui determine si le joueur wallrun ou non
    private float raycastDistance = 1.0f;       // distance a laquel les raycasts detectes des murs
    private bool wallLeft;                  //bool mur a gauche
    private bool wallRight;                 //bool mur a droite

    //slide
    private bool isSliding;
    private float slideTimer;
    private float slideTimerMax = 1f;

    Animator animator;                  // le composant animator attache au joueur
    Transform cameraT;                  // la transform de la camera principale
    CharacterController controller;     // le composant CharacterController attache au joueur

    private void Start()
    {
        animator = GetComponent<Animator>();                // obtient le composant animator
        cameraT = Camera.main.transform;                    // obtient la transform de la camera principale
        controller = GetComponent<CharacterController>();       // obtient le composant CharacterController
    }

    private void Update()
    {
        CheckGrounded();                // appelle la fonction CheckGrounded()
       // Debug.Log(remainingJumps);
        if (Input.GetKeyDown(KeyCode.Space) && remainingJumps ==1 && canJump)        // verifie si le joueur appuie sur la barre d'espacement et touche le sol, calcule et applique la vitesse de saut
        {
            animator.SetTrigger("isJumpingT");              // lance l'animationde jump
            StartCoroutine(Jump());                     //appelle la fonction coroutine Jump()
        }
        if (Input.GetKeyDown(KeyCode.Space) && remainingJumps == 0 && canJump)        // verifie si le joueur appuie sur la barre d'espacement et touche le sol, calcule et applique la vitesse de saut
        {
            animator.SetTrigger("isDJumpingT");              // lance l'animationde jump
            StartCoroutine(Jump());                     //appelle la fonction coroutine Jump()
        }

        
       // Debug.Log(velocityY);
        if(velocityY < -1 && !controller.isGrounded)            // verifie si la velocite est negative et que le player nest pas au sol, active le booleen isFalling
        {
            isFalling = true;
        }
        else if(controller.isGrounded)                  // sinon si le player est au sol, desactive le booleen de fall
        {
            isFalling = false;
        }
        animator.SetBool("isFalling", isFalling);       // lance lanimation de falling

        CheckForWall();                                             //appelle la fonction CheckForWall()
        if (Input.GetKeyDown(KeyCode.Space) && isWallRunning)
        {                                                           //si le joueur appui sur espace et quil wallrun, appelle la fonction EndWallRun()
            EndWallRun();
        }

        if (isWallRunning)
        {                                                           // si iswallrunning, verifie si la duree du wallrun a expire et appelle EndWallRun
            if (Time.time - wallRunStartTime >= wallRunDuration)
            {
                EndWallRun();
            }
        }
        else
        {                                               //sinon si wallLeft ou wallRight et shift, appelle StartWallRun()
           // Debug.Log(wallLeft + "L");
           // Debug.Log(wallRight + "R");
            if ((wallLeft || wallRight) && Input.GetKey(KeyCode.LeftShift))
            {
                StartWallRun();
            }
        }
    }

    private void FixedUpdate()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));      // obtient l'entree des axes horizontaux et verticaux
        Vector2 inputDir = input.normalized;                    // normalise l'entree pour le mouvement diagonal
        bool running = Input.GetKey(KeyCode.LeftShift);         // verifie si le joueur appuie sur la touche Maj gauche

        Move(inputDir, running);                //appelle la fonction Move()

        float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * 0.5f);      // calcule la vitesse d'animation en pourcentage de la vitesse actuelle du joueur
        animator.SetFloat("speedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);                  // definit le parametre "speedPercent" de l'animator avec la vitesse d'animation calculee

        if (isWallRunning)
        {                                   // si le player wallrun, appelle la fonction WallRunningMovement()
            WallRunningMovement();
        }
    }

    void Move(Vector2 inputDir, bool running)       // fonction pour deplacer le joueur
    {
        if (inputDir != Vector2.zero)   //si le joueur bouge
        {

            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;         // calcule la rotation cible en fonction de la direction d'entree et de la direction de la camera
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));         // fait tourner le joueur vers la rotation cible en douceur
        }


        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;        // calcule la vitesse cible en fonction de si le joueur court ou marche
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));        // change la vitesse du joueur vers la vitesse cible en douceur

        velocityY += Time.deltaTime * gravity;        // applique la gravite a la vitesse verticale du joueur
        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * velocityY;        // calcule la vitesse totale pour deplacer le joueur

        controller.Move(velocity * Time.deltaTime);        // deplace le joueur
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;     // met a jour la vitesse actuelle du joueur

        if (controller.isGrounded)        // si le joueur touche le sol, met sa vitesse verticale a 0
        {
            velocityY = 0;
        }
    }

    float GetModifiedSmoothTime(float smoothTime)       // fonction pour obtenir le temps de transition en douceur modifie pour le mouvement
    {
        if (controller.isGrounded)        // si le joueur touche le sol, retourne le temps de transition en douceur normal
        {
            return smoothTime;
        }

        if (airControlPercent == 0)        // si le pourcentage de controle en l'air est defini sur 0, retourne une valeur tres elevee
        {
            return float.MaxValue;
        }

        return smoothTime / airControlPercent;        // retourne le temps de transition en douceur divise par le pourcentage de controle en l'air
    }

    void CheckGrounded()
    {
        if (controller.isGrounded)
        {                                                   //si le player touche le sol, il ne tombe plus et il reprend ses remaining jumps
            animator.ResetTrigger("isJumpingT");
           // Debug.Log("sol");
            isFalling = false;
            remainingJumps = 1;
        }
        else
        {
            Debug.Log("air");
        }
    }

    private IEnumerator Jump()              
    {
        canJump = false;                                                // ne peu plus sauter
        //isFalling = false;                                              // ne tombe plus
        yield return new WaitForSeconds(0.1f);                              //attend 0.1f seconde
        float jumpVelocity = Mathf.Sqrt(-2 * gravity * jumpHeight);         //augmente la velocity vers le haut pour jump
        velocityY = jumpVelocity;                                           //assigne la valeur a la velocite y de player
        remainingJumps--;                                               // enleve un jump
        //yield return new WaitForSeconds(0.2f);
        canJump = true;                                     // peu a nouveau sauter
    }

    private void CheckForWall()
    {
        Debug.DrawRay(transform.position, Quaternion.Euler(0, 45, 0) * transform.forward, Color.green);
        wallRight = Physics.Raycast(transform.position, Quaternion.Euler(0, 45, 0) * transform.forward, out rightWallhit, raycastDistance);         //lance un raycast a 45 degres vers la droite du personnage
        if (wallRight && rightWallhit.transform.tag == "Wall")
        {                                                                   //si il detecte a droite un mur avec le tag "wall", active wallright, sinon non
            wallRight = true;
        }
        else
        {
            wallRight = false;
        }
        Debug.DrawRay(transform.position, Quaternion.Euler(0, -45, 0) * transform.forward, Color.blue);
        wallLeft = Physics.Raycast(transform.position, Quaternion.Euler(0, -45, 0) * transform.forward, out leftWallhit, raycastDistance);         //lance un raycast a 45 degres vers la gauche du personnage
        if (wallLeft && leftWallhit.transform.tag == "Wall")
        {                                                                   //si il detecte a gauche un mur avec le tag "wall", active wallleft, sinon non
            wallLeft = true;
        }
        else
        {
            wallLeft = false;
        }
    }

    private void StartWallRun()
    {
        // Definis la gravite a 0 pour le wallrun
        gravity = -0.5f;

        // Demarrez l'animation de wallrun
        //animator.SetBool("isWallRunning", true);

        // Enregistre le moment ou le wallrun a demarre
        wallRunStartTime = Time.time;

        // Definis isWallRunning a true
        isWallRunning = true;
    }

    private void WallRunningMovement()
    {
        velocityY = 0;              //velocite sur y est mise a zero

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;          // calcule les normales du mur 

        /*Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((transform.forward - wallForward).magnitude > (transform.forward - -wallForward).magnitude)
            wallForward = -wallForward;*/

        // push to wall force
        //if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
        controller.Move(-wallNormal * 100 * Time.deltaTime);        // ajoute une force vers le mur
    }

    private void EndWallRun()
    {
        // Retablis la gravite originale
        gravity = -12;

        // Arrete l'animation de wallrun
        //animator.SetBool("isWallRunning", false);

        // Definis isWallRunning a false
        isWallRunning = false;
    }

    private void Slide()
    {
        if (Input.GetKeyDown("c") && !isSliding) // press C to slide
        {
            slideTimer = 0.0f; // start timer
            isSliding = true;
            //slideForward = tr.forward;
        }/*
        if (isSliding)
        {
            h = 0.5 * height; // height is crouch height
            speed = slideSpeed; // speed is slide speed
            chMotor.movement.velocity = slideForward * (speed * 2);
         
            slideTimer += Time.deltaTime;
            if (slideTimer > slideTimerMax)
            {
             isSliding = false;
            }
        } */
    }
    /*  
     
     // - apply movement modifiers -    
     chMotor.movement.maxForwardSpeed = speed; // set max speed
     var lastHeight = ch.height; // crouch/stand up smoothly 
     ch.height = Mathf.Lerp(ch.height, h, 5 * Time.deltaTime);
     tr.position.y += (ch.height - lastHeight) / 2; // fix vertical position*/

    

    public void Respawn(Vector3 respawnPoint)
    {
        CharacterController cc = GetComponent<CharacterController>();
 
        cc.enabled = false;
        transform.position = respawnPoint;
        cc.enabled = true;
        
    }
}







/*
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private float wallRunDuration = 2.0f;
    private float wallRunStartTime;
    private Vector3 originalGravity;
    private bool isWallRunning = false;
    private float raycastDistance = 1.0f;
    private bool wallLeft;
    private bool wallRight;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerMovementAdvanced pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // State 1 - Wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            if (!pm.wallrunning)
                StartWallRun();
        }

        // State 3 - None
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards force
        if (upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (downwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

        // push to wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;
    }
}*/

/*using UnityEngine;

public class WallRun : MonoBehaviour
{
    private Rigidbody2D rigidbody2D;
    private Animator animator;
    private float wallRunDuration = 2.0f;
    private float wallRunStartTime;
    private float originalGravityScale;
    private bool isWallRunning = false;

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        originalGravityScale = rigidbody2D.gravityScale;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isWallRunning)
        {
            EndWallRun();
        }

        if (isWallRunning)
        {
            // V�rifiez si la dur�e du wallrun a expir�
            if (Time.time - wallRunStartTime >= wallRunDuration)
            {
                EndWallRun();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // V�rifiez si le joueur peut effectuer un wallrun
        if (collision.gameObject.tag == "Wall" && Input.GetKey(KeyCode.LeftShift))
        {
            StartWallRun();
        }
    }

    private void StartWallRun()
    {
        // D�finissez la gravit� � 0 pour le wallrun
        rigidbody2D.gravityScale = 0;

        // D�marrez l'animation de wallrun
        animator.SetBool("isWallRunning", true);

        // Enregistrez le moment o� le wallrun a d�marr�
        wallRunStartTime = Time.time;

        // D�finissez isWallRunning � true pour permettre la fin du wallrun
        isWallRunning = true;
    }

    private void EndWallRun()
    {
        // R�tablissez la gravit� originale
        rigidbody2D.gravityScale = originalGravityScale;

        // Arr�tez l'animation de wallrun
        animator.SetBool("isWallRunning", false);

        // D�finissez isWallRunning � false pour emp�cher la fin du wallrun
        isWallRunning = false;
    }
}
*/