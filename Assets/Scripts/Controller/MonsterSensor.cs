using UnityEngine;
using System;

public class MonsterSensor : MonoBehaviour {
    public Action<Collider2D> onEnter;
    public Action<Collider2D> onExit;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) onEnter?.Invoke(other);
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) onExit?.Invoke(other);
    }
}