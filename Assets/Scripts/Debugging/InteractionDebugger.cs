using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Diagnostic tool to debug interaction issues.
/// Add this to your Player to see what's happening.
/// </summary>
public class InteractionDebugger : MonoBehaviour
{
    [Header("Settings")]
    public float rayDistance = 4f;
    public bool showVisualDebug = true;

    private Camera cam;
    private InputAction diagnosticAction;

    void Awake()
    {
        // Setup Input Action for new Input System
        diagnosticAction = new InputAction("Diagnostic", InputActionType.Button);
        diagnosticAction.AddBinding("<Keyboard>/t");
        diagnosticAction.performed += ctx => RunDiagnostic();
    }

    void OnEnable() => diagnosticAction.Enable();
    void OnDisable() => diagnosticAction.Disable();

    void Start()
    {
        cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("❌ NO MAIN CAMERA FOUND!");
        }
        else
        {
            Debug.Log($"✅ Found camera: {cam.name} at position {cam.transform.position}");
        }
    }

    void Update()
    {
        if (cam == null)
        {
            cam = Camera.main;
            return;
        }

        // Visual debug rays
        if (showVisualDebug)
        {
            DrawDebugRays();
        }
    }

    void DrawDebugRays()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        // Draw the ray
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 0f, false);

        // Cast and show hit
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 0f, false);
        }
    }

    void RunDiagnostic()
    {
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("🔍 INTERACTION SYSTEM DIAGNOSTIC");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        // Check 1: Camera
        if (cam == null)
        {
            Debug.LogError("❌ FAIL: No Main Camera found!");
            Debug.Log("FIX: Tag a camera as 'MainCamera'");
            return;
        }
        else
        {
            Debug.Log($"✅ Camera: {cam.name}");
            Debug.Log($"   Position: {cam.transform.position}");
            Debug.Log($"   Forward: {cam.transform.forward}");
        }

        // Check 2: Player components
        PlayerInteract interact = GetComponent<PlayerInteract>();
        if (interact == null)
        {
            Debug.LogError("❌ FAIL: No PlayerInteract component on player!");
            Debug.Log("FIX: Add PlayerInteract to your player GameObject");
        }
        else
        {
            Debug.Log($"✅ PlayerInteract found");
            Debug.Log($"   Prompt Canvas: {(interact.promptCanvasGroup != null ? "✓" : "✗")}");
            Debug.Log($"   Prompt Text: {(interact.promptText != null ? "✓" : "✗")}");
        }

        // Check 3: Raycast test
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        Debug.Log($"📍 Raycast Info:");
        Debug.Log($"   Origin: {ray.origin}");
        Debug.Log($"   Direction: {ray.direction}");
        Debug.Log($"   Distance: {rayDistance}m");

        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, rayDistance);

        if (hit)
        {
            Debug.Log($"🎯 RAYCAST HIT!");
            Debug.Log($"   Object: {hitInfo.collider.name}");
            Debug.Log($"   Tag: {hitInfo.collider.tag}");
            Debug.Log($"   Layer: {LayerMask.LayerToName(hitInfo.collider.gameObject.layer)}");
            Debug.Log($"   Distance: {hitInfo.distance:F2}m");
            Debug.Log($"   Has IInteractable: {(hitInfo.collider.GetComponent<IInteractable>() != null ? "YES" : "NO")}");

            // Check tag
            if (hitInfo.collider.CompareTag("Interactable"))
            {
                Debug.Log("   ✅ Tagged 'Interactable'");
            }
            else
            {
                Debug.LogWarning($"   ⚠️ NOT tagged 'Interactable' (has '{hitInfo.collider.tag}')");
            }

            // Check for component
            IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                Debug.Log($"   ✅ Has IInteractable component");
                Debug.Log($"   Prompt would be: '{interactable.GetPromptText()}'");
            }
            else
            {
                Debug.LogWarning("   ⚠️ No IInteractable component found!");
                Debug.Log("   FIX: Add Card3D, InteractableDoor, or InteractableItem component");
            }
        }
        else
        {
            Debug.LogWarning("❌ RAYCAST MISSED - Nothing in front of camera!");
            Debug.Log("   Things to check:");
            Debug.Log("   1. Is an object within 4m of camera?");
            Debug.Log("   2. Does it have a collider?");
            Debug.Log("   3. Is the collider on the correct layer?");
        }

        // Check 4: Find all interactables in scene
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        GameObject[] interactables = GameObject.FindGameObjectsWithTag("Interactable");
        Debug.Log($"📋 Found {interactables.Length} objects tagged 'Interactable':");

        foreach (GameObject obj in interactables)
        {
            bool hasComponent = obj.GetComponent<IInteractable>() != null;
            bool hasCollider = obj.GetComponent<Collider>() != null;
            Vector3 pos = obj.transform.position;
            float dist = Vector3.Distance(cam.transform.position, pos);

            Debug.Log($"   • {obj.name}");
            Debug.Log($"     Position: {pos} (Distance: {dist:F2}m)");
            Debug.Log($"     IInteractable: {(hasComponent ? "✓" : "✗")}");
            Debug.Log($"     Collider: {(hasCollider ? "✓" : "✗")}");
        }

        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log("Press T again to re-run diagnostic");
        Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
    }

    void OnDrawGizmos()
    {
        if (!showVisualDebug || cam == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);

        // Draw ray in Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * rayDistance);

        // Draw hit point
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.point, 0.1f);
            Gizmos.DrawLine(ray.origin, hit.point);
        }
    }
}