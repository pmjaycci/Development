
class Program
{
    static int num = 0;
    static Lock _lock = new Lock();

    static int max = 3;

    static void Main()
    {
        Thread t1 = new Thread(Thread_1);
        Thread t2 = new Thread(Thread_2);
        t1.Start();
        t2.Start();
        t1.Join();
        t2.Join();
        System.Console.WriteLine($"TEST num [{num}]");
    }

    static void Thread_1()
    {
        for (int i = 0; i < max; i++)
        {
            _lock.Acquire();
            num++;
            Thread.Sleep(5000);
            _lock.Release();
        }
    }
    static void Thread_2()
    {
        for (int i = 0; i < max; i++)
        {
            _lock.Acquire();
            num--;
            System.Console.WriteLine("TEST");

            _lock.Release();
        }
    }
}

public class Lock
{
    AutoResetEvent _lock = new AutoResetEvent(true);
    public void Acquire()
    {
        _lock.WaitOne();
    }


    public void Release()
    {
        _lock.Set();
    }
}