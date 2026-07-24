using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputProvider : MonoBehaviour, IInputProvider
{
    public float GetHorizontal() => Input.GetAxisRaw("Horizontal");
    public bool JumpPressed() => Input.GetButtonDown("Jump");
    public bool DashPressed() => Input.GetButtonDown("Dash") || Input.GetButtonDown("Fire3") || Input.GetKeyDown(KeyCode.R);
}

public interface IInputProvider
{
    float GetHorizontal();
    bool JumpPressed();
    bool DashPressed();
}