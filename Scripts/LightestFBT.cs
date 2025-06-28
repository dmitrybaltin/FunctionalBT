using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine.Windows.Speech;

namespace Baltin.FBT
{
    public enum Status
    {
        Success = 0,
        Failure = 1,
        Running = 2,
    }

    public struct State
    {
        public Status Status;

        public UniTask UniTask;

        public State(Status status)
        {
            Status = status;
            UniTask = default;
        }
        public State(UniTask uniTask)
        {
            switch (uniTask.Status)
            {
                case UniTaskStatus.Succeeded:
                    Status = Status.Success;
                    UniTask = default;
                    return;
                case UniTaskStatus.Pending:
                    Status = Status.Running;
                    break;
                case UniTaskStatus.Canceled:
                case UniTaskStatus.Faulted:
                default:
                    Status = Status.Failure;
                    break;
            }
            UniTask = uniTask;
        }
        
        public static implicit operator Status(State status) => status.Status;
        public static State Failure => new State(Status.Failure);
        public static State Success => new State(Status.Success);
    }
    
    public static class StatusExtensions
    {
        /// <summary>
        /// Invert Status
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Invert(this State state)
        {
            return state.Status switch
            {
                Status.Failure => State.Success,
                Status.Success => State.Failure,
                _ => state,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State ToState(this bool value) => 
            value ? State.Success : State.Failure;
    }
    
    /// <summary>
    /// Functional Behavior Tree pattern
    /// 1. Convenient for debugging, since you can set breakpoint both on tree nodes and on delegates passed as parameters
    /// 2. Zero memory allocation, if you do not use closures in delegates
    ///    (to ensure which it is recommended to use the 'static' modifier before declaring the delegate)
    /// 3. Extremely fast, because here inside there are only the simplest conditions, loops and procedure calls
    /// </summary>
    /// <typeparam name="T">A type of 'blackboard' that is an interface to the data and behavior of the controlled object.</typeparam>
    public static class LightestFbt
    {
        /// <summary>
        /// Classic inverter node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="func">Delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Inverter<T>(this T board, Func<T, State> func)
            => func.Invoke(board).Invert();
        
        /// <summary>
        /// Execute the given func delegate if the given condition is true 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="func">Action to execute if condition is true. Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State If<T>(this T board, Func<T, bool> condition, Func<T, State> func) 
            => condition.Invoke(board) ? func.Invoke(board): State.Failure;

        /// <summary>
        /// Execute the given 'func' delegate if the given condition is true
        /// Else execute 'elseFunc' delegate
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="func">Action to execute if condition is true. Delegate receiving T and returning Status</param>
        /// <param name="elseFunc">Action to execute if condition is false. Delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State If<T>(this T board, Func<T, bool> condition, Func<T, State> func, Func<T, State> elseFunc) 
            => condition.Invoke(board) ? func.Invoke(board) : elseFunc.Invoke(board);

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
        /// <param name="f7">Optional delegate receiving T and returning Status</param>
        /// <param name="f8">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Selector<T>(this T board,
            Func<T, State> f1,
            Func<T, State> f2,
            Func<T, State> f3 = null,
            Func<T, State> f4 = null,
            Func<T, State> f5 = null,
            Func<T, State> f6 = null,
            Func<T, State> f7 = null,
            Func<T, State> f8 = null)
        {
            var s = f1?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return new State(s);
            s = f2?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return new State(s);
            s = f3?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return new State(s);
            s = f4?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return new State(s);
            s = f5?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return new State(s);
            s = f6?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return new State(s);
            s = f7?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return new State(s);
            s = f8?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return new State(s);

            return new State(s);
        }
        
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
        /// <param name="f7">Optional delegate receiving T and returning Status</param>
        /// <param name="f8">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static State Sequencer<T>(this T board,
            Func<T, State> f1,
            Func<T, State> f2,
            Func<T, State> f3 = null,
            Func<T, State> f4 = null,
            Func<T, State> f5 = null,
            Func<T, State> f6 = null,
            Func<T, State> f7 = null,
            Func<T, State> f8 = null)
        {
            var s = f1?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return new State(s);
            s = f2?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return new State(s);
            s = f3?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return new State(s);
            s = f4?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return new State(s);
            s = f5?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return new State(s);
            s = f6?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return new State(s);
            s = f7?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return new State(s);
            s = f8?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return new State(s);

            return new State(s);
        }

#endif

#if NET9_0_OR_GREATER
        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="funcs">Actions returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Selector<T>(this T board, 
            params ReadOnlySpan<Func<T, Status>> funcs
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

        /// <summary>
        /// Classic sequencer node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="funcs">Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Sequencer<T>(this T board, 
            params ReadOnlySpan<Func<T, Status>> funcs
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
    }
}