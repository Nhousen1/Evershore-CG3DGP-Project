using UnityEngine;
using UnityEngine.Events;

// From: https://github.com/roboryantron/Unite2017
// Unite 2017 - Game Architecture with Scriptable Objects
// Summary: Responds to GameEvents with a UnityEvent that can be easily defined in the inspector
public class UnityGameEventListener : MonoBehaviour, IGameEventListener
{
    [Tooltip("Event to register with.")]
    [SerializeField]
    private GameEvent @event;

    [Tooltip("Response to invoke when Event is raised.")]
    [SerializeField]
    private UnityEvent response;

    public void OnEnable()
    {
        if (@event != null) @event.RegisterListener(this);
    }

    public void OnDisable()
    {
        @event.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        response?.Invoke();
    }
}