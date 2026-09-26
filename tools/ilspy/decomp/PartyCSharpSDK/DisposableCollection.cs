using System;
using System.Collections.Generic;

namespace PartyCSharpSDK;

public class DisposableCollection : IDisposable
{
	private readonly List<IDisposable> disposables;

	public DisposableCollection()
	{
		disposables = new List<IDisposable>();
	}

	public void Dispose()
	{
		Dispose(isDisposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool isDisposing)
	{
		foreach (DisposableBuffer disposable in disposables)
		{
			disposable?.Dispose();
		}
	}

	~DisposableCollection()
	{
		Dispose(isDisposing: false);
	}

	public T Add<T>(T disposable) where T : IDisposable
	{
		disposables.Add(disposable);
		return disposable;
	}
}
