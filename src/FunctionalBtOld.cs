using System;
using System.Runtime.CompilerServices;

namespace Baltin.FBT
{
    /// <summary>
    /// Early attempt to realize Functional BT pattern
    /// This works, but I decided to use static functions to shorten the syntax of calling them.
    /// I plan to delete this class
    /// </summary>
    /// <typeparam name="TBoard"></typeparam>
    [Obsolete("Early attempt to realize Functional BT pattern. I plan to delete it")]
    public class FunctionalBtOld<TBoard>
    {
        public TBoard Board { get; private set; }

        public FunctionalBtOld(TBoard board)
        {
            Board = board;
        }

        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Action(Func<FunctionalBtOld<TBoard>, Status> func)
        {
            return func.Invoke(this);
        }

        /// <summary>
        /// Classic inverter node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Inverter(Func<FunctionalBtOld<TBoard>, Status> func)
        {
            return func.Invoke(this) switch
            {
                Status.Failure => Status.Success,
                Status.Success => Status.Failure,
                _ => Status.Running,
            };
        }
        
        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Selector(params Func<FunctionalBtOld<TBoard>, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(this);

                switch (childstatus)
                {
                    case Status.Running:
                        return childstatus;
                    case Status.Success:
                        return childstatus;
                }
            }

            return Status.Failure;
        }

        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void VoidActions(params Action<FunctionalBtOld<TBoard>>[] funcs)
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
        public Status StaticSelector(params Func<FunctionalBtOld<TBoard>, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(this);

                switch (childstatus)
                {
                    case Status.Running:
                        return childstatus;
                    case Status.Success:
                        return childstatus;
                }
            }

            return Status.Failure;
        }
        
        /// <summary>
        /// Classic sequencer node
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Sequencer(params Func<FunctionalBtOld<TBoard>, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(this);

                switch (childstatus)
                {
                    case Status.Running:
                        return childstatus;
                    case Status.Failure:
                        return childstatus;
                }
            }

            return Status.Success;
        }

        /// <summary>
        /// It is not really parallel. All the nodes will execute sequential.
        /// But all the child nodes always executes independent of theirs results
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public Status Parallel(ParallelPolicy policy, params Func<FunctionalBtOld<TBoard>, Status>[] funcs)
        {
            var hasRunning = false;
            var hasSuccess = false;
            var hasFailure = false;

            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                switch (f.Invoke(this))
                {
                    case Status.Running:
                        hasRunning = true;
                        break;
                    case Status.Success:
                        hasSuccess=true;
                        break;
                    case Status.Failure:
                        hasFailure=true;
                        break;
                }

            return policy switch
            {
                ParallelPolicy.RequireAllSuccess => 
                    hasFailure 
                        ? Status.Failure 
                        : hasRunning 
                            ? Status.Running 
                            : Status.Success,
                
                ParallelPolicy.RequireOneSuccess => 
                    hasSuccess 
                        ? Status.Success 
                        : hasRunning 
                            ? Status.Running 
                            : Status.Failure,
                
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
        public Status VoidActions(Status returnStatus, params Action<FunctionalBtOld<TBoard>>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                f.Invoke(this);
            
            return returnStatus;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalVoidActions(Func<FunctionalBtOld<TBoard>, bool> condition, Status returnStatus, params Action<FunctionalBtOld<TBoard>>[] funcs)
        {
            return condition.Invoke(this) 
                ? VoidActions(returnStatus, funcs) 
                : Status.Failure;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalAction(Func<FunctionalBtOld<TBoard>, bool> condition, Func<FunctionalBtOld<TBoard>, Status> func)
        {
            return condition.Invoke(this) 
                ? Action(func) 
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalInverter(Func<FunctionalBtOld<TBoard>, bool> condition, Func<FunctionalBtOld<TBoard>, Status> func)
        {
            return condition.Invoke(this) 
                ? Inverter(func) 
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSelector(Func<FunctionalBtOld<TBoard>, bool> condition, params Func<FunctionalBtOld<TBoard>, Status>[] funcs)
        {
            return condition.Invoke(this)
                ? Selector(funcs)
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSequencer(Func<FunctionalBtOld<TBoard>, bool> condition, params Func<FunctionalBtOld<TBoard>, Status>[] funcs)
        {
            return condition.Invoke(this)
                ? Sequencer(funcs)
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalParallel(Func<FunctionalBtOld<TBoard>, bool> condition, ParallelPolicy policy, params Func<FunctionalBtOld<TBoard>, Status>[] funcs)
        {
            return condition.Invoke(this)
                ? Parallel(policy, funcs)
                : Status.Failure;
        }
    }

}