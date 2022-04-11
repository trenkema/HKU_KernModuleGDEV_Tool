using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;

    [Header("Audio")]
    [SerializeField] AudioClip collectClip;
    [SerializeField] AudioClip deathClip;

    [SerializeField] AudioSource audioSource;

    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Die()
    {
        audioSource.PlayOneShot(deathClip);

        rb.bodyType = RigidbodyType2D.Static;

        animator.SetTrigger("Death");
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collectable"))
        {
            audioSource.PlayOneShot(collectClip);

            Destroy(collision.gameObject);
        }
    }
}
