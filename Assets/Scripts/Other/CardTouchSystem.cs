using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTouchSystem : MonoBehaviour
{
    private Vector3 initialRotation;
    private Vector3 lastMousePosition;
    [SerializeField] float rotationSpeed = 1f; // D�nd�rme hassasiyeti
    [SerializeField] float minVerticalAngle = -10f; // Yukar� hareket s�n�r�
    [SerializeField] float maxVerticalAngle = 10f;  // A�a�� hareket s�n�r�

    private void Start()
    {
        initialRotation = transform.eulerAngles; // Ba�lang�� rotasyonu
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseRotation();
#elif UNITY_ANDROID || UNITY_IOS
        HandleTouchRotation();
#endif
    }

    void HandleMouseRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            lastMousePosition = Input.mousePosition;

            float rotX = delta.y * rotationSpeed; // Y ekseni i�in dikey hareketi al�r
            float rotY = -delta.x * rotationSpeed; // X ekseni i�in yatay hareketi al�r

            // Yan d�nd�rme (Y ekseni - serbest)
            transform.Rotate(Vector3.up, rotY, Space.World);

            // Yukar� d�nd�rme (X ekseni - s�n�rl�)
            float newXRotation = transform.eulerAngles.x + rotX;

            // E�im s�n�rlar�n� uygula
            newXRotation = ClampAngle(newXRotation, minVerticalAngle, maxVerticalAngle);

            Vector3 clampedRotation = new Vector3(newXRotation, transform.eulerAngles.y, transform.eulerAngles.z);
            transform.eulerAngles = clampedRotation;
        }
    }

    void HandleTouchRotation()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                float rotX = delta.y * rotationSpeed * Time.deltaTime;
                float rotY = -delta.x * rotationSpeed * Time.deltaTime;

                // Yan d�nd�rme (Y ekseni - serbest)
                transform.Rotate(Vector3.up, rotY, Space.World);

                // Yukar� d�nd�rme (X ekseni - s�n�rl�)
                float newXRotation = transform.eulerAngles.x + rotX;

                // E�im s�n�rlar�n� uygula
                newXRotation = ClampAngle(newXRotation, minVerticalAngle, maxVerticalAngle);

                Vector3 clampedRotation = new Vector3(newXRotation, transform.eulerAngles.y, transform.eulerAngles.z);
                transform.eulerAngles = clampedRotation;
            }
        }
    }

    public void EnableCardRotate()
    {
        Animator animator = GetComponent<Animator>();

        animator.enabled = false;
    }

    // A��y� -180 ile 180 aras�nda s�n�rland�rma
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
