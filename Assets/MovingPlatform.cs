using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {
    private Rigidbody2D rb;

    [Header("Path")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private MovementAxis axis = MovementAxis.Points;

    [Header("Motion")]
    [SerializeField] private float speed;      // units per second
    [SerializeField] private float waitTime; // pause at each end

    private Vector3 a;
    private Vector3 b;
    private Vector3 target;
    private float waitTimer;
    private bool waiting;

    public enum MovementAxis { Points, Horizontal, Vertical }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start() {
        a = startPoint.position;
        b = endPoint.position;

        if (axis == MovementAxis.Horizontal) {
            a.y = transform.position.y;
            b.y = transform.position.y;
        }
        else if (axis == MovementAxis.Vertical) {
            a.x = transform.position.x;
            b.x = transform.position.x;
        }

        transform.position = a;
        target = b;

        waiting = false;
        waitTimer = 0f;
    }

    private void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        if (waiting) {
            waitTimer -= dt;
            if (waitTimer <= 0f) waiting = false;
            return;
        }

        Vector3 current = rb ? (Vector3)rb.position : transform.position;
        Vector3 next = Vector3.MoveTowards(current, target, speed * dt);

        if (rb) rb.MovePosition(next);
        else transform.position = next;

        if ((next - target).sqrMagnitude <= 0.0001f) {
            target = (target == a) ? b : a;
            waiting = waitTime > 0f;
            waitTimer = waitTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (axis != MovementAxis.Vertical) {
            collision.collider.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision) {
        collision.collider.transform.SetParent(null);
    }

    private void OnDrawGizmos() {
        // Preview path in Scene view
        if (startPoint != null && endPoint != null) {
            Vector3 pA = startPoint.position;
            Vector3 pB = endPoint.position;

            if (axis == MovementAxis.Horizontal) {
                pA.y = transform.position.y;
                pB.y = transform.position.y;
            }
            else if (axis == MovementAxis.Vertical) {
                pA.x = transform.position.x;
                pB.x = transform.position.x;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pA, pB);
            Gizmos.DrawWireSphere(pA, 0.08f);
            Gizmos.DrawWireSphere(pB, 0.08f);
        }
    }

}
