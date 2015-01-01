﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean. All rights reserved.
// -----------------------------------------------------------------------
using System;

namespace AudioSwitcher
{
    internal class Lifetime<T> : IDisposable
    {
        private readonly Func<T> _instanceGetter;
        private readonly Action _disposer;

        public Lifetime(Func<T> instanceGetter, Action disposer)
        {
            if (instanceGetter == null)
                throw new ArgumentNullException("instanceGetter");

            _instanceGetter = instanceGetter;
            _disposer = disposer;
        }

        public T Instance
        {
            get { return _instanceGetter(); }
        }

        public void Dispose()
        {
            if (_disposer != null)
            {
                _disposer();
            }
        }
    }
}
