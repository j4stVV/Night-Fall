using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    PlayerStats player;
    CircleCollider2D playerCollector;
    public float pullSpeed; 
    void Start()
    {
        player = FindObjectOfType<PlayerStats>();
        playerCollector = GetComponent<CircleCollider2D>();
    }

    void Update()
    {
        playerCollector.radius = player.CurrentMagnet;    
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out ICollectible collectible))
        {
            //Pulling animation
            //Gets the rigidbody 2D component on the item
            Rigidbody2D rb = col.gameObject.GetComponent<Rigidbody2D>();
            //Vector2 pointing from the item to the player
            Vector2 forceDirection = (transform.position - col.transform.position).normalized;  
            //Applies force to the item in the forceDirection with pullSpeed
            rb.AddForce(forceDirection * pullSpeed);

            collectible.Collect();
        }
    }
}
