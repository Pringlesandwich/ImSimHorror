using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("World Settings")]
    [SerializeField] float Gravity;

    void Start()
    {
        Physics.gravity = new Vector3(0, -Gravity, 0);
    }
}
