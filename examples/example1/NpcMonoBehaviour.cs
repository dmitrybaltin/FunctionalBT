using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using PlasticPipe.Server;
using UnityEngine;

namespace Baltin.FBT.Example
{
    /// <summary>
    /// NpcMonoBehaviour object that describe the NPC in scene and serve as a entry point for its behaviour 
    /// </summary>
    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Collider)), RequireComponent(typeof(MeshRenderer))]
    public class NpcMonoBehaviour : MonoBehaviour
    {
        [SerializeField] private NpcConfig config;

        /// <summary>
        /// Blackboard object that contains methods and data of controlled object
        /// </summary>
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
            //Behaviour tree is executed here and all the required NPC methods are called from Behavior Tree
            NpcFbt.Async(
                    npcBoard,
                    new CancellationTokenSource().Token)
                .Forget();
        }
    }

    [Serializable]
    public class NpcConfig
    {
        /// <summary>
        /// Multiplayer to a gravity force between the NPC and the player when them are close to each other  
        /// </summary>
        [SerializeField] public float baseClosePlayerForce = 5f;
        
        /// <summary>
        /// Multiplayer to a gravity force between the NPC and the player when them are not close to each other  
        /// </summary>
        [SerializeField] public float baseDistantPlayerForce = 0.5f;
    }
    
    /// <summary>
    /// Behavior Tree definition
    /// </summary>
    public class NpcFbt 
    {
        public static async UniTask Async(NpcBoard b, CancellationToken token) =>
            await b.Sequencer(    //Classic Sequencer node
                token,
                static async (b, c) => await b.PreUpdate(),  //The first child of Sequencer that is a classic Action node realized as a delegate Func<NpcBoard, Status> 
                static async (b, c) => await b.Selector(c,     //The first child of Sequencer is a Classic Selector node
                    static (b, c) => b.If(c,       //The first child of Sequencer a Classic Conditional node 
                        static b => b.PlayerDistance < 1f,  //Condition
                        static async (b, c) => await b.Sequencer(c,            //This Sequencer node is executed when the condition above is true 
                            static async (b, c) => await b.SetColor(Color.red),
                            static async (b, c) => await b.OscillateScale(1, 1.5f, 0.25f),
                            static async (b, c) => await b.AddForce(b.Config.baseClosePlayerForce))),
                    static async (b, c) => await b.ConditionalSequencer(c,     //Using ConditionalSequencer instead of If + Sequencer (see above)   
                        static b => b.PlayerDistance < 3f,
                        static async (b, c) => await b.SetColor(Color.magenta),
                        static async (b, c) => await b.SetScale(1f, 0.1f),
                        static async (b, c) => await b.AddForce(b.Config.baseClosePlayerForce)),
                    static async (b, c) => await b.ConditionalSequencer(c,         
                        static b => b.PlayerDistance < 8f,
                        static async (b, c) => await b.SetColor(Color.yellow),
                        static async (b, c) => await b.SetScale(1f, 1f),
                        static async (b, c) => await b.AddForce(b.Config.baseDistantPlayerForce)),
                    static async (b, c) => await b.SetColor(Color.grey), //One more action node realized as a delegate Func<NpcBoard, Status>
                    static async (b, c) => await b.SetScale(1f, 1f)));     //One more action node realized as a delegate Func<NpcBoard, Status>
    }

    /// <summary>
    /// Blackboard object that contains methods and data of controlled object
    /// </summary>
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
            Config = config;
            _player = player;
            _body = body;
            _meshRenderer = meshRenderer;
           
            _initialLocalScale = body.transform.localScale;
            
            InstanceId = body.GetInstanceID();
        }
        
        /// <summary>
        /// Some action at the beginning of execution
        /// Calculation the distance between NPC and the player
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<Status> PreUpdate()
        {
            _playerWorldPos = _player.position;
            PlayerDistance = Vector3.Distance(_playerWorldPos, _body.worldCenterOfMass);

            return UniTask.FromResult(Status.Success);
        }

        public UniTask<Status> OscillateScale(float fromScale, float toScale, float period)
        {
            if (period <= 0)
                throw new ArgumentException("Period must be greater than zero.", nameof(period));
            
            if (fromScale < 0 || toScale < 0)
                throw new ArgumentException("Scale values must be non-negative.");

            var triangleWave = Mathf.PingPong(Time.realtimeSinceStartup / period, 1f);

            _body.transform.localScale = _initialLocalScale * Mathf.Lerp(fromScale, toScale, triangleWave);
            
            return UniTask.FromResult(Status.Success);
        }

        public UniTask<Status> SetScale (float scale, float smoothTime)
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
            return UniTask.FromResult(Status.Success);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<Status> SetColor(Color color)
        {
            _meshRenderer.material.SetColor(ColorPropertyID, color);
            return UniTask.FromResult(Status.Success);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UniTask<Status> AddForce(float playerForce)
        {
            var force = (_playerWorldPos - _body.worldCenterOfMass) * (playerForce * Time.deltaTime);
            _body.AddForce(force, ForceMode.VelocityChange);
            return UniTask.FromResult(Status.Success);
        }
    }
}