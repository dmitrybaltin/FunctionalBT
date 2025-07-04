using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Baltin.FBT
{
    /// <summary>
    /// Extended version of Functional Behaviour tree containing some additional convenient nodes
    /// </summary>
    /// <typeparam name="T">Blackboard type</typeparam>
    public static class ExtendedFbt
    {
        /// <summary>
        /// Action node, using Action<T> as a delegate to execute
        /// Always return a status given by 'returnStatus' argument  
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="returnStatus">Status to return</param>
        /// <param name="a">Delegate receiving T and returning void</param>
        /// <returns>The value of 'returnStatus'</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool VoidAction<T>(this T board, bool returnStatus, Action<T> a)
        {
            a.Invoke(board);
            return returnStatus;
        }

        /// <summary>
        /// Conditional action node, using Action<T> as a delegate to execute
        /// Always return a status given by returnStatus argument 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="returnStatus">Status to return after the action if the condition is true</param>
        /// <param name="condition"></param>
        /// <param name="a">Delegate receiving T and returning Status</param>
        /// <returns>If the condition is false then return Failure. Else return the value of 'returnStatus'</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool If<T>(this T board,
            bool returnStatus,
            Func<T, bool> condition, 
            Action<T> a)
        {
            if (!condition.Invoke(board)) 
                return false;
            
            a.Invoke(board);
            return returnStatus;
        }

        /// <summary>
        /// Conditional action node, using 'true' and 'false' actions to execute given as Action<T> delegates  
        /// Always return a status given by returnStatus argument 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="returnStatus">Status to return</param>
        /// <param name="condition"></param>
        /// <param name="a">Delegate receiving T and returning Status</param>
        /// <param name="elseA">Alternative action if the condition is false</param>
        /// <returns>The value of 'returnStatus'</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool If<T>(this T board,
            bool returnState,
            Func<T, bool> condition, 
            Action<T> a, 
            Action<T> elseA)
        {
            if (condition.Invoke(board))
                a.Invoke(board);
            else
                elseA.Invoke(board);
            
            return returnState;
        }        
        
        /// <summary>
        /// Conditional action node, using Action<T> as a delegate to execute
        /// Always return a status given by returnStatus argument 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition"></param>
        /// <param name="func">Delegate receiving T and returning bool</param>
        /// <returns>If the condition is false then return Failure. Else return the result of func converted to Status</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool If<T>(
            this T board,
            Func<T, bool> condition, 
            Func<T, bool> func)
        {
            return condition.Invoke(board) && func.Invoke(board);
        }

        /// <summary>
        /// Conditional action node, using 'true' and 'false' actions to execute given as Action<T> delegates  
        /// Always return a status given by returnStatus argument 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition"></param>
        /// <param name="func">Delegate receiving T and returning bool</param>
        /// <param name="elseFunc">Alternative action if the condition is false</param>
        /// <returns>The value of 'returnStatus'</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool If<T>(this T board,
            Func<T, bool> condition, 
            Func<T, bool> func, 
            Func<T, bool> elseFunc)
        {
            return condition.Invoke(board)
                ? func.Invoke(board)
                : elseFunc.Invoke(board);
        }        
        
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
        /// <returns>The value of 'returnStatus'</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool VoidActions<T>(this T board,
            bool returnStatus,
            Action<T> a1,
            Action<T> a2,
            Action<T> a3 = null,
            Action<T> a4 = null,
            Action<T> a5 = null,
            Action<T> a6 = null)
        {
            a1.Invoke(board);
            a2.Invoke(board);
            a3?.Invoke(board);
            a4?.Invoke(board);
            a5?.Invoke(board);
            a6?.Invoke(board);

            return returnStatus;
        }

        //Conditional nodes are syntax sugar that check condition before executing the action
        //Every conditional node can be replaced by two nodes the first of them is an Action node containing the condition and wrapping the second mains node 
        //But usually is more convenient to use one conditional node instead
        
        /// <summary>
        /// Check condition, execute given action and then return its inverted result 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="func">Action returning Status</param>
        /// <param name="elseFunc"></param>
        /// <returns>If the condition is false return Failure. Else return inverted value of the func</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> ConditionalInverter<T>(
            this T board,
            CancellationToken token,
            Func<T, bool> condition,
            Func<T, CancellationToken, UniTask<bool>> func, 
            Func<T, CancellationToken, UniTask<bool>> elseFunc = null) 
            =>  condition.Invoke(board) 
                ? await board.Inverter(token, func) 
                : elseFunc != null && await board.Inverter(token, elseFunc);
        
        /// <summary>
        /// Check condition before Selector
        /// Returns Failure if the condition is false
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns>If the condition is false return Failure. Else return the result of Selector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> ConditionalSelector<T>(
                this T board,
                CancellationToken token,
                Func<T, bool> condition,
                Func<T, CancellationToken, UniTask<bool>> f1,
                Func<T, CancellationToken, UniTask<bool>> f2,
                Func<T, CancellationToken, UniTask<bool>> f3 = null,
                Func<T, CancellationToken, UniTask<bool>> f4 = null,
                Func<T, CancellationToken, UniTask<bool>> f5 = null,
                Func<T, CancellationToken, UniTask<bool>> f6 = null) 
            => condition.Invoke(board) && await board.Selector(token, f1, f2, f3, f4, f5, f6);
        
        /// <summary>
        /// Check condition before Sequencer
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns>If the condition is false return Failure. Else return the result of Sequencer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> ConditionalSequencer<T>(this T board, CancellationToken token,
                Func<T, bool> condition, 
                Func<T, CancellationToken, UniTask<bool>> f1,
                Func<T, CancellationToken, UniTask<bool>> f2,
                Func<T, CancellationToken, UniTask<bool>> f3 = null,
                Func<T, CancellationToken, UniTask<bool>> f4 = null,
                Func<T, CancellationToken, UniTask<bool>> f5 = null,
                Func<T, CancellationToken, UniTask<bool>> f6 = null) 
            => condition.Invoke(board) && await board.Sequencer(token, f1, f2, f3, f4, f5, f6);
        
        /// <summary>
        /// Check condition and then Execute Void actions
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="returnStatus">Status to return at the end of all actions</param>
        /// <param name="a1">Delegate receiving T and returning Status</param>
        /// <param name="a2">Optional delegate receiving T and returning Status</param>
        /// <param name="a3">Optional delegate receiving T and returning Status</param>
        /// <param name="a4">Optional delegate receiving T and returning Status</param>
        /// <param name="a5">Optional delegate receiving T and returning Status</param>
        /// <param name="a6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConditionalVoidActions<T>(this T board,
            bool returnStatus, 
            Func<T, bool> condition, 
            Action<T> a1,
            Action<T> a2,
            Action<T> a3 = null,
            Action<T> a4 = null,
            Action<T> a5 = null,
            Action<T> a6 = null) 
            => condition.Invoke(board) && board.VoidActions(returnStatus, a1, a2, a3, a4, a5, a6);

#endif            
        
#if NET9_0_OR_GREATER
        /// <summary>
        /// Simplest action node, using set of Action<T> delegates as input
        /// All the children nodes are always executed 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="returnStatus">Status to return at the end of all the actions</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status VoidActions<T>(this T board, 
            Status returnStatus, 
            params ReadOnlySpan<Action<T>> funcs
            )
        {
            foreach (var f in funcs)
                f.Invoke(board);
            
            return returnStatus;
        }

        /// <summary>
        /// Check condition before Selector 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSelector<T>(this T board, 
            Func<T, bool> condition, 
            params ReadOnlySpan<Func<T, Status>> funcs
        ) => condition.Invoke(board)
            ? Selector(board, funcs)
            : Status.Failure;

        /// <summary>
        /// Check condition before Sequencer
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalSequencer<T>(this T board,
            Func<T, bool> condition, 
            params ReadOnlySpan<Func<T, Status>> funcs
        ) => condition.Invoke(board)
                ? Sequencer(board, funcs)
                : Status.Failure;

        /// <summary>
        /// Check condition and then Execute Void actions
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="returnStatus">Status to return at the end of all actions</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalVoidActions<T>(this T board, 
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
    }
}