using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Default Spawn Point")]
    [SerializeField] private Transform defaultSpawnPoint;

    [Header("Loop 9 Spawn Point")]
    [SerializeField] private Transform loop9SpawnPoint;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        int currentLoop = GameManager.Instance.CurrentLoop;
        if (player == null) return;

        if(currentLoop == 9)
        {
            // For loop 9, we want to place the player at a specific spawn point
            if (loop9SpawnPoint != null)
            {
                player.transform.position = loop9SpawnPoint.position;
                Debug.Log("[PlayerSpawner] Placed player at Loop 9 spawn point.");
                return;
            }
        }

        if (defaultSpawnPoint != null)
        {
            player.transform.position = defaultSpawnPoint.position;
            Debug.Log("[PlayerSpawner] Placed player at default spawn point.");
        }
    }
}