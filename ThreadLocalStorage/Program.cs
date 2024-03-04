using System.Runtime.InteropServices;

class Program
{
    static ThreadLocal<string> threadName = new ThreadLocal<string>(() => { return $"ThreadId :{Thread.CurrentThread.ManagedThreadId}"; });

    static void ThreadFunc()
    {
        bool isCreated = threadName.IsValueCreated;
        if (isCreated)
            System.Console.WriteLine($"Created {threadName.Value}");
        else
            System.Console.WriteLine($"New Create! {threadName.Value}");
    }
    static void Main()
    {
        ThreadPool.SetMinThreads(1, 1);
        ThreadPool.SetMaxThreads(2, 2);
        Parallel.Invoke(ThreadFunc, ThreadFunc, ThreadFunc, ThreadFunc, ThreadFunc);
    }
}