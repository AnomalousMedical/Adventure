﻿using Engine;
using Microsoft.Extensions.Logging;
using System;

namespace ObjectTest
{
    class InjectedThing : IDisposable
    {
        private static int CurrentIndex = 0;

        private readonly ILogger<InjectedThing> logger;
        private int index = CurrentIndex++;

        public InjectedThing(ILogger<InjectedThing> logger)
        {
            logger.LogInformation($"Created InjectedThing {index}");

            this.logger = logger;
        }

        public void Dispose()
        {
            logger.LogInformation($"Disposed InjectedThing {index}");
        }
    }
}
