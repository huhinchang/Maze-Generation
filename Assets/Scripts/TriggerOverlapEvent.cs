using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerOverlapEvent : MonoBehaviour
{

    public UnityEvent triggerEnterEvent;
    public UnityEvent triggerExitEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        triggerEnterEvent.Invoke();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        triggerExitEvent.Invoke();
    }
}
