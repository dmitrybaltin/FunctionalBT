using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Codice.Utils;
using JetBrains.Annotations;

namespace Baltin.FBT
{
    /// <summary>
    /// Extended version of Functional Behaviour tree containing some additional convenient nodes
    /// </summary>
    /// <typeparam name="T">Blackboard type</typeparam>
    public class ExtendedFbt<T> : LightestFbt<T>
    {
        
#if !NET9_0_OR_GREATER
        /// <summary>
        /// Simplest action node, using set of Action<T> delegates as input
        /// All the children nodes are always executed 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="returnStatus">Status to return at the end of all the actions</param>
        /// <param name="a1">Delegate receiving T and returning Status</param>
        /// <param name="a2">Optional delegate receiving T and returning Status</param>
        /// <param name="a3">Optional delegate receiving T and returning Status</param>
        /// <param name="a4">Optional delegate receiving T and returning Status</param>
        /// <param name="a5">Optional delegate receiving T and returning Status</param>
        /// <param name="a6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status VoidActions(T board,
            Status returnStatus,
            Action<T> a1,
            Action<T> a2 = null,
            Action<T> a3 = null,
            Action<T> a4 = null,
            Action<T> a5 = null,
            Action<T> a6 = null)
        {
            a1.Invoke(board);
            a2?.Invoke(board);
            a3?.Invoke(board);
            a4?.Invoke(board);
            a5?.Invoke(board);
            a6?.Invoke(board);

            return returnStatus;
        }
#else
        /// <summary>
        /// Simplest action node, using set of Action<T> delegates as input
        /// All the children nodes are always executed 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="returnStatus">Status to return at the end of all the actions</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status VoidActions(T board, 
            Status returnStatus, 
            params ReadOnlySpan<Action<T>> funcs
            )
        {
            foreach (var f in funcs)
                f.Invoke(board);
            
            return returnStatus;
        }
#endif            
        
#region Conditional nodes
        //Conditional nodes are syntax sugar that check condition before executing the action
        //Every conditional node can be replaced by two nodes the first of them is an Action node containing the condition and wrapping the second mains node 
        //But usually is more convenient to use one conditional node instead
        
        /// <summary>
        /// Check condition before Secuencer
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="func">Action returning Status</param>
        /// <param name="elseFunc"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalInverter(T board,
            Func<T, bool> condition,
            Func<T, Status> func, 
            [CanBeNull] Func<T, Status> elseFunc = null) 
            =>  condition.Invoke(board) 
                ? Inverter(board, func) 
                : elseFunc != null ? Inverter(board, elseFunc) : Status.Failure;
        
#if !NET9_0_OR_GREATER
        /// <summary>
        /// Check condition before Selector 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSelector(T board,
            Func<T, bool> condition, 
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null,
            Func<T, Status> f6 = null) 
            => condition.Invoke(board)
                ? Selector(board, f1, f2, f3, f4, f5, f6)
                : Status.Failure;
#else
        /// <summary>
        /// Check condition before Selector 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSelector(T board, 
            Func<T, bool> condition, 
            params ReadOnlySpan<Func<T, Status>> funcs
        ) => condition.Invoke(board)
            ? Selector(board, funcs)
            : Status.Failure;
#endif

        
#if !NET9_0_OR_GREATER
        /// <summary>
        /// Check condition before Sequencer
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSequencer(T board,
            Func<T, bool> condition, 
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null,
            Func<T, Status> f6 = null) 
            => condition.Invoke(board)
                ? Sequencer(board, f1, f2, f3, f4, f5, f6)
                : Status.Failure;
#else
        /// <summary>
        /// Check condition before Sequencer
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSequencer(T board,
            Func<T, bool> condition, 
            params ReadOnlySpan<Func<T, Status>> funcs
        ) => condition.Invoke(board)
                ? Sequencer(board, funcs)
                : Status.Failure;
#endif

#if !NET9_0_OR_GREATER
        /// <summary>
        /// Check condition and then Execute Void actions
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="returnStatus">Status to return at the end of all actions</param>
        /// <param name="a1">Delegate receiving T and returning Status</param>
        /// <param name="a2">Optional delegate receiving T and returning Status</param>
        /// <param name="a3">Optional delegate receiving T and returning Status</param>
        /// <param name="a4">Optional delegate receiving T and returning Status</param>
        /// <param name="a5">Optional delegate receiving T and returning Status</param>
        /// <param name="a6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalVoidActions(T board,
            Func<T, bool> condition, 
            Status returnStatus, 
            Action<T> a1,
            Action<T> a2 = null,
            Action<T> a3 = null,
            Action<T> a4 = null,
            Action<T> a5 = null,
            Action<T> a6 = null) 
            => condition.Invoke(board)
                ? VoidActions(board, returnStatus, a1, a2, a3, a4, a5, a6)
                : Status.Failure;
#else
        /// <summary>
        /// Check condition and then Execute Void actions
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="returnStatus">Status to return at the end of all actions</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalVoidActions(T board, 
            Status returnStatus, 
            Func<T, bool> condition,
            params ReadOnlySpan<Action<T>> funcs
            )
        {
            return condition.Invoke(board) 
                ? VoidActions(board, returnStatus, funcs) 
                : Status.Failure;
        }
#endif
        
#endregion
    }
}