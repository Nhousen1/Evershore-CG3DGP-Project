using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointAim : MonoBehaviour
{
    public Transform target;

    [Header("Layer Filtering")]
    [SerializeField] private LayerMask aimLayers = 0; // default: everything

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Confined;
    }
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        // Get a ray from the camera through the mouse cursor
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        // Cast the ray into the scene
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, aimLayers))
        {
            Vector3 hitPoint = hitInfo.point;
            Vector3 dirVector = (hitPoint - target.transform.position).normalized;
            Vector3 aimDir = new Vector3(dirVector.x, 0, dirVector.z);
            Debug.DrawLine(transform.position, hitPoint, Color.red);

            transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);
        }
    }
}
