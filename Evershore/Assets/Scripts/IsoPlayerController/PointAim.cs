using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/* Author: Marcus King
 * Date created: 10/1/2025
 * Date last updated: 10/6/2025
 * Summary: will point the player in the direction of the cursor when the player is aiming. Enable/disable this script as necessary.
 */
public class PointAim : MonoBehaviour
{
    public Transform target;

    [Header("Layer Filtering")]
    //Which layers should the cursor raycast collide with
    [SerializeField] private LayerMask aimLayers = 0;

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
        //Uses hit point so this system works with both perspective and orthographic projection
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

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
