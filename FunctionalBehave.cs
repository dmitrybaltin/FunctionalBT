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

        public FunctionalBehave go;

        public ActorBoard(FunctionalBehave go)
        {
            this.go = go;
        }
    }

    public class MyFunctionalBT : TynyBT<ActorBoard>
    {
        public MyFunctionalBT(ActorBoard board) : base(board)
        {}
        
    }

    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider))]
    public class FunctionalBehave : MonoBehaviour
    {
        private GameObject player;

        //private ActorBoard actorBoard;
        private MyFunctionalBT bt;
        
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
            //actorBoard = new ActorBoard(this);
            bt = new(new ActorBoard(this));
            
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

        public Status ExecuteTree()
        {
            return
                bt.Parallel(ParallelPolicy.REQUIRE_ONE_SUCCESS,
                    bt => bt.Board.go.UpdateBlackboards(bt.Board),
                    bt => bt.Selector(
                        bt => bt.ConditionalVoidActions(
                            bt => bt.Board.playerDistance < 0.5f,
                            Status.RUNNING,
                            bt => bt.Board.go.SetColor(Color.red),
                            bt => bt.Board.go.StandAndFight(bt.Board)),
                        bt => bt.ConditionalVoidActions(
                            bt => bt.Board.playerDistance < 2f, 
                            Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.red),
                            bt => bt.Board.go.JumpTowards(bt.Board)),
                        bt => bt.ConditionalVoidActions(bt => bt.Board.playerDistance < 2.2f, Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.red),
                            bt => bt.Board.go.MoveAndShot(bt.Board)),
                        bt => bt.ConditionalVoidActions(bt => bt.Board.playerDistance < 4f, Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.magenta),
                            bt => bt.Board.go.MoveAndShot(bt.Board)),
                        bt => bt.ConditionalVoidActions(bt => bt.Board.playerDistance < 8f, Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.yellow),
                            bt => bt.Board.go.Move(bt.Board)),
                        bt => bt.VoidActions(Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.grey),
                            bt => bt.Board.go.Stand(bt.Board))
                    ),
                    bt => bt.Board.go.UpdateForce(bt.Board)
                );
        }        
        
        public Status UpdateBlackboards(ActorBoard board)
        {
            Vector3 playerWorldPos = player.transform.position;

            board.playerWorldPos = playerWorldPos;
            board.playerDistance = Vector3.Distance(playerWorldPos, transform.position);

            return Status.SUCCESS;
        }

        public void StandAndFight(ActorBoard board)
        {
            transform.localScale = initialLocalScale * (1f + Mathf.Sin(Time.realtimeSinceStartup * 20) * 0.2f);
        }

        public void JumpTowards(ActorBoard board)
        {
            transform.localScale = initialLocalScale * 1.1f;

            playerForce = baseClosePlayerForce;
        }
        
        public void MoveAndShot(ActorBoard board)
        {
            transform.localScale = initialLocalScale * (1f + Mathf.Sin(Time.realtimeSinceStartup * 10) * 0.1f);

            playerForce = baseDistantPlayerForce;
        }

        public void Move(ActorBoard board)
        {
            transform.localScale = Vector3.Lerp(initialLocalScale, transform.localScale, 0.1f);

            playerForce = baseDistantPlayerForce;
        }

        public void Stand(ActorBoard board)
        {
            SetColor(Color.grey);
            
            playerForce = 0;
        }

        public Status UpdateForce(ActorBoard board)
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
            
            return Status.SUCCESS;
        }

        public void SetColor(Color color)
        {
            GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        }
    }
}