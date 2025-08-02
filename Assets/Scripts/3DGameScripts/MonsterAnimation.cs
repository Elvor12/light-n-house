using UnityEngine;
using UnityEngine.AI;

public class MonsterAnimation : MonoBehaviour
{
    public Animator anime;
    public FirstPersonController playerScript;
    private MonsterLogic monsterLogic;

    private NavMeshAgent monster;
    public Sprite[] sprites;
    public SpriteRenderer spriteRenderer;



    void Start()
    {
        monsterLogic = GetComponent<MonsterLogic>();
        monster = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (anime == null) return;
        Vector3 playerPos = playerScript.transform.position;
        Vector3 directionToTarget = (playerPos - transform.position).normalized;
        Vector3 monsterDirection = monsterLogic.shiftedLookDirection.normalized;

        float speed = monster.velocity.magnitude / monsterLogic.normalSpeed;

        anime.speed = speed;

        directionToTarget.y = 0;
        monsterDirection.y = 0;
        if (directionToTarget.sqrMagnitude > 0f && monsterDirection.sqrMagnitude > 0f)
        {
            float angle = Vector3.Angle(monsterDirection, directionToTarget);
            bool right = Vector3.SignedAngle(directionToTarget, monsterDirection, Vector3.up) > 0;
            if (monster.velocity.sqrMagnitude > 0.1f)
            {
                anime.enabled = true;

                anime.SetFloat("Angle", angle);
                anime.SetBool("Right", right);
            }
            else
            {
                anime.enabled = false;
                if (angle > 120)
                {
                    spriteRenderer.sprite = sprites[3];
                }
                else if (angle > 60)
                {
                    if (right)
                    {
                        spriteRenderer.sprite = sprites[2];
                    }
                    else
                    {
                        spriteRenderer.sprite = sprites[1];
                    }
                }
                else
                {
                    spriteRenderer.sprite = sprites[0];
                }
                
            }
            
        }
        else
        {
            anime.SetFloat("Angle", 0f);
        }
    }
}
