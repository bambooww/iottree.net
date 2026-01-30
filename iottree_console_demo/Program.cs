using Grpc.Core;
using Grpc.Net.Client;
using iottree.lib;
using System.Reflection.PortableExecutable;

public class T
{
    async static Task test1()
    {
        var client_id = "123";
        var channel = GrpcChannel.ForAddress("http://localhost:9092");
        var client = new IOTTreeServer.IOTTreeServerClient(channel);

        var _headers = new Metadata {
                { "client-id", client_id },
                { "user-agent", $"IOTTreeClient/1.0 ({Environment.OSVersion})" },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }
            };
        // call 
        var resp = await client.listPrjsAsync(new ReqClient { ClientId = client_id }, _headers);
        foreach (PrjItem pi in resp.Prjs)
        {
            Console.WriteLine($"prj item: {pi.Name} - {pi.Title}");
        }
    }


    //void test2()
    //{
    //    // 创建客户端
    //    var client = new IOTTreeClient("https://localhost:9092", "my-client-001");

    //    // 设置要读取的Tag路径
    //    client.TagPathsToRead = new List<string>
    //        {
    //            "Project1/tag1",
    //            "Project1/tag2",
    //            "Project2/tag1"
    //        };

    //    // 订阅事件
    //    client.TagValueChanged += (sender, e) =>
    //    {
    //        Console.WriteLine($"Tag值变更: {e.Tag.GetFullPath()} = {e.Tag.CurrentValue}");
    //    };

    //    client.ConnStateChanged += (sender, e) =>
    //    {
    //        Console.WriteLine($"连接状态: {(e.IsConnected ? "已连接" : "已断开")}");
    //    };

    //    client.ErrorOccurred += (sender, e) =>
    //    {
    //        Console.WriteLine($"错误: {e.ErrorMessage}");
    //    };

    //    client.Reconnecting += (sender, e) =>
    //    {
    //        Console.WriteLine("正在尝试重新连接...");
    //    };

    //    client.Reconnected += (sender, e) =>
    //    {
    //        Console.WriteLine("重新连接成功");
    //    };

    //    // 启动客户端
    //    client.Start();

    //    Console.WriteLine("客户端已启动，按任意键停止...");
    //    Console.ReadKey();

    //    // 写入Tag值示例
    //    var tag = client.FindTagByFullPath("Project1/tag1");
    //    if (tag != null)
    //    {
    //        //bool success = await client.WriteTagAsync(tag.IID, "new_value");
    //        //Console.WriteLine($"写入结果: {(success ? "成功" : "失败")}");
    //    }

    //    // 停止客户端
    //    client.Stop();

    //    // 释放资源
    //    client.Dispose();
    //}

    static async Task test3()
    {
        var client = new IOTTreeClient("http://localhost:9092", "console-client-001");

        // 订阅事件
        client.StateChanged += (sender, e) =>
        {
            Console.WriteLine($"State changed: {e.OldState} -> {e.NewState} ({e.Message})");
        };

        client.TagValueChanged += (sender, e) =>
        {
            // Console.WriteLine($"Tag updated: {e.TagPath} = {e.Value} at {e.UpdateTime}");
        };

        client.ConnectionLost += (sender, e) =>
        {
            Console.WriteLine("Connection lost!");
        };

        client.ConnectionRestored += (sender, e) =>
        {
            Console.WriteLine("Connection restored!");
        };

        client.ErrorOccurred += (sender, e) =>
        {
            Console.WriteLine($"Error occurred: {e.Message}");
        };

        client.SetTagPaths(new List<string> {"watertank.ch1.aio.wl_val", "watertank.ch1.aio.m1",
        "watertank.ch1.dio.pstart","watertank.ch1.dio.pstop"});

        Console.WriteLine("start...");

        client.Start();
        Thread.Sleep(2000);
        Console.WriteLine("conned=" + client.IsConnected);


        string cmd = null;
        do
        {
            Console.Write(">>");
            cmd = Console.ReadLine();
            if (cmd == null || cmd == "")
                continue;
            switch (cmd)
            {
                case "pstart":
                    try
                    {
                        var success = await client.WriteTagValueAsync("watertank.ch1.dio.pstart", "true");
                        Console.WriteLine($"Write tag result: {success}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Write failed: {ex.Message}");
                    }
                    break;
                case "pstop":
                    try
                    {
                        var success = await client.WriteTagValueAsync("watertank.ch1.dio.pstop", "true");
                        Console.WriteLine($"Write tag result: {success}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Write failed: {ex.Message}");
                    }
                    break;
                case "read":
                    var tagValues = client.GetTagValues();// ("watertank.ch1.aio.wl_val");
                    if (tagValues != null)
                    {
                        foreach (var tagValue in tagValues)
                            Console.WriteLine(tagValue);
                    }
                    break;
                case "state":
                    Console.WriteLine(client.State);
                    break;
                case "prjs":
                    try
                    {
                        var resp = await client.GetProjectListAsync();
                        Console.WriteLine("after---3---..." + resp);
                        if (resp != null)
                        {
                            foreach (PrjItem pi in resp)
                            {
                                Console.WriteLine($"prj item: {pi.Name} - {pi.Title}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    break;
                default:
                    continue;
            }
        } while (cmd != "exit");

        Console.WriteLine("---before disponse ---...");

        client.Dispose();

        Console.WriteLine("---end of test3 ---...");
    }


    public async static Task<int> Main(string[] args)
    {
        try
        {
            await test3();
        }
        catch (Exception ee)
        {
            Console.WriteLine(ee);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
        return 0;
    }
}
