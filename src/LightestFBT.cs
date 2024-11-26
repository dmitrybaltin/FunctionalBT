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
        /// <param name="board"></param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static Status Selector(
            T board, 
#if CSHARP13
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

#if !CSHARP13
        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="board"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="f3"></param>
        /// <param name="f4"></param>
        /// <param name="f5"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Selector(T board,
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null)
        {
            var s = f1?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f2?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f3?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f4?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;
            s = f5?.Invoke(board) ?? Status.Failure; if (s is Status.Running or Status.Success) return s;

            return s;
        }
#endif

        /// <summary>
        /// Classic sequencer node
        /// </summary>
        /// <param name="board"></param>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Sequencer(
            T board, 
#if CSHARP13
            params ReadOnlySpan<Func<T, Status>> funcs
#else
            params Func<T, Status>[] funcs
#endif
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
        
#if !CSHARP13
        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="board"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="f3"></param>
        /// <param name="f4"></param>
        /// <param name="f5"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Status Sequencer(T board,
            Func<T, Status> f1,
            Func<T, Status> f2,
            Func<T, Status> f3 = null,
            Func<T, Status> f4 = null,
            Func<T, Status> f5 = null)
        {
            var s = f1?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f2?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f3?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f4?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;
            s = f5?.Invoke(board) ?? Status.Success; if (s is Status.Running or Status.Failure) return s;

            return s;
        }
#endif
    }
}