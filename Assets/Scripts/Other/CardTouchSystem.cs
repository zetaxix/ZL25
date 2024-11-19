using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTouchSystem : MonoBehaviour
{
    private Vector3 initialRotation;
    private Vector3 lastMousePosition;
    [SerializeField] float rotationSpeed = 1f; // Döndürme hassasiyeti
    [SerializeField] float minVerticalAngle = -10f; // Yukarý hareket sýnýrý
    [SerializeField] float maxVerticalAngle = 10f;  // Aþaðý hareket sýnýrý

    private void Start()
    {
        initialRotation = transform.eulerAngles; // Baþlangýç rotasyonu
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

            float rotX = delta.y * rotationSpeed; // Y ekseni için dikey hareketi alýr
            float rotY = -delta.x * rotationSpeed; // X ekseni için yatay hareketi alýr

            // Yan döndürme (Y ekseni - serbest)
            transform.Rotate(Vector3.up, rotY, Space.World);

            // Yukarý döndürme (X ekseni - sýnýrlý)
            float newXRotation = transform.eulerAngles.x + rotX;

            // Eðim sýnýrlarýný uygula
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

                // Yan döndürme (Y ekseni - serbest)
                transform.Rotate(Vector3.up, rotY, Space.World);

                // Yukarý döndürme (X ekseni - sýnýrlý)
                float newXRotation = transform.eulerAngles.x + rotX;

                // Eðim sýnýrlarýný uygula
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

    // Açýyý -180 ile 180 arasýnda sýnýrlandýrma
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
