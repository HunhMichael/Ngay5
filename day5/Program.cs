using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static CancellationTokenSource cts = new();

    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        while (true)
        {
            Console.WriteLine("\n===== MENU =====");
            Console.WriteLine("1. Bắt đầu tải dữ liệu");
            Console.WriteLine("2. Hủy quá trình");
            Console.WriteLine("3. So sánh Thread vs Task");
            Console.WriteLine("4. Thoát");
            Console.Write("Chọn: ");
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    cts = new CancellationTokenSource();
                    await FetchAllNewsAsync(cts.Token);
                    break;
                case "2":
                    cts.Cancel();
                    Console.WriteLine(">>> Đã yêu cầu hủy quá trình...");
                    break;
                case "3":
                    SoSanhThreadVsTask();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ.");
                    break;
            }
        }
    }

    static async Task<string> GetNewsAsync(string source, CancellationToken token)
    {
        Console.WriteLine($"[{DateTime.Now:T}] Đang tải từ {source}...");

        if (source == "CNN")
        {
            await Task.Delay(2000, token);
            throw new HttpRequestException($"Lỗi khi tải từ {source}");
        }

        for (int i = 0; i < 4; i++)  // giả lập tải lâu
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(500, token);
        }

        string result = $"{source} - Nội dung mô phỏng " + new string('.', new Random().Next(50, 100));
        Console.WriteLine($"[{DateTime.Now:T}] Tải xong: {source}");
        return result;
    }

    static async Task FetchAllNewsAsync(CancellationToken token)
    {
        List<string> sources = new() { "VNExpress", "Tuổi Trẻ", "Thanh Niên", "BBC", "CNN" };
        List<Task<string>> tasks = new();

        foreach (var source in sources)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    return await GetNewsAsync(source, token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"⚠️ Đã hủy tải: {source}");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"❌ Lỗi tải {source}: {ex.Message}");
                }
                return null;
            });

            tasks.Add(task);
        }

        var results = await Task.WhenAll(tasks);
        int totalChars = results.Where(r => r != null).Sum(r => r.Length);
        Console.WriteLine($"\n>>> Tổng ký tự từ các nguồn thành công: {totalChars}");
    }

    static void SoSanhThreadVsTask()
    {
        Console.WriteLine("\n>>> So sánh Thread và Task:");

        // Thread
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        List<Thread> threads = new();
        for (int i = 0; i < 3; i++)
        {
            var t = new Thread(() =>
            {
                Thread.Sleep(1000);
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} xong.");
            });
            threads.Add(t);
            t.Start();
        }
        threads.ForEach(t => t.Join());
        sw1.Stop();

        // Task
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        List<Task> taskList = new();
        for (int i = 0; i < 3; i++)
        {
            taskList.Add(Task.Run(async () =>
            {
                await Task.Delay(1000);
                Console.WriteLine($"Task {Task.CurrentId} xong.");
            }));
        }
        Task.WaitAll(taskList.ToArray());
        sw2.Stop();

        Console.WriteLine($"\n⏱ Thread mất: {sw1.ElapsedMilliseconds} ms");
        Console.WriteLine($"⏱ Task mất: {sw2.ElapsedMilliseconds} ms");
    }
}
