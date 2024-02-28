
class Program
{
    static int num = 0;
    static Spinlock spinlock = new Spinlock();

    static int max = 10000;
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
            spinlock.Acquire();
            num++;
            spinlock.Release();
        }
    }
    static void Thread_2()
    {
        for (int i = 0; i < max; i++)
        {
            spinlock.Acquire();
            num--;
            spinlock.Release();
        }
    }
}

public class Spinlock
{
    volatile int _lock = 0;
    public void Acquire()
    {
        while (true)
        {
            int desire = 1;
            int expected = 0;
            if (Interlocked.CompareExchange(ref _lock, desire, expected) == expected)
                break;
        }
    }


    public void Release()
    {
        _lock = 0;
    }
}