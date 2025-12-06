using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ShootingManager : MonoBehaviour
{
    [Header("Referanslar")]
    public BallPhysics ballPhysics;
    public LineRenderer aimLine;

    [Header("UI Referanslar")]
    public GameObject impactPanel;
    public Image ballUIImage;

    [Header("Impact Panel Ayarları")]
    public float imageMovementSpeed = 250f;
    public float imageRotationSpeed = 200f;
    private Coroutine imageMoveCoroutine;

    [Header("Vuruş Ayarları")]
    public float maxPower = 30f;
    public float loftMultiplier = 15f;
    public bool useSlingshotAim = true;

    private enum State { Aiming, ImpactSelect, Shooting }
    private State currentState = State.Aiming;

    private Vector2 shotDirection;
    private float shotPowerRatio;
    private Vector2 dragStartPos;
    private bool isDragging = false;

    private bool canCheckStop = false;

    void Start()
    {
        impactPanel.SetActive(false);
        aimLine.positionCount = 0;
    }

    void Update()
    {
        if (currentState == State.Aiming)
        {
            HandleAimInput();
        }
        else if (currentState == State.Shooting)
        {
            if (canCheckStop)
            {
                CheckIfBallStopped();
            }
        }
    }

    void HandleAimInput()
    {
        if (!ballPhysics.IsStopped) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (Vector2.Distance(mousePos, ballPhysics.transform.position) < 2.0f)
            {
                isDragging = true;
                dragStartPos = ballPhysics.transform.position;
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 dragVector;

            if (useSlingshotAim)
                dragVector = dragStartPos - currentMousePos;
            else
                dragVector = currentMousePos - dragStartPos;

            dragVector = Vector2.ClampMagnitude(dragVector, 3.0f);

            aimLine.positionCount = 2;
            aimLine.SetPosition(0, ballPhysics.transform.position);
            aimLine.SetPosition(1, (Vector2)ballPhysics.transform.position + dragVector);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            aimLine.positionCount = 0;

            Vector2 currentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 finalDragVector;
            if (useSlingshotAim)
                finalDragVector = dragStartPos - currentMousePos;
            else
                finalDragVector = currentMousePos - dragStartPos;

            shotDirection = finalDragVector.normalized;
            shotPowerRatio = Mathf.Clamp01(finalDragVector.magnitude / 3.0f);

            if (shotPowerRatio > 0.1f)
            {
                SwitchToImpactMode();
            }
        }
    }

    void SwitchToImpactMode()
    {
        currentState = State.ImpactSelect;
        impactPanel.SetActive(true);

        ballUIImage.rectTransform.rotation = Quaternion.identity;

        if (imageMoveCoroutine != null)
        {
            StopCoroutine(imageMoveCoroutine);
        }
        imageMoveCoroutine = StartCoroutine(MoveBallImage());
    }

    public void OnImpactSelected(BaseEventData eventData)
    {
        if (currentState != State.ImpactSelect) return;

        if (imageMoveCoroutine != null)
        {
            StopCoroutine(imageMoveCoroutine);
            imageMoveCoroutine = null;
        }

        PointerEventData pointerData = (PointerEventData)eventData;
        Vector2 localPoint;

        Quaternion originalRotation = ballUIImage.rectTransform.rotation;
        ballUIImage.rectTransform.rotation = Quaternion.identity;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ballUIImage.rectTransform,
            pointerData.position,
            null,
            out localPoint
        );

        ballUIImage.rectTransform.rotation = originalRotation;

        float width = ballUIImage.rectTransform.rect.width;
        float height = ballUIImage.rectTransform.rect.height;
        float xNormal = (localPoint.x / (width / 2f));
        float yNormal = (localPoint.y / (height / 2f));

        ApplyShot(xNormal, yNormal);
    }

    void ApplyShot(float hitX, float hitY)
    {
        impactPanel.SetActive(false);
        currentState = State.Shooting;

        canCheckStop = false;
        StartCoroutine(EnableStopCheckDelay());

        float curve = -hitX;
        float loftRatio = Mathf.Clamp01(-hitY);
        float finalSpeed = shotPowerRatio * maxPower;
        float finalLoft = loftRatio * loftMultiplier * shotPowerRatio;

        ballPhysics.Shoot(shotDirection, finalSpeed, finalLoft, curve);
    }

    IEnumerator MoveBallImage()
    {
        yield return null;

        RectTransform panelRect = impactPanel.GetComponent<RectTransform>();
        RectTransform ballRect = ballUIImage.rectTransform;

        float halfPanelWidth = panelRect.rect.width / 2f;
        float startX;
        float endX;
        int direction;

        if (Random.value > 0.5f)
        {
            startX = -halfPanelWidth;
            endX = halfPanelWidth;
            direction = 1;
        }
        else
        {
            startX = halfPanelWidth;
            endX = -halfPanelWidth;
            direction = -1;
        }

        ballRect.anchoredPosition = new Vector2(startX, ballRect.anchoredPosition.y);
        float currentX = startX;

        while ((direction == 1 && currentX < endX) || (direction == -1 && currentX > endX))
        {
            currentX += imageMovementSpeed * direction * Time.deltaTime;
            ballRect.anchoredPosition = new Vector2(currentX, ballRect.anchoredPosition.y);
            ballRect.Rotate(0, 0, -direction * imageRotationSpeed * Time.deltaTime);
            yield return null;
        }

        CancelShoot();
    }

    void CancelShoot()
    {
        Debug.Log("Vuruş iptal edildi! Zamanında basılmadı.");
        ResetSystem();
        MatchManager.instance.EndPlaySession();
    }

    IEnumerator EnableStopCheckDelay()
    {
        yield return new WaitForSeconds(0.5f);
        canCheckStop = true;
    }

    void CheckIfBallStopped()
    {
        if (ballPhysics.IsStopped)
        {
            MatchManager.instance.EndPlaySession();
        }
    }

    public void ResetSystem()
    {
        if (imageMoveCoroutine != null)
        {
            StopCoroutine(imageMoveCoroutine);
            imageMoveCoroutine = null;
        }

        impactPanel.SetActive(false);
        currentState = State.Aiming;
        isDragging = false;
        aimLine.positionCount = 0;
        canCheckStop = false;
    }
}