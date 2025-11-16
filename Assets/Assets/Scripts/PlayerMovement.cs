using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Detección Suelo")]
    public LayerMask groundMask;
    public float groundCheckDistance = 1.1f;

    [Header("Movimiento Avanzado")]
    public float sideMultiplier = 0.75f;   // 75% lateral
    public float backMultiplier = 0.5f;    // 50% atrás
    public float airMultiplier = 0.5f;     // 50% en el aire
    public float sprintMultiplier = 2f;    // 200% sprint

    [Header("Jetpack")]
    public float jetpackForce = 10f;
    [Range(0f, 1f)]
    public float fuel = 1f;             // 0-1
    public float fuelBurnTime = 1f;     // se gasta en 1s
    public float fuelRechargeTime = 0.5f; // se recarga en 0.5s
    float emptyGroundTime = 0f;
    bool fuelWasEmpty = false;
    public Image jetpackUI;             // medidor radial

    [Header("Animaciones (opcional)")]
    public Animator animator;

    Rigidbody rb;
    bool isGrounded;
    public bool IsGrounded => isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        GroundCheck();
        HandleJump();
        HandleJetpack();
        UpdateJetpackUI();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

 void GroundCheck()
{
    // Consideramos que el suelo está en Y = 0
    isGrounded = transform.position.y <= 1.01f;
}



    void HandleMovement()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = transform.forward * v + transform.right * h;

        if (moveDir.sqrMagnitude < 0.0001f)
            return;

        moveDir.Normalize();

        float finalSpeed = moveSpeed;

        // Movimiento avanzado
        if (v < 0)
        {
            finalSpeed *= backMultiplier; // hacia atrás
        }
        else if (Mathf.Abs(h) > 0 && Mathf.Approximately(v, 0f))
        {
            finalSpeed *= sideMultiplier; // solo lateral
        }

        // Sprint solo en suelo y hacia delante
        bool movingForward = v > 0f;
        bool canSprint = isGrounded && movingForward;

        if (canSprint && Input.GetKey(KeyCode.LeftShift))
        {
            finalSpeed *= sprintMultiplier;
        }

        // En el aire: siempre 50%
        if (!isGrounded)
        {
            finalSpeed *= airMultiplier;
        }

        Vector3 targetPosition = rb.position + moveDir * finalSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    void HandleJump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void HandleJetpack()
    {
        if (isGrounded)
        {
            // Si llegó a 0, esperar 0.5s antes de recargar
            if (fuel <= 0f)
            {
                fuelWasEmpty = true;
                emptyGroundTime += Time.deltaTime;

                if (emptyGroundTime >= 0.5f)
                {
                    fuelWasEmpty = false;
                    emptyGroundTime = 0f;
                }
            }

            if (!fuelWasEmpty && fuel < 1f)
            {
                fuel += Time.deltaTime / fuelRechargeTime;
                fuel = Mathf.Clamp01(fuel);
            }
        }
        else
        {
            // En el aire: jetpack si mantiene espacio y hay fuel
            if (Input.GetKey(KeyCode.Space) && fuel > 0f)
            {
                rb.AddForce(Vector3.up * jetpackForce, ForceMode.Acceleration);

                fuel -= Time.deltaTime / fuelBurnTime;
                fuel = Mathf.Clamp01(fuel);
            }
        }
    }

    void UpdateJetpackUI()
    {
        if (jetpackUI != null)
        {
            jetpackUI.fillAmount = fuel;
        }
    }

  void UpdateAnimator()
{
    if (animator == null) return;

    // Velocidad horizontal del rigidbody
    Vector3 horizontalVel = rb.linearVelocity;
    horizontalVel.y = 0f;

    float speed = horizontalVel.magnitude;

    // Dirección local de movimiento para el Blend Tree 2D
    Vector3 localVel = transform.InverseTransformDirection(horizontalVel);
    float horizontal = localVel.x; // izquierda/derecha
    float vertical = localVel.z;   // adelante/atrás

    animator.SetFloat("Speed", speed);         
    animator.SetFloat("Horizontal", horizontal);
    animator.SetFloat("Vertical", vertical);
    animator.SetBool("IsGrounded", isGrounded);
}

}
