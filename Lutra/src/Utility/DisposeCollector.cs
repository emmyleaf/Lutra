using System.Collections.Generic;

namespace Lutra.Utility
{
    public class DisposeCollector
    {
        private readonly List<IDisposable> _disposables = [];

        public void Add<T>(T disposable) where T : IDisposable
        {
            _disposables.Add(disposable);
        }

        public void Add<T>(params T[] array) where T : IDisposable
        {
            foreach (T item in array)
            {
                _disposables.Add(item);
            }
        }

        public void Remove(IDisposable disposable)
        {
            if (!_disposables.Remove(disposable))
            {
                throw new InvalidOperationException("Unable to untrack " + disposable + ". It was not previously tracked.");
            }
        }

        public void DisposeAll()
        {
            foreach (IDisposable disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }
    }
}
