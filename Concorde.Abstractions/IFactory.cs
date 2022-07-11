namespace Concorde.Abstractions;

public interface IFactory<out T>
{
    public T Create();
}