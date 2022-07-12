namespace Concorde.Utilities;

public class SemaphoreLocker
{
    private readonly SemaphoreSlim _semaphore;

    public SemaphoreLocker()
    {
        this._semaphore = new SemaphoreSlim(1, 1);
    }

    public void Lock(Action action)
    {
        this._semaphore.Wait();

        try
        {
            action.Invoke();
        }
        finally
        {
            this._semaphore.Release();
        }
    }
    
    public T Lock<T>(Func<T> action)
    {
        this._semaphore.Wait();

        try
        {
            return action.Invoke();
        }
        finally
        {
            this._semaphore.Release();
        }
    }

    public async Task LockAsync(Func<Task> taskFactory)
    {
        await this._semaphore.WaitAsync();

        try
        {
            await taskFactory.Invoke();
        }
        finally
        {
            this._semaphore.Release();
        }
    }
        
    public async Task<T> LockAsync<T>(Func<Task<T>> taskFactory)
    {
        await this._semaphore.WaitAsync();

        try
        {
            return await taskFactory.Invoke();
        }
        finally
        {
            this._semaphore.Release();
        }
    }
}