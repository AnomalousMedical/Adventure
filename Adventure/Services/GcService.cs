using System;

namespace Adventure.Services;

/// <summary>
/// This exposes a centralized way to interact with the Garbage Collector. By
/// using an interface the actual gc call can be abstracted. Nothing about this
/// is some kind of GC magic. The app still has to be well tuned and not have leaks
/// of its own. What this does do is allow GC.Collect to be called at natural times,
/// such as game state transitions, where the user is waiting anyway.
/// </summary>
interface IGcService
{
    public void Collect();
}

class GcService : IGcService
{
    public void Collect()
    {
        GC.Collect();
    }
}
