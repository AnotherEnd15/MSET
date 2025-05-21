using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class MapScrollChunk : MonoBehaviour
    {
        public GameObject ScrollChunk;
        public int ScrollChunkSize = 10;

        // 留后一个+当前+前两个chunk
        private LinkedList<GameObject> Chunks = new();

        public GameObject Target;

        private float checkTime;

        // Start is called before the first frame update
        void Start()
        {
            this.ScrollChunk.SetActive(false);
            var go = GameObject.Instantiate(this.ScrollChunk,this.transform);
            go.transform.localPosition = Vector3.zero;
            go.SetActive(true);
            Chunks.AddFirst(go);

            if (Target != null)
            {
                CheckAndSpawn(this.Target.transform.position);
            }

            checkTime = Time.time;
        }

        void CheckAndSpawn(Vector3 pos)
        {
            var node = this.Chunks.First;
            while (node != null)
            {
                if (InChunk(node.Value, pos))
                {
                    // 往后创建两个chunk
                    var targetChunk = node;
                    for (int i = 0; i < 2; i++)
                    {
                        if (targetChunk.Next == null)
                        {
                            AddNext(targetChunk);
                        }

                        targetChunk = targetChunk.Next;
                    }

                    // 往前大于1的chunk销毁

                   var first = this.Chunks.First;
                   while (first != node && first.Next != node)
                   {
                       GameObject.Destroy(first.Value);
                       Chunks.RemoveFirst();
                       first = this.Chunks.First;
                   }

                    return;
                }

                node = node.Next;
            }
        }

        void AddNext(LinkedListNode<GameObject> node)
        {
            var go = GameObject.Instantiate(this.ScrollChunk,this.transform);
            go.transform.localPosition = node.Value.transform.localPosition+new Vector3(0,0,this.ScrollChunkSize);
            go.transform.localScale = node.Value.transform.localScale + new Vector3(0.1f, 0, 0);

            if (go.transform.localScale.x >= 2)
            {
                go.transform.localScale = Vector3.one;
            }

            go.SetActive(true);
            this.Chunks.AddAfter(node, go);
        }

        bool InChunk(GameObject go, Vector3 pos)
        {
            var goPos = go.transform.position;
            var halfSize = this.ScrollChunkSize / 2;
            var yMax =  goPos.z + halfSize;
            var yMin = goPos.z - halfSize;
            if (pos.z <= yMax && pos.z >= yMin)
            {
                return true;
            }

            return false;
        }

        private void Update()
        {
            if (this.Target == null)
                return;


            if (Time.time - checkTime < 0.2f)
            {
                return;
            }

            checkTime = Time.time;

            CheckAndSpawn(this.Target.transform.position);
        }
    }
}
