using System;
using System.Runtime.CompilerServices;

namespace Baltin.FBT
{
    public enum Status
    {
        SUCCESS = 0,
        FAILURE = 1,
        RUNNING = 2,
    }

    public enum ParallelPolicy
    {
        REQUIRE_ONE_SUCCESS,
        REQUIRE_ALL_SUCCESS
    }

    public class BaseFbt<TBoard>
    {
        public TBoard Board { get; private set; }

        public BaseFbt(TBoard board)
        {
            Board = board;
        }

        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Action(Func<BaseFbt<TBoard>, Status> func)
        {
            return func.Invoke(this);
        }

        /// <summary>
        /// Classic inverter node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Inverter(Func<BaseFbt<TBoard>, Status> func)
        {
            return func.Invoke(this) switch
            {
                Status.FAILURE => Status.SUCCESS,
                Status.SUCCESS => Status.FAILURE,
                _ => Status.RUNNING,
            };
        }
        
        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Selector(params Func<BaseFbt<TBoard>, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(this);

                switch (childstatus)
                {
                    case Status.RUNNING:
                        return childstatus;
                    case Status.SUCCESS:
                        return childstatus;
                }
            }

            return Status.FAILURE;
        }

        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VoidActions(params Action<BaseFbt<TBoard>>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                f.Invoke(this);
        }

        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status StaticSelector(params Func<BaseFbt<TBoard>, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(this);

                switch (childstatus)
                {
                    case Status.RUNNING:
                        return childstatus;
                    case Status.SUCCESS:
                        return childstatus;
                }
            }

            return Status.FAILURE;
        }
        
        /// <summary>
        /// Classic sequencer node
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Sequencer(params Func<BaseFbt<TBoard>, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(this);

                switch (childstatus)
                {
                    case Status.RUNNING:
                        return childstatus;
                    case Status.FAILURE:
                        return childstatus;
                }
            }

            return Status.SUCCESS;
        }

        /// <summary>
        /// It is not really parallel. All the nodes will execute sequential.
        /// But all the child nodes always executes independent of theirs results
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public Status Parallel(ParallelPolicy policy, params Func<BaseFbt<TBoard>, Status>[] funcs)
        {
            var hasRunning = false;
            var hasSuccess = false;
            var hasFailure = false;

            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                switch (f.Invoke(this))
                {
                    case Status.RUNNING:
                        hasRunning = true;
                        break;
                    case Status.SUCCESS:
                        hasSuccess=true;
                        break;
                    case Status.FAILURE:
                        hasFailure=true;
                        break;
                }

            return policy switch
            {
                ParallelPolicy.REQUIRE_ALL_SUCCESS => 
                    hasFailure 
                        ? Status.FAILURE 
                        : hasRunning 
                            ? Status.RUNNING 
                            : Status.SUCCESS,
                
                ParallelPolicy.REQUIRE_ONE_SUCCESS => 
                    hasSuccess 
                        ? Status.SUCCESS 
                        : hasRunning 
                            ? Status.RUNNING 
                            : Status.FAILURE,
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /*    }

    public class AdvancedBT<TBoard> : TynyBT<TBoard>
    {*/
        
        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="func"></param>
        /// <param name="returnStatus"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status VoidActions(Status returnStatus, params Action<BaseFbt<TBoard>>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                f.Invoke(this);
            
            return returnStatus;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalVoidActions(Func<BaseFbt<TBoard>, bool> condition, Status returnStatus, params Action<BaseFbt<TBoard>>[] funcs)
        {
            return condition.Invoke(this) 
                ? VoidActions(returnStatus, funcs) 
                : Status.FAILURE;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalAction(Func<BaseFbt<TBoard>, bool> condition, Func<BaseFbt<TBoard>, Status> func)
        {
            return condition.Invoke(this) 
                ? Action(func) 
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalInverter(Func<BaseFbt<TBoard>, bool> condition, Func<BaseFbt<TBoard>, Status> func)
        {
            return condition.Invoke(this) 
                ? Inverter(func) 
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSelector(Func<BaseFbt<TBoard>, bool> condition, params Func<BaseFbt<TBoard>, Status>[] funcs)
        {
            return condition.Invoke(this)
                ? Selector(funcs)
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSequencer(Func<BaseFbt<TBoard>, bool> condition, params Func<BaseFbt<TBoard>, Status>[] funcs)
        {
            return condition.Invoke(this)
                ? Sequencer(funcs)
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalParallel(Func<BaseFbt<TBoard>, bool> condition, ParallelPolicy policy, params Func<BaseFbt<TBoard>, Status>[] funcs)
        {
            return condition.Invoke(this)
                ? Parallel(policy, funcs)
                : Status.FAILURE;
        }
    }

}