using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoMenuController : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GameObject.Find("InfoMenu").GetComponent<Animator>();

        Invoke("SlideNotification", 1.0f);
    }

    // Delay pop-up notification
    private void SlideNotification()
    {
        //Play notification menu animation.
        animator.Play("NotificationAnim");
    }
}
