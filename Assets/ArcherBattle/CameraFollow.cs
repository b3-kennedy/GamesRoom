using UnityEngine;


namespace Assets.ArcherBattle
{
    public class CameraFollow : MonoBehaviour
    {
        public Vector3 startPos;
        public Transform target;
        public bool isFollow;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isFollow)
            {
                transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
            }

        }
    }
}

