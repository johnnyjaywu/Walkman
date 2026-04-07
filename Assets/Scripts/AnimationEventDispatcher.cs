using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationEventDispatcher : MonoBehaviour
{
    public event Action<string> OnCustomEventTriggered;

    // This method acts as the sole receiver for Unity's reflection-based Animation Events configured in the timeline.
    // It encapsulates the string payload and converts it into a standard C# event for decoupled systems to consume.
    public void TriggerCustomEvent(string eventName)
    {
        OnCustomEventTriggered?.Invoke(eventName);
    }
}