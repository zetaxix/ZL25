using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StartAnimation : MonoBehaviour
{
    Animator animatior;

    private void Awake()
    {
        animatior = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(StartFlipAnim());

    }

    private void Update()
    {
        transform.rotation = Quaternion.Euler(-90, 90, 270);
    }

    IEnumerator StartFlipAnim()
    {
        yield return new WaitForSeconds(2.5f);
        animatior.SetTrigger("BeginCardAnim");
        yield return new WaitForSeconds(1f);
        animatior.SetTrigger("BeginCardAnimEnd");
    }
}
