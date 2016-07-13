// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information. 

// This file has been copied from the excellent Reactive-Extensions project, specifically:
// https://github.com/Reactive-Extensions/Rx.NET/blob/master/Rx.NET/Source/System.Reactive.Core/Reactive/Disposables/AnonymousDisposable.cs

// The file has been modified for use in this project.

using System;
using System.Diagnostics;
using System.Threading;

namespace FluentAssertionsEx.Support
{
    /// <summary>
    /// Represents an Action-based disposable.
    /// </summary>
    public sealed class ActionDisposable : IDisposable
    {
        private volatile Action _dispose;

        /// <summary>
        /// Constructs a new disposable with the given action used for disposal.
        /// </summary>
        /// <param name="dispose">Disposal action which will be run upon calling Dispose.</param>
        public ActionDisposable(Action dispose)
        {
            Debug.Assert(dispose != null);

            _dispose = dispose;
        }

        /// <summary>
        /// Calls the disposal action if and only if the current instance hasn't been disposed yet.
        /// </summary>
        public void Dispose()
        {
#pragma warning disable 0420
            var dispose = Interlocked.Exchange(ref _dispose, null);
#pragma warning restore 0420

            if (dispose != null)
            {
                dispose();
            }
        }
    }
}
