using System;
using System.Runtime.CompilerServices;

namespace Baltin.FBT
{
    public class StaticFbt<TBoard>
    {
        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Action(TBoard board, Func<TBoard, Status> func)
        {
            return func.Invoke(board);
        }

        /// <summary>
        /// Classic inverter node
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Inverter(TBoard board, Func<TBoard, Status> func)
        {
            return func.Invoke(board) switch
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
        public static Status Selector(TBoard board, params Func<TBoard, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(board);

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
        public static Status StaticSelector(TBoard board, params Func<TBoard, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(board);

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
        public static Status Sequencer(TBoard board, params Func<TBoard, Status>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
            {
                var childstatus = f.Invoke(board);

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
        public static Status Parallel(TBoard board, ParallelPolicy policy, params Func<TBoard, Status>[] funcs)
        {
            var hasRunning = false;
            var hasSuccess = false;
            var hasFailure = false;

            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                switch (f.Invoke(board))
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
        
        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="func"></param>
        /// <param name="returnStatus"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status VoidActions(TBoard board, Status returnStatus, params Action<TBoard>[] funcs)
        {
            if (funcs is null)
                throw new ArgumentNullException(nameof(funcs));
            
            foreach (var f in funcs)
                f.Invoke(board);
            
            return returnStatus;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalVoidActions(TBoard board, Func<TBoard, bool> condition, Status returnStatus, params Action<TBoard>[] funcs)
        {
            return condition.Invoke(board) 
                ? VoidActions(board, returnStatus, funcs) 
                : Status.FAILURE;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalAction(TBoard board, Func<TBoard, bool> condition, Func<TBoard, Status> func)
        {
            return condition.Invoke(board) 
                ? Action(board, func) 
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalInverter(TBoard board, Func<TBoard, bool> condition, Func<TBoard, Status> func)
        {
            return condition.Invoke(board) 
                ? Inverter(board, func) 
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSelector(TBoard board, Func<TBoard, bool> condition, params Func<TBoard, Status>[] funcs)
        {
            return condition.Invoke(board)
                ? Selector(board)
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSequencer(TBoard board, Func<TBoard, bool> condition, params Func<TBoard, Status>[] funcs)
        {
            return condition.Invoke(board)
                ? Sequencer(board)
                : Status.FAILURE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalParallel(TBoard board, Func<TBoard, bool> condition, ParallelPolicy policy, params Func<TBoard, Status>[] funcs)
        {
            return condition.Invoke(board)
                ? Parallel(board, policy, funcs)
                : Status.FAILURE;
        }
    }
}