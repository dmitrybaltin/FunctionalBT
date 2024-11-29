using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Profiling.Memory;
using Debug = UnityEngine.Debug;

namespace Baltin.FBT
{
    public class ActorBoard
    {
        public Vector3 PlayerWorldPos;
        public float PlayerDistance;

        public FbtNpcView View;

        public ActorBoard(FbtNpcView view)
        {
            View = view;
        }
    }

    public class MyLaconicFunctionalBt : LaconicFunctionalBt <ActorBoard>
    {
        public MyLaconicFunctionalBt(ActorBoard board) : base(board) { }
        
        public Status Execute()
        {
            return
                Selector(
                    _ => Action(
                        _ => SetColor(Color.grey)),
                    _ => VoidActions(Status.Success,
                        _ => Board.View.SetColor(Color.red)));
        }

        Status SetColor(Color color)
        {
            Board.View.SetColor(color);
            return Status.Failure;
        }
    }

    public class MyFunctionalBt2 : LightestFbt<MyFunctionalBt2>
    {
        
    }

    public class MyFbt : ExtendedFbt<ActorBoard>
    {
        public static void Execute(ActorBoard board)
        {
            Selector(board,
                static b => ConditionalVoidActions(b, Status.Running,
                    static b => b.PlayerDistance < 0.5f,
                    static b => b.View.SetColor(Color.red)),
                static b => ConditionalVoidActions(b, Status.Running,
                    static b => b.PlayerDistance < 0.5f,
                    static b => b.View.SetColor(Color.red)));
        }
    }

    [RequireComponent(typeof(Rigidbody))]
    public class FbtNpcView : MonoBehaviour
    {
        private FunctionalBtOld<ActorBoard> bt;

        private GameObject player;

        private ActorBoard actorBoard;
        
        private Rigidbody body;

        private Vector3 initialLocalScale;

        private Guid guid;

        private static List<FbtNpcView> _allEnemies = new();

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

            initialLocalScale = transform.localScale;

            guid = Guid.NewGuid();
            name = "Enemy " + guid;

            _allEnemies.Add(this);
        }
        
        public void Start()
        {
            var folderPath = Directory.GetParent(Application.dataPath).FullName + "/MemoryCaptures";

            MemoryProfiler.TakeSnapshot(
                Path.Combine(folderPath, $"MemorySnapshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_1.snap"),
                OnSnapshotFinished);

            for(var i=0;i<1000;i++)
                //ExecuteTree();
                MyFbt.Execute(actorBoard);
            
            MemoryProfiler.TakeSnapshot(
                Path.Combine(folderPath, $"MemorySnapshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_2.snap"),
                OnSnapshotFinished);
        }
        
        public Status ExecuteTree()
        {
            return
                bt.Selector(
                    static bt => bt.ConditionalVoidActions(
                        static bt => bt.Board.PlayerDistance < 0.5f,
                        Status.Running,
                        static bt => bt.Board.View.SetColor(Color.red)),
                    static bt => bt.ConditionalVoidActions(
                        static bt => bt.Board.PlayerDistance < 0.5f,
                        Status.Running,
                        static bt => bt.Board.View.SetColor(Color.red)));
        }
        
        private void FixedUpdate()
        {
            MyFbt.Execute(actorBoard);
            //ExecuteTree();
        }
        
        private void OnSnapshotFinished(string path, bool success)
        {
            if (success)
            {
                Debug.Log($"Memory snapshot created at: {path}");
            }
            else
            {
                Debug.LogError("Failed to create memory snapshot.");
            }
        }
        
        public Status ExplicitExecuteTree()
        {
            Func<FunctionalBtOld<ActorBoard>, bool> func = 
                new Func<FunctionalBtOld<ActorBoard>, bool>(bt => bt.Board.PlayerDistance < 0.5f);
            
            return
                bt.ConditionalVoidActions(
                    func,
                    Status.Running,
                    bt => bt.Board.View.SetColor(Color.red));
        }      
        
        public Status UpdateBlackboards(ActorBoard board)
        {
            Vector3 playerWorldPos = player.transform.position;

            board.PlayerWorldPos = playerWorldPos;
            board.PlayerDistance = Vector3.Distance(playerWorldPos, transform.position);

            return Status.Success;
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
            var force = (board.PlayerWorldPos - transform.position) * (playerForce * Time.deltaTime);

            foreach (var enemy in _allEnemies)
                if (enemy.guid != guid)
                {
                    var distance = enemy.transform.position - transform.position;
                    var emenyForce = distance * (baseEnemyForce / distance.sqrMagnitude);

                    force += emenyForce;
                }

            body.AddForce(force, ForceMode.VelocityChange);
            
            return Status.Success;
        }

        public void SetColor(Color color)
        {
            //GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        }
    }
}