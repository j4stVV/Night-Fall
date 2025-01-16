using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkTrigger : MonoBehaviour
{
    private MapController mapController;
    [SerializeField] private GameObject targerMap;

    void Start()
    {
        mapController = FindObjectOfType<MapController>();    
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            mapController.currentChunk = targerMap;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (mapController.currentChunk == targerMap)
            {
                mapController.currentChunk = null;  
            }
        }
    }
}
