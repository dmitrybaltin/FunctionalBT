using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Windows.Speech;

namespace Baltin.FBT
{
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
        public static async UniTask<bool> Inverter<T>(
            this T board, 
            CancellationToken ct, 
            Func<T, CancellationToken, UniTask<bool>> func)
            => !await func.Invoke(board, ct);

        /// <summary>
        /// Execute the given func delegate if the given condition is true 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="ct"></param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="func">Action to execute if condition is true. Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> If<T>(
            this T board, 
            CancellationToken ct, 
            Func<T, bool> condition, 
            Func<T, CancellationToken, UniTask<bool>> func) 
            => condition.Invoke(board) && await func.Invoke(board, ct);

        /// <summary>
        /// Execute the given 'func' delegate if the given condition is true
        /// Else execute 'elseFunc' delegate
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="ct"></param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="func">Action to execute if condition is true. Delegate receiving T and returning Status</param>
        /// <param name="elseFunc">Action to execute if condition is false. Delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> If<T>(
            this T board, 
            CancellationToken ct, 
            Func<T, bool> condition, 
            Func<T, CancellationToken, UniTask<bool>> func, 
            Func<T, CancellationToken, UniTask<bool>> elseFunc) 
            => condition.Invoke(board) ? await func.Invoke(board, ct) : await elseFunc.Invoke(board, ct);

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
        /// <param name="token"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> Selector<T>(this T board, CancellationToken token,
            Func<T, CancellationToken, UniTask<bool>> f1,
            Func<T, CancellationToken, UniTask<bool>> f2,
            Func<T, CancellationToken, UniTask<bool>> f3 = null,
            Func<T, CancellationToken, UniTask<bool>> f4 = null,
            Func<T, CancellationToken, UniTask<bool>> f5 = null,
            Func<T, CancellationToken, UniTask<bool>> f6 = null,
            Func<T, CancellationToken, UniTask<bool>> f7 = null,
            Func<T, CancellationToken, UniTask<bool>> f8 = null)
        {
            var s = f1 is not null && await f1.Invoke(board, token); if(s) return true;
            s = f2 is not null && await f2.Invoke(board, token); if(s) return true;
            s = f3 is not null && await f3.Invoke(board, token); if(s) return true;
            s = f4 is not null && await f4.Invoke(board, token); if(s) return true;
            s = f5 is not null && await f5.Invoke(board, token); if(s) return true;
            s = f6 is not null && await f6.Invoke(board, token); if(s) return true;
            s = f7 is not null && await f7.Invoke(board, token); if(s) return true;
            s = f8 is not null && await f8.Invoke(board, token); if(s) return true;

            return false;

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
        public static async UniTask<bool> Sequencer<T>(this T board, CancellationToken token,
            Func<T, CancellationToken, UniTask<bool>> f1,
            Func<T, CancellationToken, UniTask<bool>> f2,
            Func<T, CancellationToken, UniTask<bool>> f3 = null,
            Func<T, CancellationToken, UniTask<bool>> f4 = null,
            Func<T, CancellationToken, UniTask<bool>> f5 = null,
            Func<T, CancellationToken, UniTask<bool>> f6 = null,
            Func<T, CancellationToken, UniTask<bool>> f7 = null,
            Func<T, CancellationToken, UniTask<bool>> f8 = null)
        {
            var s = f1 is not null && await f1.Invoke(board, token); if(!s) return false;
            s = f2 is not null && await f2.Invoke(board, token); if(!s) return false;
            s = f3 is not null && await f3.Invoke(board, token); if(!s) return false;
            s = f4 is not null && await f4.Invoke(board, token); if(!s) return false;
            s = f5 is not null && await f5.Invoke(board, token); if(!s) return false;
            s = f6 is not null && await f6.Invoke(board, token); if(!s) return false;
            s = f7 is not null && await f7.Invoke(board, token); if(!s) return false;
            s = f8 is not null && await f8.Invoke(board, token); if(!s) return false;
            
            return true;
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