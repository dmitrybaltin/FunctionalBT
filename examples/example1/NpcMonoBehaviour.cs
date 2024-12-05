using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Baltin.FBT.Example
{
    [Serializable]
    public class NpcConfig
    {
        [SerializeField] public float baseClosePlayerForce = 5f;
        [SerializeField] public float baseDistantPlayerForce = 0.5f;
    }
    
    public class NpcFbt : ExtendedFbt<NpcBoard>
    {
        public static void Execute(NpcBoard b)
        {
            Sequencer(b,
                static b => b.PreUpdate(),
                static b => Selector(b,
                    static b => ConditionalSequencer(b,
                        static b => b.PlayerDistance < 1f,
                        static b => b.SetColor(Color.red),
                        static b => b.OscillateScale(1, 1.5f, 0.25f),
                        static b => b.StandAndFight()),
                    static b => ConditionalSequencer(b,
                        static b => b.PlayerDistance < 3f,
                        static b => b.SetColor(Color.red),
                        static b => b.SetScale(1f, 0.1f),
                        static b => b.JumpTowards()),
                    static b => ConditionalSequencer(b,
                        static b => b.PlayerDistance < 6f,
                        static b => b.SetColor(Color.magenta),
                        static b => b.OscillateScale(1, 1.2f, 0.5f),
                        static b => b.MoveTowardsAndShot()),
                    static b => ConditionalSequencer(b,
                        static b => b.PlayerDistance < 12f,
                        static b => b.SetColor(Color.yellow),
                        static b => b.SetScale(1f, 1f),
                        static b => b.MoveTowards()),
                    static b => b.SetColor(Color.grey),
                    static b => b.SetScale(1f, 1f)));
        }
    }

    public class NpcBoard
    {
        private NpcConfig config;
        
        public readonly int InstanceId; 
        
        public float PlayerDistance;

        private readonly Rigidbody body;
        private readonly MeshRenderer meshRenderer;
        private readonly Transform player; 
        
        private readonly Vector3 initialLocalScale;

        private Vector3 playerWorldPos;

        public NpcBoard(NpcConfig config, Rigidbody body, MeshRenderer meshRenderer, Transform player)
        {
            this.config = config;
            this.player = player;
            this.body = body;
            this.meshRenderer = meshRenderer;
           
            initialLocalScale = body.transform.localScale;
            
            InstanceId = body.GetInstanceID();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status PreUpdate()
        {
            playerWorldPos = player.position;
            PlayerDistance = Vector3.Distance(playerWorldPos, body.worldCenterOfMass);

            return Status.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status StandAndFight()
        {
            body.transform.localScale = initialLocalScale * (1f + Mathf.Sin(Time.realtimeSinceStartup * 20) * 0.2f);
            return Status.Running;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status MoveTowardsAndShot()
        {
            AddForce(config.baseDistantPlayerForce);
            return Status.Running;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status JumpTowards()
        {
            AddForce(config.baseClosePlayerForce);
            return Status.Running;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status MoveTowards()
        {
            AddForce(config.baseDistantPlayerForce);
            
            return Status.Running;
        }

        public Status OscillateScale(float fromScale, float toScale, float period)
        {
            if (period <= 0)
                throw new ArgumentException("Period must be greater than zero.", nameof(period));
            
            if (fromScale < 0 || toScale < 0)
                throw new ArgumentException("Scale values must be non-negative.");

            var triangleWave = Mathf.PingPong(Time.realtimeSinceStartup / period, 1f);

            body.transform.localScale = initialLocalScale * Mathf.Lerp(fromScale, toScale, triangleWave);

            return Status.Success;
        }

        public Status SetScale (float scale, float smoothTime)
        {
            if (scale < 0)
                throw new ArgumentException("Scale values must be non-negative.");

            if (smoothTime < Time.deltaTime)
                body.transform.localScale = initialLocalScale * scale;
            else
                body.transform.localScale = Vector3.Lerp(
                    body.transform.localScale,
                    initialLocalScale * scale,
                    Time.deltaTime/smoothTime);

            return Status.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status SetColor(Color color)
        {
            meshRenderer.material.SetColor("_Color", color);
            return Status.Success;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddForce(float playerForce)
        {
            var force = (playerWorldPos - body.worldCenterOfMass) * (playerForce * Time.deltaTime);
            body.AddForce(force, ForceMode.VelocityChange);
        }
    }
    
    
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider)), RequireComponent(typeof(MeshRenderer))]
    public class NpcMonoBehaviour : MonoBehaviour
    {
        [SerializeField] private NpcConfig config;

        private NpcBoard npcBoard;

        void Start()
        {
            Transform player = GameObject.FindGameObjectWithTag("Player").transform;
            
            npcBoard = new NpcBoard(
                config,
                GetComponent<Rigidbody>(), 
                GetComponent<MeshRenderer>(),
                player);
            name = "Npc " + npcBoard.InstanceId;
        }

        void Update()
        {
            NpcFbt.Execute(npcBoard);
        }
    }

}