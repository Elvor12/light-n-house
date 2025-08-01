// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.SceneManagement;
using System.Data;
using static UnityEngine.GraphicsBuffer;





#if UNITY_EDITOR
using UnityEditor;
    using System.Net;
#endif

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;
    public int healthBar = 3;
    private bool gameOver = false;
    private bool lockedOnMonster = false;
    private Vector3 monsterPos;
    public MonsterLogic monsterLogic;
    private Inventory inventory;

    #region Camera Movement Variables

    private ScenesManager scenesManager;

    public Camera playerCamera;
    private CameraScript playerCameraScript;

    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    #region Camera Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;

    #endregion
    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables
    private bool isWalking = false;

    #region Sprint

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = 2f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    // Internal Variables
    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;

    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    // Internal Variables
    private bool isGrounded = false;

    #endregion

    #region Crouch

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    // Internal Variables
    private bool isCrouched = false;
    private Vector3 originalScale;

    public bool IsCRouched
    {
        get { return isCrouched; }
        private set { value = isCrouched; }
    }

    #endregion
    #endregion

    #region Head Bob

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    // Internal Variables
    private Vector3 jointOriginalPos;
    private float timer = 0;

    #endregion

    private void Awake()
    {
        scenesManager = FindAnyObjectByType<ScenesManager>();

        rb = GetComponent<Rigidbody>();

        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        playerCameraScript = FindAnyObjectByType<CameraScript>();
        originalScale = transform.localScale;
        jointOriginalPos = joint.localPosition;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }
    }

    void Start()
    {

        inventory = FindAnyObjectByType<Inventory>();

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        #region Sprint Bar

        sprintBarCG = GetComponentInChildren<CanvasGroup>();

        if (useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if (hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }

        #endregion
    }

    float camRotation;


    private Coroutine regenCoroutine;

    public void RegenerateSprint(float speed)
    {
        if (regenCoroutine != null) return;

        float restoreAmount = Mathf.Clamp(sprintDuration - sprintRemaining, 0, sprintDuration);
        regenCoroutine = StartCoroutine(SprintRegenRoutine(speed, restoreAmount));
    }

    private IEnumerator SprintRegenRoutine(float speed, float restoreAmount)
    {
        Debug.Log("Regen Speed");

        float restored = 0f;

        while (restored < restoreAmount)
        {
            float delta = Time.deltaTime * speed;
            restored += delta;
            sprintRemaining = Mathf.Clamp(sprintRemaining + delta, 0, sprintDuration);
            yield return null;
        }

        regenCoroutine = null;
    }



    private void Update()
    {
        #region Camera

        // Control camera movement
        if (lockedOnMonster)
        {
            Vector3 direction = (monsterPos - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime);
            if (Quaternion.Angle(transform.rotation, targetRotation) < 5f)
            {
                UnlockedMonster();
                monsterLogic.Teleport();
            }
        }
        else if (cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                // Inverted Y
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toogle zoom
            if (Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            {
                if (!isZoomed)
                {
                    isZoomed = true;
                }
                else
                {
                    isZoomed = false;
                }
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if (holdToZoom && !isSprinting)
            {
                if (Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if (Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Lerps camera.fieldOfView to allow for a smooth transistion
            if (isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if (!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion
        #endregion

        if (scenesManager.IsUsingFirstCamera() && !gameOver && !lockedOnMonster)
        {

            #region Sprint

            if (enableSprint)
            {
                if (isSprinting)
                {
                    isZoomed = false;
                    playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

                    // Drain sprint remaining while sprinting
                    if (!unlimitedSprint)
                    {
                        sprintRemaining -= Time.deltaTime;
                        if (sprintRemaining <= 0)
                        {
                            isSprinting = false;
                            isSprintCooldown = true;
                        }
                    }
                }

                // Handles sprint cooldown 
                // When sprint remaining == 0 stops sprint ability until hitting cooldown
                if (isSprintCooldown)
                {
                    sprintBar.color = Color.red;
                    sprintCooldown -= Time.deltaTime;
                    if (sprintCooldown <= 0)
                    {
                        isSprintCooldown = false;
                    }
                }
                else
                {
                    sprintCooldown = sprintCooldownReset;
                    sprintBar.color = Color.white;
                }

                // Handles sprintBar 
                if (useSprintBar && !unlimitedSprint)
                {
                    float sprintRemainingPercent = sprintRemaining / sprintDuration;
                    sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);
                }
            }

            #endregion

            #region Jump

            // Gets input and calls jump method
            if (enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
            {
                Jump();
            }

            #endregion

            #region Crouch

            if (enableCrouch)
            {
                if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
                {
                    Crouch();
                }

                if (Input.GetKeyDown(crouchKey) && holdToCrouch)
                {
                    Crouch();
                }
                else if (Input.GetKeyUp(crouchKey) && holdToCrouch)
                {
                    Crouch();
                }
            }

            #endregion

        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            if (scenesManager.IsUsingFirstCamera())
            {
                playerCameraScript.CheckForTrigger();
            }
            else
            {
                scenesManager.SwitchToFirst();
                Debug.Log("Switched to first camera");
            }
        }

        if (Input.GetKey(KeyCode.I))
        {
            foreach (Item item in inventory.items)
            {
                if (item.itemName == "Inhaler")
                {
                    GameObject target = GameObject.Find("Enemy"); ;
                    item.Use(target, inventory);
                    break;
                }
            }
        }


        CheckGround();

        if (enableHeadBob && !gameOver && !lockedOnMonster)
        {
            HeadBob();
        }
        if (healthBar == 0)
        {
            GameOver();
        }
    }

    void FixedUpdate()
    {
        if (scenesManager.IsUsingFirstCamera() && !gameOver && !lockedOnMonster)
        {
            #region Movement

            if (playerCanMove)
            {
                // Calculate how fast we should be moving
                Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

                // Checks if player is walking and isGrounded
                // Will allow head bob
                if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded)
                {
                    isWalking = true;
                }
                else
                {
                    isWalking = false;
                }

                // All movement calculations shile sprint is active
                if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
                {
                    isSprinting = true;

                    if (hideBarWhenFull && !unlimitedSprint)
                    {
                        sprintBarCG.alpha += 5 * Time.deltaTime;
                    }

                    MoveBySpeed(targetVelocity, sprintSpeed);
                }
                // All movement calculations while walking
                else
                {
                    isSprinting = false;

                    if (hideBarWhenFull && sprintRemaining == sprintDuration)
                    {
                        sprintBarCG.alpha -= 3 * Time.deltaTime;
                    }

                    MoveBySpeed(targetVelocity, walkSpeed);
                }
            }

            #endregion
        }
    }

    private void MoveBySpeed(Vector3 targetVelocity, float speed)
    {
        targetVelocity = transform.TransformDirection(targetVelocity) * speed;

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    // Sets isGrounded based on a raycast sent straigth down from the player object
    private void CheckGround()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (transform.localScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        // When crouched and using toggle system, will uncrouch for a jump
        if (isCrouched && !holdToCrouch)
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        // Stands player up to full height
        // Brings walkSpeed back up to original speed
        if (isCrouched)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed /= speedReduction;

            isCrouched = false;
        }
        // Crouches player down to set height
        // Reduces walkSpeed
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;

            isCrouched = true;
        }
    }

    private void HeadBob()
    {
        if (isWalking)
        {
            // Calculates HeadBob speed during sprint
            if (isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during crouched movement
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            // Resets when play stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }

    public void Heal()
    {
        healthBar++;
        Debug.Log(healthBar);
    }
    public void LockedOnMonster(Vector3 pos)
    {
        lockedOnMonster = true;
        monsterPos = pos;
    }
    private void UnlockedMonster()
    {
        lockedOnMonster = false;
        monsterPos = Vector3.zero;
    }

    public void GameOver()
    {
        gameOver = true;
        Debug.Log("Слил катку");
    }
}


