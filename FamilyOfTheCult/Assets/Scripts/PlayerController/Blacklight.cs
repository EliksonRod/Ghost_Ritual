using UnityEngine;

public class Blacklight : MonoBehaviour
{
    public float range = 20f;
    public float lightExposurePerSecond = 2f;
    MonsterController MonsterScript;
    //public LayerMask monsterMask;

    private void OnDisable()
    {
        if (MonsterScript != null) StopBurn();
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            MonsterController monsterScript = hit.collider.GetComponent<MonsterController>();

            if (monsterScript != null)
            {
                MonsterScript = monsterScript;
                BurnTarget();
            }
        }
        else
        {
            StopBurn();
        }
    }


    void BurnTarget()
    {
        MonsterScript.ApplyLightExposure(lightExposurePerSecond * Time.deltaTime);
    }

    void StopBurn()
    {
        MonsterScript.StopLightExposure();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * range);
    }
}
