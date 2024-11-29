using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Baltin.FBT
{
    /// <summary>
    /// 
    /// </summary>
    public enum ParallelPolicy
    {
        RequireOneSuccess,
        RequireAllSuccess
    }

    /// <summary>
    /// Here are some controversial nodes that I am not sure are necessary.
    /// </summary>
    /// <typeparam name="T">Blackboard type</typeparam>
    public class ExperimentalFbt<T> : ExtendedFbt<T> 
    {
        /// <summary>
        /// It is not really parallel. All the nodes will execute sequential.
        /// But all the child nodes always executes independent of theirs results
        /// Return value depends on the policy parameter and the results of children nodes 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static Status Parallel(
            T board, 
            ParallelPolicy policy, 
#if NET9_0_OR_GREATER
            params ReadOnlySpan<Func<T, Status>> funcs
#else
            params Func<T, Status>[] funcs
#endif
            )
        {
            var hasRunning = false;
            var hasSuccess = false;
            var hasFailure = false;

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

            return GetParallelStatus(policy, hasRunning, hasSuccess, hasFailure);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Status GetParallelStatus(ParallelPolicy policy, bool hasRunning, bool hasSuccess, bool hasFailure)
        {
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
        /// Parallel node with precondition
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="policy"></param>
        /// <param name="condition"></param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalParallel(T board,
            ParallelPolicy policy,
            Func<T, bool> condition,
#if NET9_0_OR_GREATER
            params ReadOnlySpan<Func<T, Status>> funcs
#else
            params Func<T, Status>[] funcs
#endif
            )
        {
            return condition.Invoke(board)
                ? Parallel(board, policy, funcs)
                : Status.Failure;
        }
    }
}