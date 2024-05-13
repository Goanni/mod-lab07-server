﻿using System;
using System.Threading;
namespace TPProj
{
    class Program
    {
        static void Main()
        {
            int count_flow = 5;
            int time_server = 500;
            int time_client = 100;
            Server server = new Server();
            Client client = new Client(server);
            for (int id = 1; id <= 100; id++)
            {
                client.send(id);
                Thread.Sleep(50);
            }
            Console.WriteLine("Всего заявок: {0}", server.requestCount);
            Console.WriteLine("Обработано заявок: {0}", server.processedCount);
            Console.WriteLine("Отклонено заявок: {0}", server.rejectedCount);

            double intensity_row_application = (double)1.00 / time_client;
            double intensity_row_service = (double)1.00 / time_server;
            double average_p = (double)intensity_row_application / intensity_row_service;
            Console.WriteLine("Интенсивность потока заявок: {0}", intensity_row_application);
            Console.WriteLine("Интенсивность потока обслуживания: {0}", intensity_row_service);

            double Fact(double n)
            {
                if (n == 0)
                    return 1;
                else
                    return n * Fact(n - 1);
            }
            double sum = 0;
            for (int i = 0; i < count_flow; i++)
            {
                sum += Math.Pow(average_p, i) / Fact(i);
            }
            double probability_downtime_system = Math.Pow(sum, -1);
            double probability_failure_system = (double)(Math.Pow(average_p, count_flow) / Fact(count_flow)) * probability_downtime_system;
            double relative_throughput = 1 - (probability_failure_system);
            double absolute_throughput = intensity_row_application * relative_throughput;
            double average_number_busychannels = (double)absolute_throughput / intensity_row_service;
            Console.WriteLine("Вероятность простоя системы: {0}", probability_downtime_system);
            Console.WriteLine("Вероятность отказа системы: {0}", probability_failure_system);
            Console.WriteLine("Относительная пропускная способность: {0}", relative_throughput);
            Console.WriteLine("Абсолютная пропускная способность: {0}", absolute_throughput);
            Console.WriteLine("Среднее число занятых каналов: {0}", average_number_busychannels);
        }
    }
    struct PoolRecord
    {
        public Thread thread;
        public bool in_use;
    }
    class Server
    {
        private PoolRecord[] pool;
        private object threadLock = new object();
        public int requestCount = 0;
        public int processedCount = 0;
        public int rejectedCount = 0;
        public Server()
        {
            pool = new PoolRecord[5];
        }
        public void proc(object sender, procEventArgs e)
        {
            lock (threadLock)
            {
                Console.WriteLine("Заявка с номером: {0}", e.id);
                requestCount++;
                for (int i = 0; i < 5; i++)
                {
                    if (!pool[i].in_use)
                    {
                        pool[i].in_use = true;
                        pool[i].thread = new Thread(new ParameterizedThreadStart(Answer));
                        pool[i].thread.Start(e.id);
                        processedCount++;
                        return;
                    }
                }
                rejectedCount++;
            }
        }

public void Answer(object arg)
        {
            int id = (int)arg;
            Console.WriteLine("Обработка заявки: {0}", id);
            Thread.Sleep(500);
            for (int i = 0; i < 5; i++)
                if (pool[i].thread == Thread.CurrentThread)
                    pool[i].in_use = false;
        }
    }
    class Client
    {
        private Server server;
        public Client(Server server)
        {
            this.server = server;
            this.request += server.proc;
        }
        public void send(int id)
        {
            procEventArgs args = new procEventArgs();
            args.id = id;
            OnProc(args);
        }
        protected virtual void OnProc(procEventArgs e)
        {
            EventHandler<procEventArgs> handler = request;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        public event EventHandler<procEventArgs> request;
    }
    public class procEventArgs : EventArgs
    {
        public int id { get; set; }
    }
}