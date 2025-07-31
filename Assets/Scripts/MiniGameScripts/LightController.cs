using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LightController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float angleOffset = -90f;

    private ScenesManager scenesManager;

    void Start()
    {
        scenesManager = FindAnyObjectByType<ScenesManager>();
    }

    void Update()
    {

        if (scenesManager.IsUsingSecondCamera())
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (h != 0 || v != 0)
            {
                float angle = Mathf.Atan2(v, h) * Mathf.Rad2Deg + angleOffset;
                Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}
