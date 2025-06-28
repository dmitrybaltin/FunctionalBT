using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        public static Status VoidAction<T>(this T board, Status returnStatus, Action<T> a)
        {
            a.Invoke(board);
            return returnStatus;
        }

        /// <summary>
        /// Action node, using Func<T, bool> as a delegate to execute
        /// Return Status debending on the result of delegate   
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="func">Delegate receiving T and returning bool</param>
        /// <returns>If 'func' return true then returns Success else return Failure</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status BoolAction<T>(this T board, Func<T, bool> func)
        {
            return func.Invoke(board) 
                ? Status.Success
                : Status.Failure;
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
        public static Status If<T>(this T board,
            Status returnStatus,
            Func<T, bool> condition, 
            Action<T> a)
        {
            if (!condition.Invoke(board)) 
                return Status.Failure;
            
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
        public static Status If<T>(this T board,
            Status returnStatus,
            Func<T, bool> condition, 
            Action<T> a, 
            Action<T> elseA)
        {
            if (condition.Invoke(board))
                a.Invoke(board);
            else
                elseA.Invoke(board);
            
            return returnStatus;
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
        public static Status If<T>(this T board,
            Func<T, bool> condition, 
            Func<T, bool> func)
        {
            return condition.Invoke(board)
                ? func.Invoke(board).ToStatus()
                : Status.Failure;
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
        public static Status If<T>(this T board,
            Func<T, bool> condition, 
            Func<T, bool> func, 
            Func<T, bool> elseFunc)
        {
            return condition.Invoke(board)
                ? func.Invoke(board).ToStatus()
                : elseFunc.Invoke(board).ToStatus();
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
        public static Status VoidActions<T>(this T board,
            Status returnStatus,
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
        public static Status ConditionalInverter<T>(this T board,
            Func<T, bool> condition,
            Func<T, Status> func, 
            Func<T, Status> elseFunc = null) 
            =>  condition.Invoke(board) 
                ? board.Inverter(func) 
                : elseFunc != null 
                    ? board.Inverter(elseFunc) 
                    : Status.Failure;
        
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
        public static Status ConditionalSelector<T>(this T board,
            Func<T, bool> condition, 
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null,
            Func<T, Status> f6 = null) 
            => condition.Invoke(board)
                ? board.Selector(f1, f2, f3, f4, f5, f6)
                : Status.Failure;
        
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
        public static Status ConditionalSequencer<T>(this T board,
            Func<T, bool> condition, 
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null,
            Func<T, Status> f6 = null) 
            => condition.Invoke(board)
                ? board.Sequencer(f1, f2, f3, f4, f5, f6)
                : Status.Failure;
        
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
        public static Status ConditionalVoidActions<T>(this T board,
            Status returnStatus, 
            Func<T, bool> condition, 
            Action<T> a1,
            Action<T> a2,
            Action<T> a3 = null,
            Action<T> a4 = null,
            Action<T> a5 = null,
            Action<T> a6 = null) 
            => condition.Invoke(board)
                ? board.VoidActions(returnStatus, a1, a2, a3, a4, a5, a6)
                : Status.Failure;

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