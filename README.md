# Functional Behavior Tree in C#

This project provides a functional-style implementation of a behavior tree, designed for clear AI logic and convenient debugging.

## Key Features

1. **Concise Tree Definition**  
   Behavior trees are defined directly in code using lambda expressions, resulting in clear and compact logic.   

3. **Ease of Debugging**  
   Сode is easy to debug — the tree definition and execution code are the same, which means you can place breakpoints inside any anonymous delegate, and they will work correctly.  
   No special complex "behaviour tree debugger" is required, you will use your favorite C# IDE for debugging.

3. **No memory is allocated for tree structure**  
   No memory is allocated for the tree structure because of the structure is embedded in the code itself.

## Usage Example

Detailed example is inside the file 'FunctionalBehave.cs'. 
Here’s a core of it, the tree definition.

This example avoids closures and uses only anonymous delegates, making it relatively efficient in terms of memory usage.  

As you can see bellow this code is easy to debug — you can place breakpoints inside any anonymous delegate, and they will work correctly.

```csharp
using System;
using System.Collections.Generic;
using System.Diagnostics;
using FunctionlBT;
using UnityEngine;

namespace FunctionalBtTest
{
    //...

    public class FunctionalBehave : MonoBehaviour
    {
        //...

        public Status ExecuteTree()
        {
            return
                bt.Parallel(ParallelPolicy.REQUIRE_ONE_SUCCESS,
                    bt => bt.Board.go.UpdateBlackboards(bt.Board),
                    bt => bt.Selector(
                        bt => bt.ConditionalVoidActions(
                            bt => bt.Board.playerDistance < 0.5f,
                            Status.RUNNING,
                            bt => bt.Board.go.SetColor(Color.red),
                            bt => bt.Board.go.StandAndFight(bt.Board)),
                        bt => bt.ConditionalVoidActions(bt => bt.Board.playerDistance < 2f, Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.red),
                            bt => bt.Board.go.JumpTowards(bt.Board)),
                        bt => bt.ConditionalVoidActions(bt => bt.Board.playerDistance < 2.2f, Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.red),
                            bt => bt.Board.go.MoveAndShot(bt.Board)),
                        bt => bt.ConditionalVoidActions(bt => bt.Board.playerDistance < 4f, Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.magenta),
                            bt => bt.Board.go.MoveAndShot(bt.Board)),
                        bt => bt.ConditionalVoidActions(bt => bt.Board.playerDistance < 8f, Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.yellow),
                            bt => bt.Board.go.Move(bt.Board)),
                        bt => bt.VoidActions(Status.RUNNING, 
                            bt => bt.Board.go.SetColor(Color.grey),
                            bt => bt.Board.go.Stand(bt.Board))
                    ),
                    bt => bt.Board.go.UpdateForce(bt.Board)
                );
        }
    }
}
        //...
```

![Example of debugging](functional_bt_example.jpg)

---

## Notes on Memory Usage

This approach utilizes anonymous delegates, which may incur memory overhead, especially when closures are involved.  
However, whether anonymous delegates cause memory traffic depends on the platform and runtime, which remains an open question.

### Requirements

- **.NET Version**: .NET 5 or higher recommended.
