using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BF;
using Sirenix.OdinInspector;

namespace MapSearch
{
	public class FindPathAbility : MonoBehaviour
	{
		[SerializeField] Rigidbody2D rb;

		[SerializeField] Vector3 aimPos;

        Action<List<FullGrid>> onAskFindPath = delegate { };

        bool isWaitingReFindPath = false;

        [TableList]
        [SerializeField] List<FullGrid> pathList;


        [SerializeField] bool isMoving;

        [FoldoutGroup("Parameter")]
        [SerializeField] float realSpeed;
        [FoldoutGroup("Parameter")]
        [SerializeField] float maxSpeed;
        [FoldoutGroup("Parameter")]
        [SerializeField] float accelerate = 10f;

        [SerializeField] float sqrArriveRadius;
        
        Vector3 AimPos
        {
            get => aimPos;
            set 
            {
                aimPos = value;
                FindPath();
            }
        }

        private void Start()
        {
            InputManager.onClick+=(Vector3 screenPos) =>
            {
               Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                AimPos = new Vector3(worldPos.x,worldPos.y,0);
            };

            onAskFindPath += Move;

            sqrArriveRadius = (maxSpeed * maxSpeed / accelerate) * (maxSpeed * maxSpeed / accelerate)*0.25f;
        }
        void FindPath()
        {
            if (isWaitingReFindPath)
            {
                StopCoroutine("ReFindPath");
                isWaitingReFindPath = false;
            }
            var end = MapManager.Instance().World2FullGrid(aimPos);
            var start = MapManager.Instance().World2FullGrid(rb.position);

            PathFinder.Instance().FindPath(start, end, onAskFindPath);
        }
        void Move(List<FullGrid> path)
        {
            if (path == null)
            {
                StartCoroutine("ReFindPath");
                return;
            }

            pathList = path;
            StartCoroutine("Moving");
        }
        IEnumerator ReFindPath()
        {
            isWaitingReFindPath = true;
            yield return null;
            Debug.Log("ReFind");
            FindPath();
        }
        IEnumerator Moving()
        {
            isMoving= true;
            int pathIndex = 1;
            Vector3 nextPos = MapManager.Instance().FullGrid2WorldPos(pathList[pathIndex]);
            MapDebug.Instance().DebugPath(pathList);
            Debug.Log("Count"+pathList.Count);
            while (isMoving)  
            {
                if (pathIndex >= pathList.Count - 1 && ((Vector3)rb.position - aimPos).sqrMagnitude < sqrArriveRadius) 
                {
                    realSpeed = Mathf.Max(0f, realSpeed - Time.deltaTime * accelerate);
                    Vector2 orient = ((Vector2)aimPos - rb.position).normalized;
                    rb.velocity = orient * realSpeed;
                    if (realSpeed == 0f || Vector3.Dot(orient, ((Vector3)rb.position - aimPos)) < 0) 
                    {
                        realSpeed = 0f;
                        rb.velocity = Vector2.zero;
                        isMoving = false;
                        Debug.Log("Over");
                    }
                }
                else
                {
                    realSpeed = Mathf.Min(maxSpeed, realSpeed + Time.deltaTime * accelerate);
                    //Arrive
                    if (Vector3.Dot(rb.velocity, nextPos - (Vector3)rb.position) < 0) 
                    {
                        pathIndex++;
                        if (pathIndex < pathList.Count - 1)
                        {
                            nextPos = MapManager.Instance().FullGrid2WorldPos(pathList[pathIndex]);
                            Debug.Log("Arrive" + pathIndex);
                        }
                        else
                        {
                            nextPos = aimPos;
                            Debug.Log("Arrive!!" + pathIndex);
                        }
                    }
                    Vector2 orient = ((Vector2)nextPos - rb.position).normalized;
                    rb.velocity = orient * realSpeed;
                }
                yield return null;
            }
        }
    }
}