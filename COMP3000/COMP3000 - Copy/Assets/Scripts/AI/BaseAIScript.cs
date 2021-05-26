using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseAIScript : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private LayerMask groundMask, playerMask;

    [SerializeField]
    private Vector3 walkPoint;
    private bool walkPointSet;
    [SerializeField]
    private float walkPointRange;

    [SerializeField]
    private float attackSpeed;
    private bool alreadyAttacked;

    [SerializeField]
    private float sightRange, attackRange;
    [SerializeField]
    private bool playerInRange, playerInAttackRange;
    [SerializeField]
    private bool playerInSight;

    [SerializeField]
    private float patience = 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerMask);

        if (playerInRange) playerInSight = Mathf.Abs(Vector3.Angle(player.position - transform.position, transform.forward)) < 50;
        if (!playerInRange) playerInSight = false;

        if (playerInSight) patience = 100;

        if (patience > 1)
        {
            if (!playerInAttackRange) ChasePlayer();
            if (playerInAttackRange) AttackPlayer();
            patience -= Time.deltaTime * 30;
        }
        else Patroling();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, groundMask))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(player);

        if (!alreadyAttacked)
        {
            //Shooting code here

            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), attackSpeed);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
