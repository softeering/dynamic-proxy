namespace DomainGateway.Infrastructure;

using System.Threading;

public class AtomicReference<T>
{
	private T? _value;

	public AtomicReference(T value)
	{
		this._value = value;
	}

	public T Value
	{
		get => this._value ?? throw new InvalidOperationException("Value was not initialized");
		set => Interlocked.Exchange(ref this._value, value);
	}

	/*public bool CompareAndSet(T expected, T update)
	{
		return Interlocked.CompareExchange(ref this._value, update, expected) == expected;
	}*/
}
