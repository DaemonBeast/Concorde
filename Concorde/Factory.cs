using Concorde.Abstractions;

namespace Concorde;

public class Factory<T> : IFactory<T>
{
    private readonly Func<T> _factory;

    public Factory(Func<T> factory)
    {
        this._factory = factory;
    }

    public T Create()
    {
        return this._factory.Invoke();
    }
}