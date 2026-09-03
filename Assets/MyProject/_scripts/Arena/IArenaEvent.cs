using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArenaEvent
{
    string EventName { get; }
    bool IsActive { get; }

    void Activate();
    void Deactivate();
    void Tick(float deltaTime);
}
