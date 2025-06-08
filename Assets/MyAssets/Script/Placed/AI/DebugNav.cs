using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DebugNav : MonoBehaviour
{
    NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        StartCoroutine(TestCoroutine());
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    IEnumerator TestCoroutine()
    {
        float waitTime = Random.Range(1.0f, 5.0f);
        yield return new WaitForSeconds(waitTime);
        MoveActor();
    }

    private void MoveActor()
    {
        float x = Random.Range(-3.0f, 3.0f);
        float z = Random.Range(-3.0f, 3.0f);
        Vector3 des = new Vector3(x, 0, z);
        agent.SetDestination(transform.position + des);

        StartCoroutine(TestCoroutine());
    }
}
