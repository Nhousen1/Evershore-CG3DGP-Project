using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/* Author: Marcus King
 * Date created: 10/1/2025
 * Date last updated: 10/6/2025
 * Summary: will point the player in the direction of the cursor when the player is aiming. Enable/disable this script as necessary.
 */
public class PointAim : MonoBehaviour
{
    public Transform target;

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
        Vector2 rawPointPostion = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 rawCenter = new Vector2(Screen.width/2, Screen.height/2);
        Vector2 pointFromCenter = rawPointPostion - rawCenter;
        pointFromCenter = pointFromCenter.normalized;
        
        Vector3 camSpacePointVector = (Camera.main.transform.right * pointFromCenter.x + Camera.main.transform.up * pointFromCenter.y).normalized;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 aimingPlane = Vector3.up;

        float denominator = Vector3.Dot(aimingPlane, camForward);
        if(Mathf.Abs(denominator) <= 0.0001)
        {
            return;
            //Skip aiming because the player has cursor on character and is not aiming anywhere.
        }

        float forwardAmmount = -1f * (Vector3.Dot(aimingPlane, camSpacePointVector)) / denominator;
        Vector3 lookDir = (camSpacePointVector + camForward * forwardAmmount).normalized;

        target.transform.rotation = Quaternion.LookRotation(lookDir);

        Debug.Log(lookDir);
        Debug.DrawLine(target.transform.position, target.transform.position + lookDir * 5, Color.red);
    }
}
