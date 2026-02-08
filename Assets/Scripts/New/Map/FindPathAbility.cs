using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BF;

namespace MapSearch
{
	public class FindPathAbility : MonoBehaviour
	{
		[SerializeField] Rigidbody2D rb;

		[SerializeField] Vector3 aimPos;
        Vector3 AimPos
        {
            get => aimPos;
            set 
            {
                aimPos = value;
                FindPath(aimPos);
            }
        }

        private void Start()
        {
            InputManager.onClick+=(Vector3 screenPos) =>
            {
               Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
                AimPos = new Vector3(worldPos.x,worldPos.y,0);
            };
        }
        void FindPath(Vector3 pos)
        {
             MapManager.Instance().World2FullGrid(pos);
        }
    }
}