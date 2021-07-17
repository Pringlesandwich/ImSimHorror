using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] float sensX = 75f;
    [SerializeField] float sensY = 75f;
    [SerializeField] float InteractionLength = 100.0f;

    Camera cam;

    float mouseX;
    float mouseY;

    float multiplier = 0.01f;

    float rotationX;
    float rotationY;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();

        //TEMP
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log(this.transform.rotation.y);

        rotationY = this.transform.rotation.y;
    }

    private void Update()
    {
        MyInput();

        cam.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    private void MyInput()
    {
        mouseX = Input.GetAxisRaw("Mouse X");
        mouseY = Input.GetAxisRaw("Mouse Y");

        rotationY += mouseX * sensX * multiplier;
        rotationX -= mouseY * sensY * multiplier;

        rotationX = Mathf.Clamp(rotationX, -85, 85);

        if (Input.GetButtonDown("Fire1"))
        {
            Interact();
        }
    }

    private void Interact()
    {
        RaycastHit hit;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, InteractionLength))
        {
            //var npc = hit.collider.GetComponent<NPC>();
            //if (npc != null)
            //{
            //    npc.Interact();
            //}
        }
    }
}
