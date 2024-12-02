# Functional Behavior Tree in C#

Highly efficient, functional-style implementation of behavior tree in C# designed to enable clear AI logic, convenient debugging, and fast execution with zero memory allocation.

The codebase is minimal, simple, and easy to understand. Once familiarized with the approach, you can quickly reproduce or adapt it.  
Rather than a library, this solution is better described as a design pattern.

The implementation has no strict dependency on Unity and can be integrated into any C# project.

# Overview

## Key Features

1. **Clear and Concise Behavior Tree Definition**  
   Behavior trees are defined directly in code using functions calls and lambda expressions, resulting in clear and compact logic.   

2. **Ease of Debugging**  
   The tree definition and execution code are the same, which means you can place breakpoints inside any anonymous delegate, and they will work correctly.  
   No special complex "behaviour tree debugger" is required, you will use your favorite C# IDE.

3. **Zero memory allocation**  
   No memory is allocated for the tree structure because it is embedded directly into the code.  
   No memory is allocated for delegate instances, thanks to the use of static anonymous delegates.  
   No memory is allocated for transferring parameters to functions due to the use of 'params Collection' arguments (in C# 13) or functions with predefined argument sets (in earlier versions of C#) instead of 'params arrays'.

4. **High speed**  
   The implementation relies solely on function invocations, static delegates, conditional expressions, and loops.  
   No expensive features are used (e.g., garbage collection, hash tables, closures, etc.).
  
5. **Minimal and highly readable code**  
   The simplest version of the library, containing only classic nodes, is just a single .cs file with 160 lines, including comments.

## Usage Example

Detailed example is inside the file 'FunctionalBehave.cs'. Here’s a core of it, the tree definition.  
```csharp
public class NpcFbt : ExtendedFbt<NpcBoard>
{
    public static void Execute(NpcBoard b)
    {
        ConditionalAction(b,
            static b => !b.IsRagdollEnabled,
            static b => ConditionalAction(b,
                static b => !b.IsAttacking,
                static b => Selector(b,
                    static b => Sequencer(b, 
                        static b => b.FindTarget(),
                        static b => Selector(b,
                            static b => b.Attack(), 
                            static b => ConditionalAction(b,
                                static b => b.NavigateRequired(),
                                static b => b.Move()))),
                    static bd => bd.Idle())));
    }
}
```
This tree definition implements a simple behavior for an NPC that moves to the player if they are within the NPC's sight, then attacks. Key points to note:  
1. **Classes**
   1. **NpcBoard** is a custom class created by the user. It serves as the **blackboard** object and class contains the data and methods related to the NPC, which are controlled by the NpcFbt behavior tree.
   1. **NpcFbt** is a custom class responsible for implementing the AI logic for this NPC. Derived from **ExtendedFbt**. It contains only one static function Execute() and does not contain any data fields.
   1. **NpcBoard** instance is stored in an external container (e.g., a MonoBehaviour in Unity), which calls **NpcBoard.Execute()** during the update cycle.
1. **Methods**
   1. **Action()**, **Sequencer()**, **Selector()**, and **ConditionalAction()** are static methods of the ExtendedFbt class, implemented by this library.
   1. **b.FindTarget()**, **b.Attack()**, **b.Move()**, and **b.Idle()** are defined by the user inside NpcBoard class.
1. **Memory allocation**
   1. **static** modifier before anonymous delegates guarantee avoiding closures, making them extremely efficient in terms of memory usage because no memory allocation required when using these delegates.
      1. In each lambda function, only a single internal variable, **b**, is used, and there are no closures.
      2. All **b** variables point to the same **NpcBoard** instance received as an argument, but they are independent variables.
      3. Every tree node function receives the blackboard as a first parameter and forwards it to child nodes through delegates.
      4. Any accidental reference to a variable from a different lambda function would create a closure, causing memory allocation, but the **static** modifier prevents such situations.
   1. Functions with multiple arguments (**Selector**, **Sequence**, etc) avoid using **params arrays** definition that's why no memory allocated for these calls.

As shown in the example code, this implementation is extremely simple, zero allocation and fast and focused purely on logic, making it easy to debug. You can set breakpoints on any anonymous delegate or tree node function. When the execution reaches these breakpoints, the debugger will pause correctly, allowing you to inspect the state at that point.
Here is an illustration of breakpoints in the code:
![Example of debugging](functional_bt_example.jpg)

Below is an example implementation of a classic behavior tree node: the Selector.

```csharp
        public static Status Selector(T board, params ReadOnlySpan<Func<T, Status>> funcs)
        {
            foreach (var f in funcs)
            {
                var childStatus = f?.Invoke(board) ?? Status.Failure;
                if(childStatus is Status.Running or Status.Success) 
                    return childStatus;
            }
            return Status.Failure;
        }
```
As you can see, the code is minimal and contains only the core logic. Each behavior tree node is a static function rather than an object, reducing overhead.

### Note
The provided **Selector()** function leverages C# 13 features - the **params Collections** - to pass multiple input arguments using **ReadOnlySpan** instead of a traditional array, completely avoiding dynamic memory allocation.
However, since C# 13 support in Unity might not be available for several years, the library also includes versions of the behavior tree functions compatible with earlier C# versions. These implementations are slightly more verbose but remain simple, clear, and allocation-free.

## Requirements
- C# 9 (Unity 2021.2 and later) is required because of using static anonymous delegates.

# Description

## Initial Constraints and Approach

1. Functional BT implements a classic behavior tree, executed in its entirety during each game loop cycle, rather than a more complex variant like an Event-Driven BT.  
2. It follows a "code-only" approach, relying exclusively on C# without the need for specialized visual editors (such as Behavior Designer) or custom behavior tree languages (like PandaBT [link]).   
3. The implementation adheres to the modern standard, where the behavior tree and the managed object (commonly referred to as the Blackboard) are treated as separate entities. The tree consists of nested nodes that operate on the Blackboard, which is shared across all nodes within the same tree instance.

## Functional Approach to Behavior Tree Development

In my opinion, the authors of most behavior tree solutions make a mistake by treating the behavior tree as a graph, which is too complex a metaphor that leads to unnecessarily complicated designs. Nodes are typically represented as objects, each requiring its own class, and editing the tree often involves complex visual editors. Since nodes are objects, their creation and execution happen in different parts of the code, making debugging difficult and prompting the need for specialized debuggers.

Instead, the behavior tree should be thought of as a function. Each BT node is a function (not an object!) because it has multiple inputs and one output, and it does not require memory. That is why the tree as a whole is also a function that recursively calls a set of functions. This is why we need to use functional programming here - this approach allows us to create clear and structured FB code that is easy to develop and debug using your favorite IDE.

These main principles guided the development of this library, and I am glad that the result is nearly identical to my original vision. There were some compromises, and the resulting code contains minor boilerplate, but it remains easy to read, merge, and debug, while being fast and memory-efficient.

## Convenient debugging

Look at the code [above](#usage-example).   
You can set a breakpoint on every line of this code — on each anonymous delegate, which represents an action or condition, and on each call to a node function (such as Selector, Sequencer, Action) — and every  breakpoint will be invoked correctly at the right moment.  
I believe the debugging capabilities are ideal.

## Zero memory allocation

### What is the problem

In one of the early versions of the library, the code looked like this:

```csharp
    public class MyLaconicFunctionalBt : LaconicFunctionalBt <ActorBoard>
    {
        public MyLaconicFunctionalBt(ActorBoard board) : base(board) { }
        
        public Status Execute()
        {
            return
                Selector(
                    _ => Action(
                        _ => SetColor(Color.grey)),
                    _ => VoidActions(Status.Success,
                        _ => SetColor(Color.red)));
        }

        Status SetColor(Color color)
        {
            Board.View.SetColor(color);
            return Status.Failure;
        }
    }
```

Where Sequencer() function head is:
```csharp
public Status Sequencer(T board, params Func<T, Status>[] funcs)
```

In my opinion, this code looks great and fully implements the original idea — it is easy to debug, and the tree structure is stored directly in the code.

However, this code makes heavy use of dynamic memory allocation, although this may not be obvious at first glance.

1. Here closures are used because each delegate argument implicitly uses “this” pointer that is an external variable. As a result, every time such a lambda function is used, the new operator is called, creating a new System.Delegate object.  
2. To pass a variable list of parameters to the Sequencer and Selector functions, arrays are implicitly used, since this is how functions with a variable number of arguments work in C#: before calling the function, an array is created in the heap (via new), filled with values, and passed to the function.

The tree is executed on each game loop cycle for every NPC object, and each time, these arrays and delegates are created. With a large number of NPCs in the scene, memory traffic can become significant.  

But both problems were resolved.

### Solution to the Memory Allocation Problem for Anonymous Delegates

The problem was fully resolved by using 'static anonymous functions', which were introduced in C# 9 and are supported in Unity 2021.2 and later versions.  
A detailed explanation of this feature is available in the article [Understanding the Cost of C# Delegates](https://devblogs.microsoft.com/dotnet/understanding-the-cost-of-csharp-delegates/). 

Please look at the final Functional BT syntax:  

Code fragment here

Please note the following key points:
1. The reference to the blackboard is an internal variable within the lambda function that’s why every BT node (Action, Selector, Sequencer, etc.) gets the blackboard as a first input argument and then uses it when calling input delegates.  
2. Node functions (Sequencer, Selector, etc.) are declared as static, meaning they do not reference “this” pointer.  
3. The static modifier ensures that closures are not used here and allows the using of the same blackboard variable name “b” in all the delegates (which, in my opinion, is convenient), avoiding compiler warnings.

As a result, memory allocation for delegate instances is completely eliminated. Only static class fields (whose memory is allocated once) are used, instead of instance fields.

### Resolving the Problem with `params` Arrays

In C#13, which was released about a month ago, a great new feature called *params Collections* was introduced. This feature allows memory allocation for functions with a variable number of arguments to occur on the stack instead of the heap, providing a simple and efficient solution.

To use this feature, you only need to modify the function's parameter declaration.  
Instead of:
```csharp
params Func<T, Status>[] funcs  
```
you write   
```csharp
params ReadOnlySpan<Func<T, Status>> funcs  
```
That's it! The issue is resolved. From now on, no heap allocation is performed for the array; everything happens on the stack!

Unfortunately, C#13 will not be integrated into Unity anytime soon, which necessitates an alternative solution.

I addressed the problem by defining a function with a fixed number of parameters, with the last ones being nullable.

**Explanation of the Solution**

1. **Sequencer with ReadOnlySpan**  
   The `Sequencer` function accepts a collection of type `ReadOnlySpan` (stored on the stack) when using C#13. In earlier versions, it defaults to handling a standard array.

2. **Overloaded Sequencer Function**  
   There’s also an overloaded `Sequencer` function with a fixed number of parameters, all of which have default values. This allows it to be called with a varying number of arguments.  

   - A practical limit of 5 nodes is implemented based on typical usage scenarios.  
   - If the user provides 6 or more nodes, the variadic version of the function will be invoked. While less memory-efficient, it remains fully functional.

3. **Advantages and Trade-offs**  
   Although this approach may not look elegant, it works effectively. It might even perform better than `ReadOnlySpan` under certain conditions.

**Optimizing Further**

For those who are deeply concerned about execution efficiency, the best solution would involve creating (or auto-generating) a series of functions with fixed argument counts (e.g., from 1 to 20).  

While the resulting code would appear verbose and cumbersome, the compiler would always select the most optimal function for the given input, ensuring no unnecessary overhead.
