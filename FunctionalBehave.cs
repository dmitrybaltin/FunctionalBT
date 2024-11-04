using System;
using System.Collections.Generic;
using System.Diagnostics;
using FunctionlBT;
using UnityEngine;

namespace FunctionalBtTest
{
    public class ActorBoard
    {
        public Vector3 playerPos;
        public Vector3 playerWorldPos;
        public float playerDistance;   
        public bool playerInRange;
        public bool engaged;
        public bool foo;
    }

    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
    public class FunctionalBehave : MonoBehaviour
    {
        private GameObject player;
        
        private ActorBoard actorBoard = new();
        private AdvancedBT bt = new();
        
        private Rigidbody body;
        private Collider collider;

        private Vector3 initialLocalScale;

        private Guid guid;

        private static List<FunctionalBehave> _allEnemies = new();

        [SerializeField] private float baseClosePlayerForce = 5f;
        [SerializeField] private float baseDistantPlayerForce = 0.5f;
        [SerializeField] private float baseEnemyForce = -1f;

        private float playerForce;

        void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            body = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();

            initialLocalScale = transform.localScale;

            guid = Guid.NewGuid();
            name = "Enemy " + guid;

            _allEnemies.Add(this);
        }
        
        private void FixedUpdate()
        {
            ExecuteTree();
        }
        
        private Status ExecuteTree()
        {
            return
                bt.Sequencer(
                    () => UpdateBlackboards(actorBoard),
                    () => bt.Selector(
                        () => bt.ConditionalVoidActions(
                            () => actorBoard.playerDistance < 0.5f,
                            Status.RUNNING,
                            () => SetColor(Color.red),
                            () => StandAndFight(actorBoard)),
                        () => bt.ConditionalVoidActions(() => actorBoard.playerDistance < 2f, Status.RUNNING, 
                            () => SetColor(Color.red),
                            () => JumpTowards(actorBoard)),
                        () => bt.ConditionalVoidActions(() => actorBoard.playerDistance < 2.2f, Status.RUNNING, 
                            () => SetColor(Color.red),
                            () => MoveAndShot(actorBoard)),
                        () => bt.ConditionalVoidActions(() => actorBoard.playerDistance < 4f, Status.RUNNING, 
                            () => SetColor(Color.magenta),
                            () => MoveAndShot(actorBoard)),
                        () => bt.ConditionalVoidActions(() => actorBoard.playerDistance < 8f, Status.RUNNING, 
                            () => SetColor(Color.yellow),
                            () => Move(actorBoard)),
                        () => bt.VoidActions(Status.RUNNING, 
                            () => SetColor(Color.grey),
                            () => Stand(actorBoard))
                        )
                );
        }
        
        private Status UpdateBlackboards(ActorBoard board)
        {
            Vector3 playerWorldPos = player.transform.position;

            board.playerWorldPos = playerWorldPos;
            board.playerDistance = Vector3.Distance(playerWorldPos, transform.position);

            UpdateForce(board);

            return Status.SUCCESS;
        }

        private void StandAndFight(ActorBoard board)
        {
            transform.localScale = initialLocalScale * (1f + Mathf.Sin(Time.realtimeSinceStartup * 20) * 0.2f);
        }

        private void JumpTowards(ActorBoard board)
        {
            transform.localScale = initialLocalScale * 1.1f;

            playerForce = baseClosePlayerForce;
        }
        
        private void MoveAndShot(ActorBoard board)
        {
            transform.localScale = initialLocalScale * (1f + Mathf.Sin(Time.realtimeSinceStartup * 10) * 0.1f);

            playerForce = baseDistantPlayerForce;
        }

        private void Move(ActorBoard board)
        {
            transform.localScale = Vector3.Lerp(initialLocalScale, transform.localScale, 0.1f);

            playerForce = baseDistantPlayerForce;
        }

        private void Stand(ActorBoard board)
        {
            SetColor(Color.grey);
            
            playerForce = 0;
        }

        private void UpdateForce(ActorBoard board)
        {
            var force = (board.playerWorldPos - transform.position) * (playerForce * Time.deltaTime);

            foreach (var enemy in _allEnemies)
                if (enemy.guid != guid)
                {
                    var distance = enemy.transform.position - transform.position;
                    var emenyForce = distance * (baseEnemyForce / distance.sqrMagnitude);

                    force += emenyForce;
                }

            body.AddForce(force, ForceMode.VelocityChange);
        }

        private void SetColor(Color color)
        {
            GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        }
    }
}