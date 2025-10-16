using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float jumpForce = 10f;

    [Header("Air Dash")]
    [Tooltip("จำนวนครั้งที่ Dash ได้กลางอากาศต่อการลอย 1 ครั้ง")]
    public int maxAirDashes = 1;
    private int airDashesLeft;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("References")]
    public Animator animator;

    [Header("Animation Tuning")]
    [Tooltip("ต่ำกว่านี้จะถือว่าเริ่ม 'ตก' เพื่อเซ็ต isFalling")]
    public float fallingThreshold = -0.1f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isDashing = false;
    private float dashTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        airDashesLeft = maxAirDashes;
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // ตรวจจับพื้น
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // รีเซ็ตจำนวน Air Dash เมื่อแตะพื้น
        if (isGrounded) airDashesLeft = maxAirDashes;

        // ---------- Animator sync พื้นฐาน ----------
        float animSpeed = Mathf.Abs(moveInput);
        if (animSpeed > 0.5f) animSpeed = 0.5f;
        animator.SetFloat("Speed", animSpeed);
        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);

        // ---------- กด Dash (พื้นหรืออากาศ) ----------
        bool canGroundDash = isGrounded && moveInput != 0 && !isDashing;
        bool canAirDash    = !isGrounded && airDashesLeft > 0 && moveInput != 0 && !isDashing;

        if (Input.GetKeyDown(KeyCode.LeftShift) && (canGroundDash || canAirDash))
        {
            isDashing = true;
            dashTime  = Time.time + dashDuration;

            if (!isGrounded) airDashesLeft--;     // ใช้สิทธิ์ Air Dash
            animator.SetBool("IsDashing", true);  // ให้ Animator เข้าท่า Dash กลางอากาศได้
        }

        // ---------- ระหว่าง Dash ----------
        if (isDashing)
        {
            rb.velocity = new Vector2((facingRight ? 1 : -1) * dashSpeed, rb.velocity.y);

            // ถ้าอยากให้ Dash บนพื้นยังคงใช้ท่าใน Blend Tree ให้คง Speed=1
            if (isGrounded) animator.SetFloat("Speed", 1f);

            if (Time.time >= dashTime)
            {
                isDashing = false;
                animator.SetBool("IsDashing", false);
            }
            return; // ระหว่าง Dash ไม่ให้เดินปกติ
        }

        // ---------- เดินปกติ ----------
        rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);

        // พลิกตัวละครตามทิศ
        if (moveInput > 0 && !facingRight)      Flip();
        else if (moveInput < 0 && facingRight)  Flip();

        // ---------- กระโดด ----------
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // ---------- ตก ----------
        bool isFalling = rb.velocity.y < fallingThreshold && !isGrounded;
        animator.SetBool("isFalling", isFalling);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}
