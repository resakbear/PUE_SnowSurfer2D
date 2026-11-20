using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] private float boostMultiplier = 30f; // Multiplier for the boost speed
    [SerializeField] private float baseSpeed = 15f; 
    
    [Header("Score Manager")]
    [SerializeField] private ScoreManager scoreManager; 
    
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;

    [SerializeField] private float jumpForce = 10f; // Force applied when the player jumps



    Rigidbody2D rb;
    SurfaceEffector2D surfaceEffector2D;

    InputAction moveAction;
    InputAction jumpAction;
    bool isGrounded;

    Vector2 moveValue ; 
    int flipCount;
    float totalRotation;
    float previousRotation;

    float maxTorque = 500f;

    private bool isPaused = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPaused = false;
        Time.timeScale = 1f; //0f detiene el juego y 1f lo reanuda
        
        //Active the selected character sprite
        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);

        //Activated the selected character sprite
        transform.GetChild(0).GetChild(selectedCharacter).gameObject.SetActive(true);

        // Get the Move action from the Input System
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");

        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        surfaceEffector2D = FindAnyObjectByType<SurfaceEffector2D>();           
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the Escape key is pressed
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToogglePause();
        }
        RotatePlayer();
        BoostPlayer();
        CalculateFlips();
        Jump();
    }

    void Jump()
    {
        if (isPaused || !isGrounded) return;

        bool jumpPressed = jumpAction != null && jumpAction.enabled
            ? jumpAction.WasPressedThisFrame()
            : Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (jumpPressed)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    private void ToogglePause()
    {
        isPaused = !isPaused; //Cambia el estado de pausa
        pauseMenuPanel.SetActive(isPaused); //Muestra u oculta el panel de pausa
        Time.timeScale = isPaused ? 0f : 1f; //Pausa o reanuda el juego
    }

    /// <summary>
    /// Calculates the number of flips the player has performed based on their rotation and updates the score accordingly.
    /// </summary>
    private void CalculateFlips()
    {
        float currentRotation = transform.rotation.eulerAngles.z; 
        totalRotation += Mathf.DeltaAngle(previousRotation, currentRotation); 
        if (Mathf.Abs(totalRotation) >= 340f ) 
        {
            flipCount++;
            totalRotation = 0f;            
            scoreManager.UpdateScore(flipCount*100); // Update the score based on flips
        }
        previousRotation = currentRotation;
    }

    private void BoostPlayer()
    {
        // if the player is moving  (up), increase the speed of the SurfaceEffector2D
        if (moveValue.y > 0)
        {
            surfaceEffector2D.speed = boostMultiplier; // Increase speed when moving forward
        }
        else
        {
            surfaceEffector2D.speed = baseSpeed; // Reset speed when not moving forward
        }
    }

private void RotatePlayer()
    {
        // Read the value of the Move action and store it in moveValue
        moveValue = moveAction.ReadValue<Vector2>();

        if (moveValue.x > 0)
        {
            rb.AddTorque(-2f); // Apply torque to rotate the player clockwise
            if (rb.angularVelocity < -maxTorque)
            {
                rb.angularVelocity = -maxTorque; // Limit the maximum clockwise rotation speed
            }
        }
        else if (moveValue.x < 0)
        {
            rb.AddTorque(2f); // Apply torque to rotate the player counterclockwise
            if (rb.angularVelocity > maxTorque)
            {
                rb.angularVelocity = maxTorque; // Limit the maximum counterclockwise rotation speed
            }
        }
    }

private void OnCollisionEnter2D(Collision2D collision)
{
    CheckGround(collision);
}

private void OnCollisionStay2D(Collision2D collision)
{
    CheckGround(collision);
}

private void OnCollisionExit2D(Collision2D collision)
{
    isGrounded = false;
}

private void CheckGround(Collision2D collision)
{
    foreach (ContactPoint2D contact in collision.contacts)
    {
        if (contact.normal.y > 0.5f)
        {
            isGrounded = true;
            return;
        }
    }
}

}
