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

        StartCoroutine(SlideNotification());
    }

    // Delay pop-up notification
    private IEnumerator SlideNotification()
    {
        //Wait for 2 secs.
        yield return new WaitForSeconds(2);

        //Play notification menu animation.
        animator.Play("NotificationAnim");
    }
}
