using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Baltin.FBT
{
    /// <summary>
    /// Extended version of Functional Behaviour tree containing some additional convenient nodes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExtendedFbt<T> : LightestFbt<T>
    {
        /// <summary>
        /// Simplest action node, using set of Action<TBoard> delegates as input
        /// All the children nodes are always executed 
        /// </summary>
        /// <param name="func"></param>
        /// <param name="returnStatus"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status VoidActions(
            T board, 
            Status returnStatus, 
#if CSHARP13
            params ReadOnlySpan<Action<T>> funcs
#else
            params Action<T>[] funcs
#endif            
            )
        {
            foreach (var f in funcs)
                f.Invoke(board);
            
            return returnStatus;
        }
#if !CSHARP13
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status VoidActions(T board,
            Status returnStatus,
            Func<T, bool> condition,
            Action<T> a1,
            Action<T> a2,
            Action<T> a3 = null,
            Action<T> a4 = null,
            Action<T> a5 = null)
        {
            a1.Invoke(board);
            a2.Invoke(board);
            a3?.Invoke(board);
            a4?.Invoke(board);
            a5?.Invoke(board);
            
            return returnStatus;
        }
#endif
        
#region Conditional nodes
        //Conditional nodes are syntax sugar that check condition before executing the action
        //Every conditional node can be replaced by two nodes the first of them is an Action node containing the condition and wrapping the second mains node 
        //But usually is more convenient to use one conditional node instead
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="condition"></param>
        /// <param name="func"></param>
        /// <param name="elseFunc"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalAction(
            T board, 
            Func<T, bool> condition, 
            Func<T, Status> func,
            [CanBeNull] Func<T, Status> elseFunc = null) 
            => condition.Invoke(board) 
                ? Action(board, func) 
                : elseFunc != null ? Action(board, elseFunc) : Status.Failure;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="condition"></param>
        /// <param name="func"></param>
        /// <param name="elseFunc"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalInverter(
            T board,
            Func<T, bool> condition,
            Func<T, Status> func, 
            [CanBeNull] Func<T, Status> elseFunc = null) 
            =>  condition.Invoke(board) 
                ? Inverter(board, func) 
                : elseFunc != null ? Inverter(board, elseFunc) : Status.Failure;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="condition"></param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSelector(
            T board, 
            Func<T, bool> condition, 
#if CSHARP13
            params ReadOnlySpan<Func<T, Status>> funcs
#else
            params Func<T, Status>[] funcs
#endif
        ) => condition.Invoke(board)
                ? Selector(board, funcs)
                : Status.Failure;
        
#if !CSHARP13
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSelector(T board,
            Func<T, bool> condition, 
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null) 
            => condition.Invoke(board)
                ? Selector(board, f1, f2, f3, f4, f5)
                : Status.Failure;
#endif
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="condition"></param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSequencer(
            T board,
            Func<T, bool> condition, 
#if CSHARP13
            params ReadOnlySpan<Func<T, Status>> funcs
#else
            params Func<T, Status>[] funcs
#endif
        ) => condition.Invoke(board)
                ? Sequencer(board, funcs)
                : Status.Failure;

#if !CSHARP13
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSequencer(T board,
            Func<T, bool> condition, 
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null) 
            => condition.Invoke(board)
                ? Sequencer(board, f1, f2, f3, f4, f5)
                : Status.Failure;
#endif
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="returnStatus"></param>
        /// <param name="condition"></param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalVoidActions(
            T board, 
            Status returnStatus, 
            Func<T, bool> condition,
#if CSHARP13
            params ReadOnlySpan<Action<T>> funcs
#else
            params Action<T>[] funcs
#endif
            )
        {
            return condition.Invoke(board) 
                ? VoidActions(board, returnStatus, funcs) 
                : Status.Failure;
        }

#if !CSHARP13
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalVoidActions(T board,
            Func<T, bool> condition, 
            Status returnStatus, 
            Action<T> a1,
            Action<T> a2,
            Action<T> a3 = null,
            Action<T> a4 = null,
            Action<T> a5 = null) 
            => condition.Invoke(board)
                ? VoidActions(board, returnStatus, a1, a2, a3, a4, a5)
                : Status.Failure;
#endif
        
#endregion
    }
}