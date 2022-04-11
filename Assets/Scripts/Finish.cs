using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    [SerializeField] float completeDelayTime = 2f;

    [SerializeField] AudioClip finishClip;

    [SerializeField] AudioSource audioSource;

    bool levelCompleted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !levelCompleted)
        {
            levelCompleted = true;

            audioSource.PlayOneShot(finishClip);

            Invoke("CompleteLevel", completeDelayTime);
        }
    }

    private void CompleteLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
