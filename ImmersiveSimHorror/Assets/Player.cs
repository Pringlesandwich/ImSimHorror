using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    PlayerMove playerMove;
    PlayerLook playerLook;

    void Start()
    {
        playerMove = this.GetComponent<PlayerMove>();
        playerLook = this.GetComponent<PlayerLook>();
    }

    void Update()
    {
        //put all inputs in here then call the other scripts with inputs or transforms


        //climb


        //crouch

    }
}
