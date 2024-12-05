using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Baltin.FBT
{
    public enum Status
    {
        Success = 0,
        Failure = 1,
        Running = 2,
    }

    /// <summary>
    /// The best attempt at implementing the functional behavior tree pattern achieved so far
    /// 1. Convenient for debugging, since you can set breakpoint both on tree nodes and on delegates passed as parameters
    /// 2. Zero memory allocation, if you do not use closures in delegates
    ///    (to ensure which it is recommended to use the 'static' modifier before declaring the delegate)
    /// 3. Extremely fast, because here inside there are only the simplest conditions, the simplest loops and procedure calls
    /// </summary>
    /// <typeparam name="T">A type of 'blackboard' that is an interface to the data and behavior of the controlled object.</typeparam>
    public class LightestFbt<T>
    {
        /// <summary>
        /// Classic Action node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="func">Delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Action(T board, Func<T, Status> func) 
            => func.Invoke(board);

        /// <summary>
        /// Classic inverter node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="func">Delegate receiving T and returning Status</param>
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
        /// Check condition before Action
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="func">Action if condition is true. Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalAction(T board, 
            Func<T, bool> condition, 
            Func<T, Status> func) 
            => condition.Invoke(board) ? Action(board, func) : Status.Failure;

        /// <summary>
        /// Check condition before Action
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="func">Action if condition is true. Delegate receiving T and returning Status</param>
        /// <param name="elseFunc">Action if condition is false. Delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status ConditionalAction(T board, 
            Func<T, bool> condition, 
            Func<T, Status> func,
            Func<T, Status> elseFunc) 
            => condition.Invoke(board) ? Action(board, func) : Action(board, elseFunc);

        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="funcs">Actions returning Status</param>
        /// <returns></returns>
        public static Status Selector(T board, 
#if NET9_0_OR_GREATER
            params ReadOnlySpan<Func<T, Status>> funcs
#else
            params Func<T, Status>[] funcs
#endif
            )
        {
            foreach (var f in funcs)
            {
                var childStatus = f?.Invoke(board) ?? Status.Failure;
                if(childStatus is Status.Running or Status.Success) 
                    return childStatus;
            }
            return Status.Failure;
        }

#if !NET9_0_OR_GREATER
        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Selector(T board,
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null,
            Func<T, Status> f6 = null)
        {
            var s = f1?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f2?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f3?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f4?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f5?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f6?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;

            return s;
        }
#else
        /// <summary>
        /// Classic sequencer node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Sequencer(T board, 
            params Func<T, Status>[] funcs
            )
        {
            foreach (var f in funcs)
            {
                var childStatus = f?.Invoke(board) ?? Status.Success;
                if (childStatus is Status.Running or Status.Failure)
                    return childStatus;
            }

            return Status.Success;
        }
#endif
        
#if !NET9_0_OR_GREATER
        /// <summary>
        /// Classic sequencer node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Sequencer(T board,
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null,
            Func<T, Status> f6 = null)
        {
            var s = f1?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f2?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f3?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f4?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f5?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f6?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;

            return s;
        }
#endif
    }
}