using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Animator animator;

    [Header("Settings")]
    [SerializeField] float deathZoneY = -200f;

    [Header("Audio")]
    [SerializeField] AudioClip collectClip;
    [SerializeField] AudioClip deathClip;

    [SerializeField] AudioSource audioSource;

    Rigidbody2D rb;

    bool canDie = true;

    bool hasDied = false;

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

    private void Update()
    {
        if (!hasDied)
        {
            if (transform.position.y <= deathZoneY)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        if (!hasDied)
        {
            hasDied = true;

            EventSystemNew.RaiseEvent(Event_Type.CHARACTER_DIED);

            audioSource.PlayOneShot(deathClip);

            rb.bodyType = RigidbodyType2D.Static;

            animator.SetTrigger("Death");
        }
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
        if (collision.CompareTag("Collectable") && canDie && collision.TryGetComponent(out Collectable collectable))
        {
            if (collectable.pickupable)
            {
                audioSource.PlayOneShot(collectClip);
            }
        }
    }
}
