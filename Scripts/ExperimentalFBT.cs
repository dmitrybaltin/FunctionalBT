using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
/*
namespace Baltin.FBT
{
    /// <summary>
    /// Policy of parallel node
    /// Gives the rule of conversion the statuses of children nodes to teh overall status
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
    public static class ExperimentalFbt
    {
        /// <summary>
        /// Current status of the parallel node aggregating statuses of all the children nodes
        /// </summary>
        private struct ParallelStatus
        {
            /// <summary>
            /// True if one of nodes has returned Running status
            /// </summary>
            private bool _hasRunning;

            /// <summary>
            /// True if one of nodes has returned Success status
            /// </summary>
            private bool _hasSuccess;

            /// <summary>
            /// True if one of nodes has returned Failure status
            /// </summary>
            private bool _hasFailure;

            /// <summary>
            /// Add the status of an executed node to the overall status 
            /// </summary>
            /// <param name="status"></param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AddNodeStatus(Status status)
            {
                switch (status)
                {
                    case Status.Running:
                        _hasRunning = true;
                        break;
                    case Status.Success:
                        _hasSuccess = true;
                        break;
                    case Status.Failure:
                        _hasFailure = true;
                        break;
                }
            }

            /// <summary>
            /// Convert ParallelStatus to Status object using given ParallelPolicy
            /// </summary>
            /// <param name="policy">Gives the rules of conversion the statuses of children nodes to overall status</param>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Status ConvertToNodeStatus(ParallelPolicy policy)
            {
                return policy == ParallelPolicy.RequireAllSuccess
                    ? _hasFailure
                        ? Status.Failure
                        : _hasRunning
                            ? Status.Running
                            : Status.Success
                    : _hasSuccess
                        ? Status.Success
                        : _hasRunning
                            ? Status.Running
                            : Status.Failure;
            }
        }

#if! NET9_0_OR_GREATER
        /// <summary>
        /// It is not really parallel. All the nodes will execute sequential.
        /// But all the child nodes always executes independent of theirs results
        /// Return value depends on the policy parameter and the results of children nodes 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="policy">Gives the rules of conversion the statuses of children nodes to overall status</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        public static Status Parallel<T>(this T board, 
            ParallelPolicy policy, 
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null,
            Func<T, Status> f6 = null) 
        {
            var parallelStatus = new ParallelStatus();

            parallelStatus.AddNodeStatus(f1.Invoke(board));
            parallelStatus.AddNodeStatus(f2.Invoke(board));
            if(f3 is not null) parallelStatus.AddNodeStatus(f1.Invoke(board));
            if(f4 is not null) parallelStatus.AddNodeStatus(f4.Invoke(board));
            if(f5 is not null) parallelStatus.AddNodeStatus(f5.Invoke(board));
            if(f6 is not null) parallelStatus.AddNodeStatus(f6.Invoke(board));

            return parallelStatus.ConvertToNodeStatus(policy);
        }
#else
        /// <summary>
        /// It is not really parallel. All the nodes will execute sequential.
        /// But all the child nodes always executes independent of theirs results
        /// Return value depends on the policy parameter and the results of children nodes 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="policy">Gives the rules of conversion the statuses of children nodes to overall status</param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static Status Parallel<T>(this T board,ParallelPolicy policy,params ReadOnlySpan<Func<T, Status>> funcs)
        {
            var parallelStatus = new ParallelStatus();
            
            foreach (var f in funcs)
                parallelStatus.AddNodeStatus(f.Invoke(board));

            return parallelStatus.ConvertToNodeStatus(policy);
        }
#endif

#if! NET9_0_OR_GREATER
        /// <summary>
        /// Parallel node with precondition
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="policy">Gives the rules of conversion the statuses of children nodes to overall status</param>
        /// <param name="condition"></param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalParallel<T>(this T board,
            ParallelPolicy policy,
            Func<T, bool> condition,
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null,
            Func<T, Status> f6 = null) 
            => condition.Invoke(board)
                ? Parallel(board, policy, f1, f2, f3, f4, f5, f6)
                : Status.Failure;
#else
        /// <summary>
        /// Parallel node with precondition
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="policy"></param>
        /// <param name="condition"></param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalParallel<T>(this T board,
            ParallelPolicy policy,
            Func<T, bool> condition,
            params ReadOnlySpan<Func<T, Status>> funcs)
            => condition.Invoke(board)
                ? Parallel(board, policy, funcs)
                : Status.Failure;
#endif
    }
}*/