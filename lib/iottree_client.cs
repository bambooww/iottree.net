using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


namespace iottree.lib
{
    /// <summary>
    /// 客户端状态枚举
    /// </summary>
    public enum ClientState
    {
        Disconnected,
        Connecting,
        Connected,
        Syncing,
        Disconnecting,
        Error
    }

    /// <summary>
    /// 标签值变更事件参数
    /// </summary>
    public class TagValueChangedEventArgs : EventArgs
    {
        public IOTTreeTagVal TagVal { get; set; }
    }

    /// <summary>
    /// 客户端状态变更事件参数
    /// </summary>
    public class ClientStateChangedEventArgs : EventArgs
    {
        public ClientState OldState { get; set; }
        public ClientState NewState { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// IOTTree gRPC客户端封装类
    /// </summary>
    public class IOTTreeClient : IDisposable
    {
        private readonly string _serverAddress;
        private readonly string _clientId;
        private readonly ConcurrentDictionary<string, IOTTreeTagVal> _tagCache;
        private readonly List<string> _subscribeTagPaths;
        private readonly object _syncLock = new object();
        private readonly int _reconnectIntervalMs = 5000; // 5秒重连间隔
        private readonly int _heartbeatTimeoutMs = 30000; // 30秒心跳超时

        private CancellationTokenSource _cts;
        private Thread _workerThread;
        private AsyncServerStreamingCall<TagSynVals> _streamCall;
        private GrpcChannel _channel;
        private IOTTreeServer.IOTTreeServerClient _client;
        private DateTime _lastHeartbeatTime;
        private bool _isDisposed;
        private Metadata _headers;

        /// <summary>
        /// 当前客户端状态
        /// </summary>
        public ClientState State { get; private set; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => State == ClientState.Connected ||
                                 State == ClientState.Syncing ||
                                 State == ClientState.Connecting;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => State == ClientState.Connected || State == ClientState.Syncing;

        /// <summary>
        /// 标签缓存数量
        /// </summary>
        public int TagCount => _tagCache.Count;

        /// <summary>
        /// 当标签值变更时触发
        /// </summary>
        public event EventHandler<TagValueChangedEventArgs> TagValueChanged;

        /// <summary>
        /// 当客户端状态变更时触发
        /// </summary>
        public event EventHandler<ClientStateChangedEventArgs> StateChanged;

        /// <summary>
        /// 当发生错误时触发
        /// </summary>
        public event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// 当连接断开时触发
        /// </summary>
        public event EventHandler ConnectionLost;

        /// <summary>
        /// 当连接恢复时触发
        /// </summary>
        public event EventHandler ConnectionRestored;

        /// <summary>
        /// 初始化IOTTree客户端
        /// </summary>
        /// <param name="serverAddress">服务器地址，格式："host:port"</param>
        /// <param name="clientId">客户端ID</param>
        public IOTTreeClient(string serverAddress, string clientId)
        {
            _serverAddress = serverAddress ?? throw new ArgumentNullException(nameof(serverAddress));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));

            _tagCache = new ConcurrentDictionary<string, IOTTreeTagVal>();
            _subscribeTagPaths = new List<string>();

            State = ClientState.Disconnected;
            _lastHeartbeatTime = DateTime.MinValue;

            _headers = new Metadata {
                { "client-id", clientId },
                { "user-agent", $"IOTTreeClient/1.0 ({Environment.OSVersion})" },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }
            };

        }

        /// <summary>
        /// 设置要订阅的标签路径
        /// </summary>
        /// <param name="tagPaths">标签路径列表</param>
        public void SetTagPaths(IEnumerable<string> tagPaths)
        {
            lock (_syncLock)
            {
                _subscribeTagPaths.Clear();
                if (tagPaths != null)
                {
                    _subscribeTagPaths.AddRange(tagPaths.Distinct());
                }

                // 清除不再订阅的标签缓存
                var currentPaths = new HashSet<string>(_subscribeTagPaths);
                var pathsToRemove = _tagCache.Keys.Where(path => !currentPaths.Contains(path)).ToList();
                foreach (var path in pathsToRemove)
                {
                    _tagCache.TryRemove(path, out _);
                }
            }
        }

        /// <summary>
        /// 添加要订阅的标签路径
        /// </summary>
        public void AddTagPath(string tagPath)
        {
            lock (_syncLock)
            {
                if (!_subscribeTagPaths.Contains(tagPath))
                {
                    _subscribeTagPaths.Add(tagPath);
                }
            }
        }

        /// <summary>
        /// 移除订阅的标签路径
        /// </summary>
        public void RemoveTagPath(string tagPath)
        {
            lock (_syncLock)
            {
                _subscribeTagPaths.Remove(tagPath);
                _tagCache.TryRemove(tagPath, out _);
            }
        }

        /// <summary>
        /// 获取所有订阅的标签路径
        /// </summary>
        public List<string> GetTagPaths()
        {
            lock (_syncLock)
            {
                return new List<string>(_subscribeTagPaths);
            }
        }

        /// <summary>
        /// 获取标签缓存值
        /// </summary>
        public IOTTreeTagVal GetTagValue(string tagPath)
        {
            return _tagCache.TryGetValue(tagPath, out var item) ? item : null;
        }

        public List<IOTTreeTagVal> GetTagValues()
        {
            return _tagCache.Values.ToList();
        }

        /// <summary>
        /// 获取所有标签缓存值
        /// </summary>
        public Dictionary<string, IOTTreeTagVal> GetTagValuesMap()
        {
            return _tagCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public List<IOTTreeTag> GetRegisterTags()
        {
            List<IOTTreeTagVal> tvs = GetTagValues();
            List<IOTTreeTag> rets = new List<IOTTreeTag>();
            foreach(IOTTreeTagVal tv in tvs)
            {
                rets.Add(tv.Tag);
            }
            return rets;
        }

        /// <summary>
        /// 启动客户端
        /// </summary>
        public void Start()
        {
            if (State != ClientState.Disconnected && State != ClientState.Error)
            {
                throw new InvalidOperationException($"Client is already in {State} state");
            }

            _cts = new CancellationTokenSource();
            _workerThread = new Thread(WorkerThreadProc)
            {
                Name = $"IOTTreeClient-{_clientId}",
                IsBackground = true
            };

            _workerThread.Start();
        }

        /// <summary>
        /// 停止客户端
        /// </summary>
        public async Task StopAsync()
        {
            ChangeState(ClientState.Disconnecting, "Stopping client");

            _cts?.Cancel();

            if (_workerThread != null && _workerThread.IsAlive)
            {
                await Task.Run(() => _workerThread.Join(5000));
            }

            await CleanupResources();
            ChangeState(ClientState.Disconnected, "Client stopped");
        }

        /// <summary>
        /// write value to tag,it may cause device driver to do write operation
        /// </summary>
        public async Task<bool> WriteTagValueAsync(string tagPath, string value)
        {
            try
            {
                if (!IsConnected)
                {
                    throw new InvalidOperationException("Client is not connected");
                }

                var request = new ReqTagW
                {
                    ClientId = _clientId,
                    TagPath = tagPath,
                    StrVal = value
                };

                var result = await _client.writeTagValAsync(request, _headers);
                return result.Succ;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new Exception($"WriteTagValueAsync Error: {ex.Message}", ex));
                throw;
            }
        }

        /// <summary>
        /// set tag value in memory
        /// </summary>
        public async Task<bool> SetTagValueAsync(string tagPath, string value)
        {
            try
            {
                if (!IsConnected)
                {
                    throw new InvalidOperationException("Client is not connected");
                }

                var request = new ReqTagW
                {
                    ClientId = _clientId,
                    TagPath = tagPath,
                    StrVal = value
                };

                var result = await _client.setTagValAsync(request, _headers);
                return result.Succ;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new Exception($"SetTagValueAsync Error: {ex.Message}", ex));
                throw;
            }
        }

        /// <summary>
        /// get project list in IOT-Tree Server
        /// </summary>
        public async Task<List<PrjItem>> ReadProjectListAsync()
        {
            if(_client==null)
                return new List<PrjItem>();
            try
            {
                var result = await _client.listPrjsAsync(new ReqClient { ClientId = _clientId }, _headers);
                if (result == null)
                {
                    return new List<PrjItem>();
                }
                return result.Prjs.ToList();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new Exception($"GetProjectListAsync Error: {ex.Message}", ex));
                throw;
            }
        }

        /// <summary>
        /// get tags in project
        /// </summary>
        public async Task<List<TagItem>> ReadTagsInProjectAsync(string projectName)
        {
            try
            {
                var request = new ReqPrj
                {
                    ClientId = _clientId,
                    PrjName = projectName
                };
                var result = await _client.listTagsInPrjAsync(request, _headers);
                return result.Tags.ToList();
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new Exception($"GetTagsInProjectAsync Error: {ex.Message}", ex));
                throw;
            }
        }

        /// <summary>
        /// work main thread loop
        /// </summary>
        private async void WorkerThreadProc()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    await ConnectAsync();

                    // 设置同步标签路径
                    await SetupTagPathsAsync();

                    // 开始同步
                    await StartSyncingAsync();

                    OnConnectionLost();

                    if (!_cts.IsCancellationRequested)
                    {
                        ChangeState(ClientState.Disconnected, "Connection lost, waiting to reconnect");
                        
                        await Task.Delay(_reconnectIntervalMs, _cts.Token);
                    }

                    //ChangeState(ClientState.Disconnected, "Connection lost, waiting to reconnect");
                }
                catch (OperationCanceledException)
                {
                    // normal cancel
                    break;
                }
                catch (Exception ex)
                {
                    OnErrorOccurred(new Exception($"WorkerThreadProc Error: {ex.Message}", ex));
                    if (!_cts.IsCancellationRequested)
                    {
                        await Task.Delay(_reconnectIntervalMs, _cts.Token);
                    }
                }
                finally
                {
                    if(State==ClientState.Connected)
                    {
                        OnConnectionLost();
                        ChangeState(ClientState.Disconnected, "Connection lost, waiting to reconnect");
                    }
                        

                }
            }
        }


        /// <summary>
        /// 建立gRPC连接
        /// </summary>
        async private Task ConnectAsync()
        {
            ChangeState(ClientState.Connecting, "Connecting to server");

            try
            {
                _channel = GrpcChannel.ForAddress(_serverAddress);

                
                //Console.WriteLine(" - -1  " + _serverAddress);
                _client = new IOTTreeServer.IOTTreeServerClient(_channel);
                //Console.WriteLine(" - -2  " + _serverAddress);
                // test channel
                var testRequest = new ReqClient { ClientId = _clientId };
                await _client.listPrjsAsync(testRequest,_headers);

                //Console.WriteLine(" - -3  " + _serverAddress);
                ChangeState(ClientState.Connected, "Connected to server");
                OnConnectionRestored();
            }
            catch (Exception ex)
            {
                await CleanupResources();
                throw new Exception($"ConnectAsync Error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task SetupTagPathsAsync()
        {
            lock (_syncLock)
            {
                if (_subscribeTagPaths.Count == 0)
                {
                    return;
                }
            }

            try
            {
                var request = new ReqTagPaths
                {
                    ClientId = _clientId
                };

                lock (_syncLock)
                {
                    request.TagPaths.AddRange(_subscribeTagPaths);
                }

                var response = await _client.setSynTagPathAsync(request, _headers);

                // 初始化标签缓存
                InitializeTagCache(response.Tags);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to setup tag paths: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// init tag cache
        /// </summary>
        private void InitializeTagCache(RepeatedField<TagItem> tags)
        {
            lock (_syncLock)
            {
                foreach (var tag in tags)
                {
                    IOTTreeTag ttag = new IOTTreeTag(tag);
                    var cacheItem = new IOTTreeTagVal(ttag, -1, -1, null, false);
                    _tagCache[tag.Path] = cacheItem;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async Task StartSyncingAsync()
        {
            ChangeState(ClientState.Syncing, "Starting tag synchronization");

            var request = new ReqClient { ClientId = _clientId };
            _streamCall = _client.startSyn(request, _headers);
            _lastHeartbeatTime = DateTime.UtcNow;

            try
            {
                // 启动心跳检测任务
                var heartbeatTask = StartHeartbeatCheckAsync();

                // 读取流数据
                await foreach (var tagVal in _streamCall.ResponseStream.ReadAllAsync(_cts.Token))
                {
                    _lastHeartbeatTime = DateTime.UtcNow;
                    ProcessTagUpdate(tagVal);
                }

                // 等待心跳检测任务完成
                await heartbeatTask;
            }
            catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.Cancelled)
            {
                // 正常取消
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            finally
            {
                await StopSyncingAsync();
            }
        }

        /// <summary>
        /// 处理标签更新
        /// </summary>
        private void ProcessTagUpdate(TagSynVals tagVals)
        {
            foreach (TagSynVal tagVal in tagVals.TagVals)
            {
                if (_tagCache.TryGetValue(tagVal.TagPath, out var cacheItem))
                {
                    cacheItem.setUpdateVal(tagVal.UpdateDt, tagVal.ChangeDt, tagVal.Valid, tagVal.StrVal);
                    
                    OnTagValueChanged(new TagValueChangedEventArgs
                    {
                        TagVal = cacheItem
                    });
                }
            }
        }

        /// <summary>
        /// 启动心跳检测
        /// </summary>
        private async Task StartHeartbeatCheckAsync()
        {
            while (!_cts.IsCancellationRequested && State == ClientState.Syncing)
            {
                await Task.Delay(1000, _cts.Token);

                var elapsed = DateTime.UtcNow - _lastHeartbeatTime;
                if (elapsed.TotalMilliseconds > _heartbeatTimeoutMs)
                {
                    throw new TimeoutException($"Heartbeat timeout: No data received for {elapsed.TotalSeconds:F1} seconds");
                }
            }
        }

        /// <summary>
        /// 停止同步
        /// </summary>
        private async Task StopSyncingAsync()
        {
            try
            {
                if (_streamCall != null)
                {
                    _streamCall.Dispose();
                    _streamCall = null;
                }

                if (_client != null)
                {
                    var request = new ReqClient { ClientId = _clientId };
                    await _client.stopSynAsync(request, _headers);
                }
            }
            catch (Exception ex)
            {
                // 忽略停止同步时的错误
                OnErrorOccurred(new Exception($"StopSyncingAsync Error: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private async Task CleanupResources()
        {
            try
            {
                if (_streamCall != null)
                {
                    _streamCall.Dispose();
                    _streamCall = null;
                }

                if (_channel != null) // && _channel.State != ChannelState.Shutdown)
                {
                    await _channel.ShutdownAsync();
                    _channel = null;
                }

                _client = null;
            }
            catch (Exception ex)
            {
                OnErrorOccurred(new Exception($"CleanupResources Error: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// 变更客户端状态
        /// </summary>
        private void ChangeState(ClientState newState, string message = null)
        {
            var oldState = State;
            State = newState;

            OnStateChanged(new ClientStateChangedEventArgs
            {
                OldState = oldState,
                NewState = newState,
                Message = message
            });
        }

        /// <summary>
        /// 触发标签值变更事件
        /// </summary>
        protected virtual void OnTagValueChanged(TagValueChangedEventArgs e)
        {
            TagValueChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 触发状态变更事件
        /// </summary>
        protected virtual void OnStateChanged(ClientStateChangedEventArgs e)
        {
            StateChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 触发错误事件
        /// </summary>
        protected virtual void OnErrorOccurred(Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }

        /// <summary>
        /// 触发连接断开事件
        /// </summary>
        protected virtual void OnConnectionLost()
        {
            ConnectionLost?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 触发连接恢复事件
        /// </summary>
        protected virtual void OnConnectionRestored()
        {
            ConnectionRestored?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            Dispose();
        }

        /// <summary>
        /// release
        /// </summary>
        public async void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            try
            {
                await StopAsync();
            }
            catch
            {
                // 忽略释放时的错误
            }

            _cts?.Dispose();
        }
    }

    /// <summary>
    /// 标签缓存项
    /// </summary>
    internal class TagCacheItem
    {
        /// <summary>
        /// 标签信息
        /// </summary>
        public TagItem TagInfo { get; set; }

        /// <summary>
        /// 最后的值
        /// </summary>
        public string LastValue { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 最后变更时间
        /// </summary>
        public DateTime LastChangeTime { get; set; }
    }
}