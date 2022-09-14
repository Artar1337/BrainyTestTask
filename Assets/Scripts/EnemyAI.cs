using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    private Path _path;
    private int _currentWaypoint = 0;
    private float _nextWaypointDistance;
    private Seeker _seeker;

    private void Start()
    {
        _seeker = GetComponent<Seeker>();
        _nextWaypointDistance = GetComponent<AIPath>().pickNextWaypointDist;
        _seeker.StartPath(transform.position, 
            GetComponent<AIDestinationSetter>().target.transform.position, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_path == null)
            return;

        // path ended
        if (_currentWaypoint >= _path.vectorPath.Count)
        {
            Debug.Log("GOT IT!!!");
            return;
        }

        if (Vector2.Distance(transform.position, _path.vectorPath[_currentWaypoint]) < _nextWaypointDistance)
        {
            _currentWaypoint++;
        }
    }
}
