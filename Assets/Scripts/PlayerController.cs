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

    bool canDie = true;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.CHARACTER_FINISHED, CharacterFinished);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.CHARACTER_FINISHED, CharacterFinished);
    }

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

    private void CharacterFinished()
    {
        canDie = false;
    }

    private void LevelFailed()
    {
        EventSystemNew.RaiseEvent(Event_Type.LEVEL_FAILED);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap") && canDie)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Collectable") && canDie)
        {
            audioSource.PlayOneShot(collectClip);

            Destroy(collision.gameObject);
        }
    }
}
