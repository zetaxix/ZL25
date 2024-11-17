using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTouchSystem : MonoBehaviour
{
    private Vector3 initialRotation;
    private Vector3 lastMousePosition;
    [SerializeField] float rotationSpeed = 1f; // Döndürme hassasiyeti

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

            transform.Rotate(Vector3.up, rotY, Space.World);  // Y ekseninde dünya çapýnda döndür
            transform.Rotate(Vector3.right, rotX, Space.World); // X ekseninde dünya çapýnda döndür
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

                transform.Rotate(Vector3.up, rotY, Space.World);  // Y ekseninde dünya çapýnda döndür
                transform.Rotate(Vector3.right, rotX, Space.World); // X ekseninde dünya çapýnda döndür
            }
        }
    }

    public void EnableCardRotate()
    {
        Animator animator = GetComponent<Animator>();

        animator.enabled = false;
    }
}