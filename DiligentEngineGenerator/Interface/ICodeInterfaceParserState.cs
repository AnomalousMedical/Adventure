﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DiligentEngineGenerator
{
    interface ICodeInterfaceParserState
    {
        ICodeInterfaceParserState Parse(String line, List<String> comments, CodeInterface code);
    }
}
