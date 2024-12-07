using System;
using System.Runtime.CompilerServices;
using PlasticPipe.Server;
using UnityEngine;

namespace Baltin.FBT.Example
{
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

    [Serializable]
    public class NpcConfig
    {
        [SerializeField] public float baseClosePlayerForce = 5f;
        [SerializeField] public float baseDistantPlayerForce = 0.5f;
    }
    
    public class NpcFbt : ExtendedFbt<NpcBoard>
    {
        public static void Execute(NpcBoard b) =>
            Sequencer(b,
                static b => b.PreUpdate(),
                static b => Selector(b,
                    static b => ConditionalSequencer(b, 
                        static b => b.PlayerDistance < 1f,
                        static b => b.SetColor(Color.red),
                        static b => b.OscillateScale(1, 1.5f, 0.25f),
                        static b => b.AddForce(b.Config.baseClosePlayerForce)),
                    static b => ConditionalSequencer(b, 
                        static b => b.PlayerDistance < 3f,
                        static b => b.SetColor(Color.magenta),
                        static b => b.SetScale(1f, 0.1f),
                        static b => b.AddForce(b.Config.baseClosePlayerForce)),
                    static b => ConditionalSequencer(b, 
                        static b => b.PlayerDistance < 8f,
                        static b => b.SetColor(Color.yellow),
                        static b => b.SetScale(1f, 1f),
                        static b => b.AddForce(b.Config.baseDistantPlayerForce)),
                    static b => b.SetColor(Color.grey),
                    static b => b.SetScale(1f, 1f)));
    }

    public class NpcBoard
    {
        private static readonly int ColorPropertyID = Shader.PropertyToID("_Color");
        
        public NpcConfig Config;
        
        public readonly int InstanceId; 
        
        public float PlayerDistance;

        private readonly Rigidbody _body;
        private readonly MeshRenderer _meshRenderer;
        private readonly Transform _player; 
        
        private readonly Vector3 _initialLocalScale;

        private Vector3 _playerWorldPos;

        public NpcBoard(NpcConfig config, Rigidbody body, MeshRenderer meshRenderer, Transform player)
        {
            this.Config = config;
            this._player = player;
            this._body = body;
            this._meshRenderer = meshRenderer;
           
            _initialLocalScale = body.transform.localScale;
            
            InstanceId = body.GetInstanceID();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status PreUpdate()
        {
            _playerWorldPos = _player.position;
            PlayerDistance = Vector3.Distance(_playerWorldPos, _body.worldCenterOfMass);

            return Status.Success;
        }

        public Status OscillateScale(float fromScale, float toScale, float period)
        {
            if (period <= 0)
                throw new ArgumentException("Period must be greater than zero.", nameof(period));
            
            if (fromScale < 0 || toScale < 0)
                throw new ArgumentException("Scale values must be non-negative.");

            var triangleWave = Mathf.PingPong(Time.realtimeSinceStartup / period, 1f);

            _body.transform.localScale = _initialLocalScale * Mathf.Lerp(fromScale, toScale, triangleWave);
            
            return Status.Success;
        }

        public Status SetScale (float scale, float smoothTime)
        {
            if (scale < 0)
                throw new ArgumentException("Scale values must be non-negative.");

            if (smoothTime < Time.deltaTime)
                _body.transform.localScale = _initialLocalScale * scale;
            else
                _body.transform.localScale = Vector3.Lerp(
                    _body.transform.localScale,
                    _initialLocalScale * scale,
                    Time.deltaTime/smoothTime);
            return Status.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status SetColor(Color color)
        {
            _meshRenderer.material.SetColor(ColorPropertyID, color);
            return Status.Success;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status AddForce(float playerForce)
        {
            var force = (_playerWorldPos - _body.worldCenterOfMass) * (playerForce * Time.deltaTime);
            _body.AddForce(force, ForceMode.VelocityChange);
            return Status.Success;
        }
    }
}