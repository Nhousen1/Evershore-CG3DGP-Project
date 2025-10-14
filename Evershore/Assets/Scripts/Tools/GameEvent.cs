using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Borrowed from: https://github.com/roboryantron/Unite2017
// Unite 2017 - Game Architecture with Scriptable Objects
[CreateAssetMenu(fileName = "GameEvent", menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    /// <summary>
    /// The list of listeners that this event will notify if it is raised.
    /// </summary>
    private readonly List<IGameEventListener> m_eventListeners = new List<IGameEventListener>();
    /// <summary>
    /// Calls the GameEvent.
    /// </summary>
    public void Raise()
    {
        for (int i = m_eventListeners.Count - 1; i >= 0; i--)
        {
            m_eventListeners[i].OnEventRaised();
        }
    }
    public void RegisterListener(IGameEventListener listener)
    {
        if (!m_eventListeners.Contains(listener))
            m_eventListeners.Add(listener);
    }
    public void UnregisterListener(IGameEventListener listener)
    {
        if (m_eventListeners.Contains(listener))
            m_eventListeners.Remove(listener);
    }
}
public interface IGameEventListener
{
    void OnEventRaised();
}
