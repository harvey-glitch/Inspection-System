using System.Collections;
using UnityEngine;
using TMPro;

public class InspectionSystem : MonoBehaviour
{
    [Header("Inspection Settings")]
    [Tooltip(" The layer that defines what can be inspected")]
    public LayerMask TargetLayer;

    [Tooltip("Speed at which the object can be rotate when inspecting")]
    public float RotateSpeed;

    [Tooltip("Speed at which the object can be zoom in and out when inspecting")]
    public float ZoomSpeed;

    [Tooltip("Maximum distance the object can move when zooming")]
    public float ZoomDistance = 10f;

    [Tooltip("Transform reference where the object will be inspected from")]
    public Transform InspectPoint;

    [SerializeField, Tooltip("Flag to check if the player is inspecting an object")]
    private bool IsInspecting;

    [Header("Transition Settings")]
    [Tooltip("Animation curve for smoothing position overtime")]
    public AnimationCurve PositionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

    [Tooltip("Animation curve for smoothing rotation overtime")]
    public AnimationCurve RotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

    [Tooltip("Determine the speed of animation")]
    public float TransitionTime;

    [Tooltip("Flag to check whether the transition is done or not")]
    public bool IsTransitioning;

    [Header("Interface Settings")]
    [Tooltip("Text to display when looking at inspectable objects")]
    public TextMeshProUGUI PromptText;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public GameObject InspectionPanel;

    // reference to the object that currently inspecting
    private GameObject _inspectObject;

    // original transform reference of inspect object
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    // reference to the main camera
    private Camera _camera;

    private FPSController _fpsController;

    // initial position of the inspection point
    private Vector3 _originalInspectPos;

    // reference to the object hit by ray
    private RaycastHit _rayHit;

    // store the last known state of bool variable isInspecting
    private bool _LastInspectState;

    // store the item data in the inspect object
    private ItemData _itemData;

    private void Awake()
    {
        _fpsController ??= FindFirstObjectByType<FPSController>();
        _camera = Camera.main;
    }

    private void Start()
    {
        _originalInspectPos = InspectPoint.transform.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SelectObject();
        }

        OnInspectStay(); // handle inspect logic (rotation, zooming, exiting)
        HandleInspectUI(); // display / hide inspect interfaces
    }

    private void SelectObject()
    {
        // only allow selecting object if any transitioning is completed and not in inspect mode
        if (RayHitSomething() && !IsTransitioning && !IsInspecting)
        {
            OnInspectEnter();
        }
    }

    private void HandleInspectUI()
    {
        // clear the prompt tex in the first frame
        PromptText.text = string.Empty;

        if (RayHitSomething() && !IsTransitioning && !IsInspecting)
        {
            PromptText.text = "[E]" + "\n" + _rayHit.transform.name;
        }

        if (IsInspecting != _LastInspectState)
        {
            InspectionPanel.SetActive(IsInspecting);
            _LastInspectState = IsInspecting; // update the last known inspect state

            // update the texts based on item data of inspect object
            NameText.text = _itemData.name;
            DescriptionText.text = _itemData.Description;

            Debug.Log("UI Updated");
        }
    }

    private void OnInspectEnter()
    {
        // save the object and its original transforms
        _inspectObject = _rayHit.transform.gameObject;
        _originalPosition = _inspectObject.transform.position;
        _originalRotation = _inspectObject.transform.rotation;

        // attached the object to the inspect point
        _inspectObject.transform.SetParent(InspectPoint);

        // disable the fps controller to avoid rotation conflict
        _fpsController.enabled = false;

        // try to get the item data for display item name and description
        if (_inspectObject.TryGetComponent(out Items items))
        {
            _itemData = items.GetItemData();
        }

        // start moving the object on the desired transform
        StartCoroutine(MoveInspectObject(
            InspectPoint.position, Quaternion.identity, true));
    }

    private void OnInspectStay()
    {
        // only allow interaction if not currently transitioning and in inspect mode
        if (!IsTransitioning && IsInspecting)
        {
            HandleRotation();
            HandleZooming();

            // right-click to exit inspect mode
            if (Input.GetMouseButtonDown(1))
            {
                OnInspectExit();
            }
        }
    }

    private void OnInspectExit()
    {
        // detach the inspect object from the  inspect point
        _inspectObject.transform.SetParent(null);

        // remove unnecessary rotation
        InspectPoint.transform.localEulerAngles = Vector3.zero;

        // re-enable the fps controller
        _fpsController.enabled = true;

        IsInspecting = false; // mark as not inspecting

        // move the object back to its original transform
        StartCoroutine(MoveInspectObject(
            _originalPosition, _originalRotation, false));
    }

    private IEnumerator MoveInspectObject(Vector3 position, Quaternion rotation, bool inspecting)
    {
        IsTransitioning = true; // mark as transitioning

        float percentage = 0f; // time to track the animation progress

        while (percentage < TransitionTime)
        {
            percentage += Time.deltaTime; // advance the time

            // normalize time to a 0–1 range for curve evaluation
            float progress = Mathf.Clamp01(percentage / TransitionTime) / TransitionTime;

            // smoothly interpolate the object's position and rotation using the evaluated curve
            _inspectObject.transform.position = Vector3.Lerp(
                _inspectObject.transform.position, position, PositionCurve.Evaluate(progress));

            _inspectObject.transform.rotation = Quaternion.Lerp(
                _inspectObject.transform.rotation, rotation, RotationCurve.Evaluate(progress));

            yield return null;
        }

        IsTransitioning = false; // mark transition as complete

        IsInspecting = inspecting;
    }

    private void HandleRotation()
    {
        Vector2 mouseInput = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y") * RotateSpeed * Time.deltaTime);

        // The direction from the camera to the inspect point
        Vector3 viewDirection = InspectPoint.position - _camera.transform.position;

        // Right axis relative to the view, used for vertical rotation
        Vector3 viewRight = Vector3.Cross(_camera.transform.up, viewDirection);

        // Up axis relative to the view, used for horizontal  rotation
        Vector3 viewUp = Vector3.Cross(viewDirection, viewRight);

        // Create a horizontal rotation around the view's up axis
        Quaternion xRotation = Quaternion.AngleAxis(-mouseInput.x, viewUp);

        // Create a vertical rotation around the view's right axis
        Quaternion yRotation = Quaternion.AngleAxis(mouseInput.y, viewRight);

        // Apply both rotations combined
        InspectPoint.rotation = xRotation * yRotation * InspectPoint.rotation;
    }

    private void HandleZooming()
    {
        // get scroll input and scale it
        float scrollInput = Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;

        if (scrollInput != 0)
        {
            // apply movement along camera's forward direction based on scroll input
            InspectPoint.position += _camera.transform.forward * scrollInput;

            // calculate the distance from the original to current inspect position
            Vector3 distance = InspectPoint.position - _originalInspectPos;

            // clamp the distance from the initial position
            InspectPoint.position = _originalInspectPos + Vector3.ClampMagnitude(distance, ZoomDistance);
        }
    }

    private bool RayHitSomething()
    {
        return Physics.Raycast(transform.position, transform.forward, out _rayHit, 3f, TargetLayer);
    }
}
