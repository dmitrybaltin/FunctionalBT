using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Profiling.Memory;
using Debug = UnityEngine.Debug;

namespace Baltin.FBT
{
    public class ActorObject
    {
        public Vector3 playerPos;
        public Vector3 playerWorldPos;
        public float playerDistance;
        public bool playerInRange;
        public bool engaged;
        public bool foo;

        public FbtExamples go;

        public ActorObject(FbtExamples go)
        {
            this.go = go;
        }
    }

    public class MyLaconicFBT : LaconicFbt <ActorObject>
    {
        public MyLaconicFBT(ActorObject obj) : base(obj) { }
        
        public Status Execute()
        {
            return
                Selector(
                    _ => Action(
                        _ => SetColor(Color.grey)),
                    _ => VoidActions(Status.SUCCESS,
                        _ => Obj.go.SetColor(Color.red)));
        }

        Status SetColor(Color color)
        {
            Obj.go.SetColor(color);
            return Status.FAILURE;
        }
    }
    
    public class MyStaticFbt : StaticFbt<ActorObject>
    {
        private ActorObject obj;

        public MyStaticFbt(ActorObject @object)
        {
            obj = @object; 
        }
        
        public Status Execute()
        {
            return
                Selector(obj,
                    static o => ConditionalVoidActions(o, 
                        static o => o.playerDistance < 0.5f,
                        Status.RUNNING,
                        static o => o.go.SetColor(Color.red)),
                    static o => ConditionalVoidActions(o,
                        static o => o.playerDistance < 0.5f,
                        Status.RUNNING,
                        static o => o.go.SetColor(Color.red)));
        }
    }

    [RequireComponent(typeof(Rigidbody))]
    public class FbtExamples : MonoBehaviour
    {
        private BaseFbt<ActorObject> bt;

        public Status ExecuteTree()
        {
            return
                bt.Selector(
                    static bt => bt.ConditionalVoidActions(
                        static bt => bt.Board.playerDistance < 0.5f,
                        Status.RUNNING,
                        static bt => bt.Board.go.SetColor(Color.red)),
                    static bt => bt.ConditionalVoidActions(
                        static bt => bt.Board.playerDistance < 0.5f,
                        Status.RUNNING,
                        static bt => bt.Board.go.SetColor(Color.red)));
        }
        
        private GameObject player;

        //private ActorObject actorBoard;
        private MyStaticFbt sbt;
        
        private Rigidbody body;

        private Vector3 initialLocalScale;

        private Guid guid;

        private static List<FbtExamples> _allEnemies = new();

        [SerializeField] private float baseClosePlayerForce = 5f;
        [SerializeField] private float baseDistantPlayerForce = 0.5f;
        [SerializeField] private float baseEnemyForce = -1f;

        private float playerForce;

        void Awake()
        {
            //actorBoard = new ActorObject(this);
            bt = new(new ActorObject(this));
            
            player = GameObject.FindGameObjectWithTag("Player");
            body = GetComponent<Rigidbody>();

            initialLocalScale = transform.localScale;

            guid = Guid.NewGuid();
            name = "Enemy " + guid;

            _allEnemies.Add(this);
        }
        
        private void FixedUpdate()
        {
            ExecuteTree();
        }
        
        public void Start()
        {
            var folderPath = Directory.GetParent(Application.dataPath).FullName + "/MemoryCaptures";

            MemoryProfiler.TakeSnapshot(
                Path.Combine(folderPath, $"MemorySnapshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_1.snap"),
                OnSnapshotFinished);

            for(var i=0;i<1000;i++)
                ExecuteTree();
            
            MemoryProfiler.TakeSnapshot(
                Path.Combine(folderPath, $"MemorySnapshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_2.snap"),
                OnSnapshotFinished);
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
            Func<BaseFbt<ActorObject>, bool> func = 
                new Func<BaseFbt<ActorObject>, bool>(bt => bt.Board.playerDistance < 0.5f);
            
            return
                bt.ConditionalVoidActions(
                    func,
                    Status.RUNNING,
                    bt => bt.Board.go.SetColor(Color.red));
        }      
        
        public Status UpdateBlackboards(ActorObject @object)
        {
            Vector3 playerWorldPos = player.transform.position;

            @object.playerWorldPos = playerWorldPos;
            @object.playerDistance = Vector3.Distance(playerWorldPos, transform.position);

            return Status.SUCCESS;
        }

        public void StandAndFight(ActorObject @object)
        {
            transform.localScale = initialLocalScale * (1f + Mathf.Sin(Time.realtimeSinceStartup * 20) * 0.2f);
        }

        public void JumpTowards(ActorObject @object)
        {
            transform.localScale = initialLocalScale * 1.1f;

            playerForce = baseClosePlayerForce;
        }
        
        public void MoveAndShot(ActorObject @object)
        {
            transform.localScale = initialLocalScale * (1f + Mathf.Sin(Time.realtimeSinceStartup * 10) * 0.1f);

            playerForce = baseDistantPlayerForce;
        }

        public void Move(ActorObject @object)
        {
            transform.localScale = Vector3.Lerp(initialLocalScale, transform.localScale, 0.1f);

            playerForce = baseDistantPlayerForce;
        }

        public void Stand(ActorObject @object)
        {
            SetColor(Color.grey);
            
            playerForce = 0;
        }

        public Status UpdateForce(ActorObject @object)
        {
            var force = (@object.playerWorldPos - transform.position) * (playerForce * Time.deltaTime);

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
            //GetComponent<MeshRenderer>().material.SetColor("_Color", color);
        }
    }
}