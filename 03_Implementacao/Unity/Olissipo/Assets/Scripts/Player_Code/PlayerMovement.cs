/*
 * Este script controla o movimento do jogador, incluindo andar, saltar e voar.
 * Gere a detecção do chão /ground check) e a aplicação de arrasto (drag) dependendo do estado do jogador (no chão, no ar a voar).
 * Adiciona também efeitos sonoros de passos e limita a velocidade do jogador.
 */

using System;
using UnityEngine;

// #my_code
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool grounded;

    [Header("Flying")]
    public bool isFlying = false;
    public float flySpeed = 12f;
    public float flyDrag = 6f;
    public KeyCode ascendKey = KeyCode.Space;
    public KeyCode descendKey = KeyCode.LeftControl;

    [Header("References")]
    public Transform orientation;

    [Header("Footsteps")]
    public string footstepSoundName = "Footstep";
    public float footstepInterval = 0.4f;
    private float footstepTimer;

    private float horizontalInput;
    private float verticalInput;
    private float flyUpInput;

    private Vector3 moveDirection;
    private Rigidbody rb;

    private const float groundRayPadding = 0.2f;
    private const float groundForceMultiplier = 10f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
    }

    private void Update()
    {
        // Bloquear inputs quando o jogo estiver pausado
        if (PauseMenu.IsPaused)
        {
            horizontalInput = 0;
            verticalInput = 0;
            flyUpInput = 0;
            return;
        }

        MyInput();
        UpdateGroundedState();
        UpdateDrag();
        HandleFootsteps();
    }

    private void FixedUpdate()
    {
        if (PauseMenu.IsPaused) return;

        if (!isFlying)
            MovePlayer();
        else
            HandleFlyingMovement();

        SpeedControl();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        flyUpInput = 0f;

        // Quando saltar
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (isFlying)
        {
            if (Input.GetKey(ascendKey))
                flyUpInput = 1f;
            if (Input.GetKey(descendKey))
                flyUpInput = -1f;
        }
    }

    public void SetFlying(bool enabled)
    {
        isFlying = enabled;

        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.useGravity = !isFlying;

        if (isFlying)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        }

        UpdateGroundedState();
        UpdateDrag();
    }

    private RaycastHit groundHit;

    private void UpdateGroundedState()
    {
        if (isFlying)
        {
            grounded = false;
            return;
        }

        grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            out groundHit,
            playerHeight * 0.5f + groundRayPadding,
            whatIsGround
        );
    }

    private void UpdateDrag()
    {
        if (isFlying)
            rb.linearDamping = 0f;
        else
        {
            rb.linearDamping = grounded ? groundDrag : 0f;
        }
    }

    private void MovePlayer()
    {
        // Calcular a direção do movimento em relação à orientação do jogador
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
        // No ground
        if(grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * groundForceMultiplier, ForceMode.Force);
        }
        else if(!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * groundForceMultiplier * airMultiplier, ForceMode.Force);
        }
    }

    private void HandleFlyingMovement()
    {
        moveDirection = orientation.forward * verticalInput
                      + orientation.right * horizontalInput
                      + orientation.up * flyUpInput;

        if (moveDirection != Vector3.zero)
        {
            rb.linearVelocity = moveDirection.normalized * flySpeed;
        }
        else
        {
            // Para imediatamente quando não há input
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void SpeedControl()
    {
        if (!isFlying)
        {
            // Ground: limitar apenas a velocidade horizontal
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
        else
        {
            // Flying: limitar a velocidade geral (todos os eixos)
            if (rb.linearVelocity.magnitude > flySpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * flySpeed;
            }
        }
    }

    private void Jump()
    {
        // Reset velocidade y (veritcal)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void HandleFootsteps()
    {
        if (isFlying || !grounded || PauseMenu.IsPaused)
        {
            footstepTimer = 0f;
            return;
        }

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool hasInput = horizontalInput != 0f || verticalInput != 0f;
        bool isMoving = hasInput && flatVel.magnitude > 0.1f;

        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            // Footsteps default, substituídos por um componente FootstepSurface se presente
            string soundToPlay = footstepSoundName;
            var surface = groundHit.collider != null
                ? groundHit.collider.GetComponent<FootstepSurface>()
                : null;
            if (surface != null) soundToPlay = surface.soundName;

            SoundManager.Instance.PlaySound3D(soundToPlay, transform.position);
            footstepTimer = footstepInterval;
        }
    }
}