using System;
using System.Runtime.CompilerServices;

namespace Baltin.FBT
{
    /// <summary>
    /// A wrapper over the LightestFBT class, allowing to achieve the most concise behaviour tree structure.
    /// However, this is achieved with the help of closures, that is, dynamic memory allocation when each execution of each tree node.
    /// This class is saved to illustrate the desired appearance of the behaviour tree, which I will strive for in future versions of the pattern
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LaconicFunctionalBt<T>
    {
        public T Board { get; set; }

        public LaconicFunctionalBt(T board)
        {
            Board = board;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Action(Func<T, Status> func) =>
            LightestFbt<T>.Action(Board, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Inverter(Func<T, Status> func) =>
            LightestFbt<T>.Inverter(Board, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Selector(params Func<T, Status>[] funcs) => 
            LightestFbt<T>.Selector(Board, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Sequencer(params Func<T, Status>[] funcs) => 
            LightestFbt<T>.Sequencer(Board, funcs);

        public Status Parallel(ParallelPolicy policy, params Func<T, Status>[] funcs) => 
            ExperimentalFbt<T>.Parallel(Board, policy, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status VoidActions(Status returnStatus, params Action<T>[] funcs) => 
            ExperimentalFbt<T>.VoidActions(Board, returnStatus, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalVoidActions(Status returnStatus, Func<T, bool> condition, params Action<T>[] funcs) => 
            ExperimentalFbt<T>.ConditionalVoidActions(Board, returnStatus, condition, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalAction(Func<T, bool> condition, Func<T, Status> func) => 
            ExperimentalFbt<T>.ConditionalAction(Board, condition, func);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalInverter(Func<T, bool> condition, Func<T, Status> func) => 
            ExperimentalFbt<T>.ConditionalInverter(Board, condition, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSelector(Func<T, bool> condition, params Func<T, Status>[] funcs) => 
            ExperimentalFbt<T>.ConditionalSelector(Board, condition, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSequencer(Func<T, bool> condition, params Func<T, Status>[] funcs) =>
            ExperimentalFbt<T>.ConditionalSequencer(Board, condition, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalParallel(ParallelPolicy policy, Func<T, bool> condition, params Func<T, Status>[] funcs) => 
            ExperimentalFbt<T>.ConditionalParallel(Board, policy, condition, funcs);
    }
}