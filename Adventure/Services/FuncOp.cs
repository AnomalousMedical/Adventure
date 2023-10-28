using System;

namespace Adventure.Services;

static class FuncOp
{
    public static T Create<T>(Func<T> op)
    {
        return op();
    }
}
