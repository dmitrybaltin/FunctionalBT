using System;
using System.Runtime.CompilerServices;

namespace Baltin.FBT
{
    public enum Status
    {
        Success = 0,
        Failure = 1,
        Running = 2,
    }

    public enum ParallelPolicy
    {
        RequireOneSuccess,
        RequireAllSuccess
    }

    /// <summary>
    /// The best attempt at implementing the functional behavior tree pattern achieved so far
    /// 1. Convenient for debugging, since you can set breakpoint both on tree nodes and on delegates passed as parameters
    /// 2. Zero memory allocation, if you do not use closures in delegates
    ///    (to ensure which it is recommended to use the 'static' modifier before declaring the delegate)
    /// 3. Extremely fast, because here inside there are only the simplest conditions, the simplest loops and procedure calls
    /// </summary>
    /// <typeparam name="T">A type of 'blackboard' that is an interface to the data and behavior of the controlled object.</typeparam>
    public class FunctionalBt<T>
    {
        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Action(T board, Func<T, Status> func)
        {
            return func.Invoke(board);
        }

        /// <summary>
        /// Classic inverter node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Inverter(T board, Func<T, Status> func)
        {
            return func.Invoke(board) switch
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
        public static Status Selector(T board, params Func<T, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(board);

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
        public static Status Sequencer(T board, params Func<T, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(board);

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
        /// Return value depends on the policy parameter and the results of children nodes 
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static Status Parallel(T board, ParallelPolicy policy, params Func<T, Status>[] funcs)
        {
            var hasRunning = false;
            var hasSuccess = false;
            var hasFailure = false;

            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                switch (f.Invoke(board))
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
        
        /// <summary>
        /// Simplest action node, using set of Action<TBoard> delegates as input
        /// All the children nodes are always executed 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="returnStatus"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status VoidActions(T board, Status returnStatus, params Action<T>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                f.Invoke(board);
            
            return returnStatus;
        }

        #region  Conditional nodes
        //Conditional nodes are syntax sugar that check condition before executing the action
        //Every conditional node can be replaced by two nodes the first of them is an Action node containing the condition and wrapping the second mains node 
        //But usually is more convenient to use one conditional node instead
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalAction(T board, Func<T, bool> condition, Func<T, Status> func)
        {
            return condition.Invoke(board) 
                ? Action(board, func) 
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalInverter(
            T board,
            Func<T, bool> condition,
            Func<T, Status> func)
        {
            return condition.Invoke(board) 
                ? Inverter(board, func) 
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSelector(
            T board, 
            Func<T, bool> condition, 
            params Func<T, Status>[] funcs)
        {
            return condition.Invoke(board)
                ? Selector(board)
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSequencer(
            T board,
            Func<T, bool> condition, 
            params Func<T, Status>[] funcs)
        {
            return condition.Invoke(board)
                ? Sequencer(board)
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalParallel(T board,
            ParallelPolicy policy,
            Func<T, bool> condition,
            params Func<T, Status>[] funcs)
        {
            return condition.Invoke(board)
                ? Parallel(board, policy, funcs)
                : Status.Failure;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalVoidActions(
            T board, 
            Status returnStatus, 
            Func<T, bool> condition,
            params Action<T>[] funcs)
        {
            return condition.Invoke(board) 
                ? VoidActions(board, returnStatus, funcs) 
                : Status.Failure;
        }
        
        #endregion
    }
}