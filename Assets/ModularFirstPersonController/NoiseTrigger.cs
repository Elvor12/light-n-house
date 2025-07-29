using UnityEngine;

public class NoiseTrigger : MonoBehaviour
{
    public Rigidbody player;
    private FirstPersonController playerScript;
    private bool playerInside = false;
    public MonsterLogic monster;
    void Start()
    {
        playerScript = player.GetComponent<FirstPersonController>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
    void Update()
    {
        if (playerInside && player != null)
        {
            if (player.linearVelocity.sqrMagnitude > 0.01f && !playerScript.IsCRouched)
            {
                Debug.Log("hear");
                monster.NoiseSetup(player.position);
            }
        }
    }
}
