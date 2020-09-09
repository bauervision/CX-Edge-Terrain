using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraFly : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Multiplier for camera sensitivity.")]
    [Range(0f, 300)]
    public float sensitivity = 90f;
    [Tooltip("Multiplier for camera movement upwards.")]
    [Range(0f, 10f)]
    public float climbSpeed = 4f;
    [Tooltip("Multiplier for normal camera movement.")]
    [Range(0f, 20f)]
    public float normalMoveSpeed = 10f;
    [Tooltip("Multiplier for slower camera movement.")]
    [Range(0f, 5f)]
    public float slowMoveSpeed = 0.25f;
    [Tooltip("Multiplier for faster camera movement.")]
    [Range(0f, 40f)]
    public float fastMoveSpeed = 3f;
    [Tooltip("Rotation limits for the X-axis in degrees. X represents the lowest and Y the highest value.")]
    public Vector2 rotationLimitsX;
    [Tooltip("Rotation limits for the X-axis in degrees. X represents the lowest and Y the highest value.")]
    public Vector2 rotationLimitsY;
    [Tooltip("Whether the rotation on the X-axis should be limited.")]
    public bool limitXRotation = false;
    [Tooltip("Whether the rotation on the Y-axis should be limited.")]
    public bool limitYRotation = false;

    public static bool isActive = false;
    private Vector3 cameraRotation;
    public bool cameraLocked = false;

    [SerializeField]
    private KeyCode ToggleCameraMode = KeyCode.Return;

    private Material shader;

    private GameObject activeCameraPanel;
    private GameObject enterKeyPanel;
    public Vector3 loadedVector3;

    private bool cameraRotLoaded = false;

    private void Awake()
    {
        activeCameraPanel = GameObject.Find("CameraActive");
        enterKeyPanel = GameObject.Find("EnterKey");
    }

    private void Start()
    {
        // start camera with this default rotation value
        cameraRotation = new Vector3(-180, 45);
        activeCameraPanel.SetActive(true);
        enterKeyPanel.SetActive(false);

    }

    public void SetCameraVector(Vector3 setVector)
    {
        loadedVector3 = setVector;
        print("setVector" + loadedVector3);
    }

    void HandleCameraUI()
    {
        activeCameraPanel.SetActive(!cameraLocked);
        enterKeyPanel.SetActive(cameraLocked);
        // if camera can fly, hide all the panels
        if (!cameraLocked)
            UIManager.instance.HideAllPanels();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(ToggleCameraMode))
        {
            cameraLocked = !cameraLocked;
            HandleCameraUI();
        }



        if ((isActive && loadedVector3.x != 0) && !cameraRotLoaded)
        {
            cameraRotation = loadedVector3;
            cameraRotLoaded = true;
        }

    }

    // LateUpdate is called every frame, if the Behaviour is enabled
    private void LateUpdate()
    {

        if (cameraRotLoaded)
        {
            if (!cameraLocked)
            {

                cameraRotation.x += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
                cameraRotation.y += Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

                transform.localRotation = Quaternion.AngleAxis(cameraRotation.x, Vector3.up);
                transform.localRotation *= Quaternion.AngleAxis(cameraRotation.y, Vector3.left);

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    transform.position += transform.right * (normalMoveSpeed * fastMoveSpeed) * Input.GetAxis("Horizontal") * Time.deltaTime;
                    transform.position += transform.forward * (normalMoveSpeed * fastMoveSpeed) * Input.GetAxis("Vertical") * Time.deltaTime;
                }
                else if (Input.GetKey(KeyCode.LeftControl))
                {
                    transform.position += transform.right * (normalMoveSpeed * slowMoveSpeed) * Input.GetAxis("Horizontal") * Time.deltaTime;
                    transform.position += transform.forward * (normalMoveSpeed * slowMoveSpeed) * Input.GetAxis("Vertical") * Time.deltaTime;
                }
                else
                {
                    transform.position += transform.right * normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime;
                    transform.position += transform.forward * normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
                }

                if (Input.GetKeyUp(KeyCode.A))
                {
                    transform.position += transform.up * climbSpeed * Time.deltaTime;
                }

                if (Input.GetKeyUp(KeyCode.Z))
                {
                    transform.position -= transform.up * climbSpeed * Time.deltaTime;
                }

                if (Input.GetKeyUp(KeyCode.Q))
                {
                    transform.localPosition += transform.up * climbSpeed * Time.deltaTime;
                }
            }
        }
    }
}