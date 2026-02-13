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
        [SerializeField] float maxSpeed;
        [SerializeField] float accelerate = 10f;

        float arriveRadius;
        
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

            arriveRadius = maxSpeed * maxSpeed / accelerate * 0.5f;
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
            FindPath();
        }
        IEnumerator Moving()
        {
            isMoving= true;
            while ()
            {

            }
        }
    }
}