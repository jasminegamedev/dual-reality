using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
// This Component controls everything to do with player input, player movement, and other player related actions.
public class PlayerController : MonoBehaviour
{
    [Tooltip("How high the player can jump.")]
    public float JumpForce = 10;
    [Tooltip("How fast the player can move horizontally.")]
    public float MoveSpeed = 10;
    [Tooltip("How fast the player moves horizontally after a wall jump.")]
    public float WallJumpSpeed = 12;

    [Tooltip("Sound that plays when the player jumps.")]
    public AudioClip JumpSound;
    [Tooltip("Sound that plays when the player swaps realities.")]
    public AudioClip SwapSound;
    [Tooltip("Sound that plays when the player dies.")]
    public AudioClip DeadSound;

    [Tooltip("Animator controller to play when player is in the black reality.")]
    public AnimatorOverrideController BlackAnimation;
    [Tooltip("Animator controller to play when player is in the white reality.")]
    public AnimatorOverrideController WhiteAnimation;

    // Other GameObject References.
    [Tooltip("Reference to the Pause Menu GameObject.")]
    public GameObject PauseMenu;
    [Tooltip("Reference to the Main Camera GameObject.")]
    public Camera Cam;

    // Cached components
    private Rigidbody2D rbody;
    private BoxCollider2D bCollider;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Animator animator;

    // Contact filters for collision checks
    private ContactFilter2D blackFilter;
    private ContactFilter2D whiteFilter;
    private ContactFilter2D blackgrayFilter;
    private ContactFilter2D whitegrayFilter;

    // Whether the player is currently able to jump.
    private bool canJump = true;
    // Whether the player is currently able to wall jump.
    private bool canWallJump;
    // If the player has shifted to the black color.
    private bool isBlack;
    // If the player has died.
    private bool isDead;

    // Values to accelerate/decelerate between previous speed and desired speed.
    private float oldVelX;
    private float targetVelX;
    private float velXDelta;

    // Value representing if there is currently a wall to the left of the player.
    private bool isWallLeft;
    // Value representing if there is currently a wall to the right of the player.
    private bool isWallRight;
    // Value representing if the player is standing on a floor.
    private bool isFloorDown;

    // Cache for the previous horizontal axis value, so we can know when that changes.
    private float prevHorizontal;

    void Start()
    {
        SetupContactFilters();
        SetupComponentCache();

        audioSource.volume = PlayerPrefs.GetInt("SoundVolume", 15) / 20.0f;
    }

    // Setup contact filters for collision checks.
    private void SetupContactFilters()
    {
        blackFilter.layerMask = LayerMask.GetMask("blacklayer");
        blackFilter.useLayerMask = true;

        whiteFilter.layerMask = LayerMask.GetMask("whitelayer");
        whiteFilter.useLayerMask = true;

        blackgrayFilter.layerMask = LayerMask.GetMask("blacklayer", "graylayer");
        blackgrayFilter.useLayerMask = true;

        whitegrayFilter.layerMask = LayerMask.GetMask("whitelayer", "graylayer");
        whitegrayFilter.useLayerMask = true;
    }

    // Cache common components
    private void SetupComponentCache()
    {
        rbody = GetComponent<Rigidbody2D>();
        bCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is split into a few sub-functions to keep things organized
    void Update()
    {
        if (isDead)
        {
            return;
        }

        if (UpdatePause()) return;
        UpdateColorSwap();
        UpdateRaycasts();
        UpdateJump();
        UpdateMovement();
        UpdateVisuals();
    }

    // Checks whether the pause button was pressed. If so, open the pause menu, and freeze player.
    private bool UpdatePause()
    {
        bool holdingLeft = Input.GetAxis("Horizontal") <= -0.2f;
        bool holdingRight = Input.GetAxis("Horizontal") >= 0.2f;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenu.SetActive(!PauseMenu.activeSelf);
            PauseMenu.GetComponentInChildren<MenuController>().ResetState();
            rbody.simulated = !PauseMenu.activeSelf;

            if (!PauseMenu.activeSelf)
            {
                if (holdingLeft)
                {
                    oldVelX = rbody.velocity.x;
                    targetVelX = -MoveSpeed;
                    velXDelta = 0;
                }
                else if (holdingRight)
                {
                    oldVelX = rbody.velocity.x;
                    targetVelX = MoveSpeed;
                    velXDelta = 0;
                }
                else
                {
                    oldVelX = rbody.velocity.x;
                    velXDelta = 0;
                    targetVelX = 0;
                }
            }
        }

        return PauseMenu.activeSelf;
    }

    // If player pressed swap button, swap physics layers and visuals.
    private void UpdateColorSwap()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            audioSource.PlayOneShot(SwapSound);
            isBlack = !isBlack;

            if (isBlack)
            {
                gameObject.layer = LayerMask.NameToLayer("blacklayer");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("whitelayer");
            }
            List<Collider2D> results = new List<Collider2D>();
            if (bCollider.OverlapCollider(isBlack ? blackFilter : whiteFilter, results) > 0)
            {
                Die();
            }

            if (isBlack)
            {
                animator.runtimeAnimatorController = BlackAnimation;
                Cam.backgroundColor = new Color(1, 1, 1, 1);
            }
            else
            {
                animator.runtimeAnimatorController = WhiteAnimation;
                Cam.backgroundColor = new Color(0, 0, 0, 1);
            }
        }
    }

    // Update shared properties that require raycasts against walls and floors.
    private void UpdateRaycasts()
    {
        List<RaycastHit2D> resultsA = new List<RaycastHit2D>();

        isWallLeft = Physics2D.Raycast(transform.position + new Vector3(-0.45f, 0, 0), Vector2.left, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1 ||
            Physics2D.Raycast(transform.position + new Vector3(-0.45f, 0.45f, 0), Vector2.left, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1 ||
            Physics2D.Raycast(transform.position + new Vector3(-0.45f, -0.45f, 0), Vector2.left, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1;
        isWallRight = Physics2D.Raycast(transform.position + new Vector3(0.45f, 0, 0), Vector2.right, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1 ||
            Physics2D.Raycast(transform.position + new Vector3(0.45f, 0.45f, 0), Vector2.right, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1 ||
            Physics2D.Raycast(transform.position + new Vector3(0.45f, -0.45f, 0), Vector2.right, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1;
        isFloorDown = Physics2D.Raycast(transform.position + new Vector3(0, -0.45f, 0), Vector2.down, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1 ||
            Physics2D.Raycast(transform.position + new Vector3(-0.45f, -0.45f, 0), Vector2.down, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1 ||
            Physics2D.Raycast(transform.position + new Vector3(0.45f, -0.45f, 0), Vector2.down, isBlack ? blackgrayFilter : whitegrayFilter, resultsA, 0.1f) > 1;
    }

    // Handle player jumps and wall jumps.
    private void UpdateJump()
    {
        if (!canJump && rbody.velocity.y <= 0)
        {
            if (isFloorDown)
            {
                canJump = true;
            }
        }
        if (Input.GetButtonDown("Jump"))
        {
            if (canJump)
            {
                rbody.velocity = new Vector2(rbody.velocity.x, JumpForce);
                audioSource.PlayOneShot(JumpSound);
                canJump = false;
            }
            else
            {
                if (isWallRight)
                {
                    rbody.velocity = new Vector2(-WallJumpSpeed, JumpForce);
                    oldVelX = -WallJumpSpeed;
                    velXDelta = 0;
                    audioSource.PlayOneShot(JumpSound);
                }
                else if (isWallLeft)
                {
                    rbody.velocity = new Vector2(WallJumpSpeed, JumpForce);
                    oldVelX = WallJumpSpeed;
                    velXDelta = 0;
                    audioSource.PlayOneShot(JumpSound);
                }
            }
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (rbody.velocity.y > 0)
            {
                rbody.velocity = new Vector2(rbody.velocity.x, 0);
            }
        }
    }

    // Handles updating player movement variables.
    private void UpdateMovement()
    {
        bool pressedLeft = (prevHorizontal > -0.2f && Input.GetAxis("Horizontal") <= -0.2f);
        bool pressedRight = (prevHorizontal < 0.2f && Input.GetAxis("Horizontal") >= 0.2f);

        bool releasedLeft = (prevHorizontal <= -0.2f && Input.GetAxis("Horizontal") > -0.2f);
        bool releasedRight = (prevHorizontal >= 0.2f && Input.GetAxis("Horizontal") < 0.2f);

        bool holdingLeft = Input.GetAxis("Horizontal") <= -0.2f;
        bool holdingRight = Input.GetAxis("Horizontal") >= 0.2f;

        if (pressedRight)
        {
            oldVelX = rbody.velocity.x;
            targetVelX = MoveSpeed;
            velXDelta = 0;
        }
        else if (pressedLeft)
        {
            oldVelX = rbody.velocity.x;
            targetVelX = -MoveSpeed;
            velXDelta = 0;
        }

        if (releasedRight)
        {
            oldVelX = rbody.velocity.x;
            velXDelta = 0;

            if (holdingLeft)
            {
                targetVelX = -MoveSpeed;
            }
            else
            {
                targetVelX = 0;
            }
        }
        else if (releasedLeft)
        {
            oldVelX = rbody.velocity.x;
            velXDelta = 0;

            if (holdingRight)
            {
                targetVelX = MoveSpeed;
            }
            else
            {
                targetVelX = 0;
            }
        }

        prevHorizontal = Input.GetAxis("Horizontal");
    }

    // Handles updating player visuals, mainly animations, and sprite settings.
    private void UpdateVisuals()
    {
        bool holdingLeft = Input.GetAxis("Horizontal") <= -0.2f;
        bool holdingRight = Input.GetAxis("Horizontal") >= 0.2f;

        if (holdingLeft && isWallLeft)
        {
            animator.SetBool("IsSquished", true);
        }
        else if (holdingRight && isWallRight)
        {
            animator.SetBool("IsSquished", true);
        }
        else
        {
            animator.SetBool("IsSquished", false);
        }

        rbody.velocity = new Vector2(Mathf.Lerp(oldVelX, targetVelX, velXDelta), rbody.velocity.y);
        velXDelta += Time.deltaTime * 3;

        if (rbody.velocity.x > 0.0f)
        {
            spriteRenderer.flipX = false;
        }
        else if (rbody.velocity.x < 0.0f)
        {
            spriteRenderer.flipX = true;
        }

        animator.SetBool("IsWalking", Mathf.Abs(rbody.velocity.x) >= 0.2f);
        animator.SetBool("IsInAir", !isFloorDown);
        animator.SetBool("IsSquating", isFloorDown && Mathf.Abs(rbody.velocity.x) < 0.2f && (Input.GetKey(KeyCode.DownArrow) || Input.GetAxis("Vertical") <= -0.2f));
    }

    // Kill The player, and freeze all actions that shouldn't be running during death sequence.
    // Starts Timer for level reset.
    public void Die()
    {
        isDead = true;
        audioSource.PlayOneShot(DeadSound);
        rbody.simulated = false;
        StartCoroutine(nameof(DeathDelay));
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsInAir", true);
        animator.SetBool("IsSquating", false);
    }

    // After death, wait for a while and then load the next scene.
    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(0.85f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    //If player is inside of a wall, kill them.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector3 closest = collision.collider.ClosestPoint(transform.position);
        if (closest == transform.position)
        {
            Die();
        }
    }
}
