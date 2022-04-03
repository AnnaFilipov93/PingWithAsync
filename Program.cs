﻿using System.Diagnostics;
using System.Net.NetworkInformation;

class Program
{
    #region Fields & Properties
    static int _PingCount = 2;
    static int _PingInterval = 500;
    static Stopwatch _StopWatch;
    static List<string> _HostsNames = new List<string>()
        {
            "cnn.com",
            "sbs.com.au",
            "bbc.co.uk",
            "maariv.co.il",
            "brazilian.report"
        };
    static string _Menu = @"Choose async method invokation that you would like to compare to sync invokation:
                        t = Thread
                        tp = ThreadPool
                        ta = Task
                        pf = Parallel for
                        pfe = Parallel for each
                        pi = Parallel invoke
                        OR ctrl+C to break...";

    #endregion
    public static void Main()
    {
        Console.WriteLine(_Menu);
        string userInput = Console.ReadLine().ToLower().Trim();
        Console.Clear();
        //
        PrintStars();
        PrintReport(GetHostsReplies);
        //        
        PrintStars();
        if (userInput == "t")
            PrintReport(GetHostsRepliesWithThreads);
        else if (userInput == "tp")
            PrintReport(GetHostsRepliesWithThreadPool);
        else if (userInput == "ta")
            PrintReport(GetHostsRepliesWithTasks);
        else if (userInput == "pf")
            PrintReport(GetHostsRepliesWithParallelFor);
        else if (userInput == "pfe")
            PrintReport(GetHostsRepliesWithParallelForEach);
        else if (userInput == "pi")
            PrintReport(GetHostsRepliesWithParallelInvoke);
        else Console.WriteLine("invalid input...");
    }

    #region  GetHostsReplies
    static Dictionary<string, List<PingReply>> GetHostsReplies()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        foreach (var hostName in _HostsNames)
        {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < _PingCount; i++)
            {
                pingReplies.Add(ping.Send(hostName));
                Thread.Sleep(_PingInterval);
            }
            hostsReplies.Add(hostName, pingReplies);
        }
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreads()
    {
        //throw new NotImplementedException();
        Dictionary<string, List<PingReply>> hostsRepliesWithThreads = new Dictionary<string, List<PingReply>>();
        object dicLock = new object();
        List<Thread> threads = new List<Thread>();

        foreach (var hostName in _HostsNames)
        {
            threads.Add(new Thread(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();
                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }
                lock (dicLock)
                {
                    hostsRepliesWithThreads.Add(hostName, pingReplies);
                }

            }
            ));
        }
        foreach (Thread thread in threads)
        {
            thread.Start();
        }
        foreach (Thread thread in threads)
        {
            thread.Join();
        }
        return hostsRepliesWithThreads;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreadPool()
    {
        //throw new NotImplementedException();
        Dictionary<string, List<PingReply>> hostsRepliesWithThreadsPool = new Dictionary<string, List<PingReply>>();
        object dicLock = new object();

        List<EventWaitHandle> ewhList = new List<EventWaitHandle>();

        foreach (var hostName in _HostsNames)
        {
            EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
            ewhList.Add(ewh);
            ThreadPool.QueueUserWorkItem((data) => {

                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();
                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }
                lock (dicLock)
                {
                    hostsRepliesWithThreadsPool.Add(hostName, pingReplies);
                }
                ; ewh.Set(); });

        }
        foreach (EventWaitHandle ewh in ewhList)
        {
            ewh.WaitOne();
        }
        return hostsRepliesWithThreadsPool;
    }


    static Dictionary<string, List<PingReply>> GetHostsRepliesWithTasks()
    {
        //throw new NotImplementedException();
        Dictionary<string, List<PingReply>> hostsRepliesWithWithTask = new Dictionary<string, List<PingReply>>();
        object dicLock = new object();

        List<Task> tList = new List<Task>();

        foreach (var hostName in _HostsNames)
        {
            tList.Add(new Task(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();
                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    //Thread.Sleep(_PingInterval);
                    Task.Delay(_PingInterval).Wait();
                }
                lock (dicLock)
                {
                    hostsRepliesWithWithTask.Add(hostName, pingReplies);
                }
            }));  
        }
        foreach (Task t in tList)
        {
            t.Start();
        }

        foreach (Task t in tList)
        {
            t.Wait();
        }
        //Task.WaitAll(tList.ToArray());

        return hostsRepliesWithWithTask;
    }

    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelInvoke()
    {
        //throw new NotImplementedException();
        Dictionary<string, List<PingReply>> hostsRepliesWithParallelInvoke = new Dictionary<string, List<PingReply>>();

        object dicLock = new object();
        List<Action> delegates = new List<Action>();    

        foreach (var hostName in _HostsNames)
        {
            delegates.Add(new Action(() =>
            {
                Ping ping = new Ping();
                List<PingReply> pingReplies = new List<PingReply>();
                for (int i = 0; i < _PingCount; i++)
                {
                    pingReplies.Add(ping.Send(hostName));
                    Thread.Sleep(_PingInterval);
                }
                lock (dicLock)
                {
                    hostsRepliesWithParallelInvoke.Add(hostName, pingReplies);
                }

            }
            ));
        }
        Parallel.Invoke(delegates.ToArray());

        return hostsRepliesWithParallelInvoke;
    }

    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelForEach()
    {
        //throw new NotImplementedException();
        Dictionary<string, List<PingReply>> hostsRepliesWithParallel = new Dictionary<string, List<PingReply>>();

        object dicLock = new object();

        Parallel.ForEach(_HostsNames, (hostName) =>
        {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < _PingCount; i++)
            {
                pingReplies.Add(ping.Send(hostName));
                Thread.Sleep(_PingInterval);
            }
            lock (dicLock)
            {
                hostsRepliesWithParallel.Add(hostName, pingReplies);
            }
        });
        return hostsRepliesWithParallel;
    }

    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelFor()
    {
        //throw new NotImplementedException();
        Dictionary<string, List<PingReply>> hostsRepliesWithParallel = new Dictionary<string, List<PingReply>>();

        object dicLock = new();

        Parallel.For(0, _HostsNames.Count, (index) =>
        {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < _PingCount; i++)
            {
                pingReplies.Add(ping.Send(_HostsNames[index]));
                Thread.Sleep(_PingInterval);
            }
            lock (dicLock)
            {
                hostsRepliesWithParallel.Add(_HostsNames[index], pingReplies);
            }
        });

        return hostsRepliesWithParallel;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithTPL()
    {
        return null;
    }
    #endregion

    #region Print
    static void PrintLine() => Console.WriteLine("---------------------------");
    static void PrintStars() => Console.WriteLine("***************************");
    static void PrintReport(Func<Dictionary<string, List<PingReply>>> getHostsReplies)
    {
        Console.WriteLine($"Started {getHostsReplies.Method.Name}");
        _StopWatch = Stopwatch.StartNew();
        Dictionary<string, List<PingReply>> hostsReplies = getHostsReplies();
        _StopWatch.Stop();
        Console.WriteLine($"Finished {getHostsReplies.Method.Name}");
        PrintLine();
        Console.WriteLine($"Printing {getHostsReplies.Method.Name} report:");
        if (hostsReplies != null)
            PrintHostsRepliesReports(hostsReplies);
        PrintLine();
    }
    static void PrintHostsRepliesReports(Dictionary<string, List<PingReply>> hostsReplies)
    {
        long hostsTotalRoundtripTime = 0;
        Dictionary<string, PingReplyStatistics> hrs = GetHostsRepliesStatistics(hostsReplies);
        PrintTotalRoundtripTime(hrs);
        PrintLine();
        hostsTotalRoundtripTime = hrs.Sum(hr => hr.Value.TotalRoundtripTime);
        Console.WriteLine($"Report took {_StopWatch.ElapsedMilliseconds} ms to generate,{_PingCount * _HostsNames.Count} total pings took total {hostsTotalRoundtripTime} ms hosts roundtrip time");
    }
    static void PrintTotalRoundtripTime(Dictionary<string, PingReplyStatistics> hrs, bool ascendingOrder = true)
    {
        string orderDescription = ascendingOrder ? "ascending" : "descending";
        Console.WriteLine($"Hosts total roundtrip time in {orderDescription} order: (HostName:X,Replies statistics:Y)");
        var orderedHrs = ascendingOrder ? hrs.OrderBy(hr => hr.Value.TotalRoundtripTime) : hrs.OrderByDescending(hr => hr.Value.TotalRoundtripTime);
        foreach (var hr in orderedHrs)
        {
            Console.WriteLine($"{hr.Key},{hr.Value}");
        }
    }
    static void PrintHostsRepliesStatistics(Dictionary<string, PingReplyStatistics> hrs)
    {
        Console.WriteLine("Hosts replies statistics: (HostName:X,Replies statistics:Y)");
        foreach (var hr in hrs)
        {
            Console.WriteLine($"{hr.Key},{hr.Value}");
        }
    }

    #endregion

    static Dictionary<string, PingReplyStatistics> GetHostsRepliesStatistics(Dictionary<string, List<PingReply>> hostsReplies)
    {
        Dictionary<string, PingReplyStatistics> hrs = new Dictionary<string, PingReplyStatistics>();
        foreach (var hr in hostsReplies)
            hrs.Add(hr.Key, new PingReplyStatistics(hr.Value));
        return hrs;
    }
}