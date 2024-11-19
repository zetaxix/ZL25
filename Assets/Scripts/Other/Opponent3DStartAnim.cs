using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opponent3DStartAnim : MonoBehaviour
{
    public static Opponent3DStartAnim instance;
    
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (instance == null )
        {
            instance = this;
        }
    }

    public void StartAnim()
    {
        animator.SetTrigger("OpponentAnimStart");
    }
}
