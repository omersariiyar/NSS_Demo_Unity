using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class BallController : MonoBehaviour
{
    [Header("Ayarlar")]
    public float maxDragDistance = 7f; // Topu en fazla ne kadar çekebiliriz
    public float powerMultiplier = 10f;  // Çekme mesafesini hıza çeviren çarpan
    public float loftMultiplier = 0.5f;  // Hıza göre topun ne kadar havalanacağı

    [Header("Referanslar")]
    private BallPhysics physicsScript;
    private LineRenderer aimLine;
    private Vector2 dragStartPos;
    private bool isDragging = false;

    void Start()
    {
        physicsScript = GetComponent<BallPhysics>();
        aimLine = GetComponent<LineRenderer>();

        aimLine.positionCount = 0;
    }

    void Update()
    {
        if (physicsScript.GetComponent<Rigidbody2D>().linearVelocity.magnitude > 0.1f) return;

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.transform == transform)
            {
                isDragging = true;
                dragStartPos = transform.position;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVector = dragStartPos - currentMousePos;

            dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

            DrawAimLine(dragVector);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            aimLine.positionCount = 0;

            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVector = dragStartPos - currentMousePos;
            dragVector = Vector2.ClampMagnitude(dragVector, maxDragDistance);

            Shoot(dragVector);
        }
    }

    void DrawAimLine(Vector2 direction)
    {
        aimLine.positionCount = 2;
        aimLine.SetPosition(0, transform.position);
        aimLine.SetPosition(1, (Vector2)transform.position + direction);
    }

    void Shoot(Vector2 forceVector)
    {
        float speed = forceVector.magnitude * powerMultiplier;

        Vector2 direction = forceVector.normalized;

        float loft = speed * loftMultiplier;

        physicsScript.Shoot(direction, speed, loft, 0f);
    }
}