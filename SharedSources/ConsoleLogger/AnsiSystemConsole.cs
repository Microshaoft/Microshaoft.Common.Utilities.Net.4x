// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


namespace Microshaoft.Extensions.Logging.Console
{

    using System.IO;
    internal class AnsiSystemConsole : IAnsiSystemConsole
    {
        private readonly TextWriter _textWriter;

        /// <inheritdoc />
        public AnsiSystemConsole(bool stdErr = false)
        {
            _textWriter = stdErr ? System.Console.Error : System.Console.Out;
        }

        public void Write(string message)
        {
            _textWriter.Write(message);
        }
    }
}
