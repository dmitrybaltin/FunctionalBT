using System;
using System.Runtime.CompilerServices;

namespace FunctionlBT
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

    public class TynyBT
    {
        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Action(Func<Status> func)
        {
            return func.Invoke();
        }

        /// <summary>
        /// Classic inverter node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Inverter(Func<Status> func)
        {
            return func.Invoke() switch
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
        public Status Selector(params Func<Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke();

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
        public Status Sequencer(params Func<Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke();

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
        public Status Parallel(ParallelPolicy policy, params Func<Status>[] funcs)
        {
            var hasRunning = false;
            var hasSuccess = false;
            var hasFailure = false;

            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                switch (f.Invoke())
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
    }

    public class AdvancedBT : TynyBT
    {
        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="func"></param>
        /// <param name="returnStatus"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status VoidActions(Status returnStatus, params Action[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                f.Invoke();
            
            return returnStatus;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalVoidActions(Func<bool> condition, Status returnStatus, params Action[] funcs)
        {
            return condition.Invoke() 
                ? VoidActions(returnStatus, funcs) 
                : Status.FAILURE;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalAction(Func<bool> condition, Func<Status> func)
        {
            return condition.Invoke() 
                ? Action(func) 
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalInverter(Func<bool> condition, Func<Status> func)
        {
            return condition.Invoke() 
                ? Inverter(func) 
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSelector(Func<bool> condition, params Func<Status>[] funcs)
        {
            return condition.Invoke()
                ? Selector(funcs)
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSequencer(Func<bool> condition, params Func<Status>[] funcs)
        {
            return condition.Invoke()
                ? Sequencer(funcs)
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalParallel(Func<bool> condition, ParallelPolicy policy, params Func<Status>[] funcs)
        {
            return condition.Invoke()
                ? Parallel(policy, funcs)
                : Status.FAILURE;
        }
    }

}