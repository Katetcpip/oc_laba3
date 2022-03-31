using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace osiLaba3
{
    class Threader
    {
        public Thread thread;
        public static bool ShutDown;
        public static int ConsumersAlive;
    }

    class Producer : Threader
    {
        int ManifacturerNumber;
        public Queue<int> mq = new();
        Random random = new();
        bool isSleeping;

        public Producer(int num)
        {
            ManifacturerNumber = num;
            isSleeping = false;
            ShutDown = false;
            thread = new Thread(Manufacte);
            thread.Start();
        }
        private void Manufacte()
        {
        Mnf:
            if (isSleeping)
            {
                if (Program.q.Count <= 80) isSleeping = false;
            }
            else
            {
                int w = random.Next(1, 100);
                lock (Program.q)
                    Program.q.Enqueue(w);
                lock (mq)
                    mq.Enqueue(w);
                Console.WriteLine("Производитель [" + ManifacturerNumber.ToString() + "]: добавлено " + w.ToString());
                if (Program.q.Count >= 100)
                {
                    isSleeping = true;
                    Console.WriteLine("Производитель [" + ManifacturerNumber.ToString() + "]: уснул");
                }
            }
            try
            {
                Thread.Sleep(500);
            }
            catch
            {
                return;
            }
            goto Mnf;
        }
    }

    class Consumer : Threader
    {
        int ConsumerNumber;
        public Queue<int> cq = new();

        public Consumer(int num)
        {
            ConsumerNumber = num;
            ConsumersAlive = 2;
            thread = new Thread(Consume);
            thread.Start();
        }
        private void Consume()
        {
        Cns:
            lock (Program.q)
            {
                if (Program.q.Count > 0)
                {
                    lock (cq)
                        cq.Enqueue(Program.q.Dequeue());
                }
                else if (ShutDown)
                {
                    ConsumersAlive--;
                    if (ConsumersAlive == 0)
                    {
                        Console.WriteLine("\n\n Подождите, программа сейчас завершится\n\n");
                        Thread.Sleep(3000);
                    }
                    return;
                }
            }
            Thread.Sleep(500);
            goto Cns;
        }
    }

    class Program
    {
        public static Queue<int> q = new();

        public async static void Print()
        {
            Queue<int> temp;
        Printing:
            Console.Clear();
            Console.WriteLine();
            lock (q) temp = q;
            lock (temp)
                foreach (int item in temp)
                {
                    Console.Write(" " + item.ToString());
                    if (item < 10) Console.Write(" ");
                }
            Console.WriteLine();
            for (int i = 0; i < Console.WindowWidth; i++)
                Console.Write("_");
            Console.WriteLine("\n\n q - остановить производителей");
            await Task.Delay(500);
            if (Threader.ConsumersAlive == 0) return;
            goto Printing;
        }

        static void Main(string[] args)
        {
            Thread printer = new(Print);
            Threader[] f =
            {
                new Producer(1),
                new Producer(2),
                new Producer(3),
                new Consumer(1),
                new Consumer(2)
            };
            printer.Start();
            ConsoleKey key = Console.ReadKey().Key;
            if (key == ConsoleKey.Q)
            {
                for (int i = 0; i < 3; i++)
                {
                    f[i].thread.Interrupt();
                    Threader.ShutDown = true;
                }
            }
        }
    }
}
