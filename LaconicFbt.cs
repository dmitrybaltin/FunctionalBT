using System;
using System.Runtime.CompilerServices;

namespace Baltin.FBT
{
    public class LaconicFbt<T>
    {
        public T Obj { get; set; }

        public LaconicFbt(T obj)
        {
            Obj = obj;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Action(Func<T, Status> func) =>
            StaticFbt<T>.Action(Obj, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Inverter(Func<T, Status> func) =>
            StaticFbt<T>.Inverter(Obj, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Selector(params Func<T, Status>[] funcs) => 
            StaticFbt<T>.Selector(Obj, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status StaticSelector(params Func<T, Status>[] funcs) => 
            StaticFbt<T>.StaticSelector(Obj, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status Sequencer(params Func<T, Status>[] funcs) => 
            StaticFbt<T>.Sequencer(Obj, funcs);

        public Status Parallel(ParallelPolicy policy, params Func<T, Status>[] funcs) => 
            StaticFbt<T>.Parallel(Obj, policy, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status VoidActions(Status returnStatus, params Action<T>[] funcs) => 
            StaticFbt<T>.VoidActions(Obj, returnStatus, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalVoidActions(Func<T, bool> condition, Status returnStatus, params Action<T>[] funcs) => 
            StaticFbt<T>.VoidActions(Obj, returnStatus, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalAction(Func<T, bool> condition, Func<T, Status> func) => 
            StaticFbt<T>.ConditionalAction(Obj, condition, func);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalInverter(Func<T, bool> condition, Func<T, Status> func) => 
            StaticFbt<T>.ConditionalInverter(Obj, condition, func);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSelector(Func<T, bool> condition, params Func<T, Status>[] funcs) => 
            StaticFbt<T>.ConditionalSelector(Obj, condition, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalSequencer(Func<T, bool> condition, params Func<T, Status>[] funcs) =>
            StaticFbt<T>.ConditionalSequencer(Obj, condition, funcs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Status ConditionalParallel(Func<T, bool> condition, ParallelPolicy policy, params Func<T, Status>[] funcs) => 
            StaticFbt<T>.ConditionalParallel(Obj, condition, policy, funcs);
    }
}