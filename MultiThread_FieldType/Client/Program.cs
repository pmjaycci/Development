class Program
{
    static void Main()
    {
        for (int i = 0; i < 100; i++)
        {
            var test = new ServerConnect($"테스트유저{i}");
            ThreadPool.QueueUserWorkItem(_ => test.Run());
        }
        Console.ReadLine();
    }
}