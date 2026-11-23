using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputProvider
{
    float GetHorizontal();
    bool JumpPressed();
    bool DashPressed();
}

public class PlayerInputProvider : MonoBehaviour, IInputProvider
{
    public float GetHorizontal()
    {
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontal -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontal += 1f;
        }

        return horizontal;
    }

    public bool JumpPressed() => Input.GetKeyDown(KeyCode.Space) || 
                                 Input.GetKeyDown(KeyCode.W) ||    
                                 Input.GetKeyDown(KeyCode.UpArrow);

    public bool DashPressed() => Input.GetKeyDown(KeyCode.LeftShift) ||
                                 Input.GetKeyDown(KeyCode.DownArrow) ||
                                 Input.GetKeyDown(KeyCode.Q) ||
                                 Input.GetKeyDown(KeyCode.E);
    
}


/* public class PlayerInputProvider : MonoBehaviour, IInputProvider
{
    private GameSettingsData Settings
    {
        get
        {
            return GameSettingsManager.Instance != null
                ? GameSettingsManager.Instance.Current
                : null;
        }
    }

    public float GetHorizontal()
    {
        GameSettingsData settings = Settings;

        if (settings == null)
            return Input.GetAxisRaw("Horizontal");

        float horizontal = 0f;

        // Movimento para esquerda
        if (Input.GetKey(settings.moveLeft) ||
            Input.GetKey(settings.moveLeftAlt))
        {
            horizontal -= 1f;
        }

        // Movimento para direita
        if (Input.GetKey(settings.moveRight) ||
            Input.GetKey(settings.moveRightAlt))
        {
            horizontal += 1f;
        }

        return horizontal;
    }

    public bool JumpPressed()
    {
        GameSettingsData settings = Settings;

        if (settings == null)
            return Input.GetButtonDown("Jump");

        return Input.GetKeyDown(settings.jump) ||
               Input.GetKeyDown(settings.jumpAlt) ||
               Input.GetKeyDown(settings.jumpAlt2);
    }

    public bool DashPressed()
    {
        GameSettingsData settings = Settings;

        if (settings == null)
        {
            return Input.GetKeyDown(KeyCode.LeftShift) ||
                   Input.GetKeyDown(KeyCode.Mouse0) ||
                   Input.GetKeyDown(KeyCode.Q) ||
                   Input.GetKeyDown(KeyCode.R);
        }

        return Input.GetKeyDown(settings.dash) ||
               Input.GetKeyDown(settings.dashAlt) ||
               Input.GetKeyDown(settings.dashAlt2) ||
               Input.GetKeyDown(settings.dashAlt3);
    }
}

*/