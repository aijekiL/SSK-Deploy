using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venus.Face
{
    public class SingleThreadExecutor
    {
        private readonly BlockingCollection<WorkItem> _taskQueue = new BlockingCollection<WorkItem>();
        private readonly Thread _executionThread;
        private readonly ManualResetEventSlim _threadReady = new ManualResetEventSlim(false);
        private bool _disposed;

        public SingleThreadExecutor()
        {
            // 创建专用线程
            _executionThread = new Thread(ExecuteTasks)
            {
                IsBackground = true,
                Name = $"SdkExecutorThread-{Guid.NewGuid():N}"
            };

            // 启动线程
            _executionThread.Start();

            // 等待线程初始化完成
            _threadReady.Wait();
        }

        // 执行任务并返回结果
        public T Execute<T>(Func<T> action)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SingleThreadExecutor));

            var workItem = new WorkItem<T>(action);
            _taskQueue.Add(workItem);
            workItem.Completion.Wait();

            if (workItem.Exception != null)
                throw new AggregateException("执行SDK方法时发生异常", workItem.Exception);

            return ((WorkItem<T>)workItem).Result;
        }

        // 异步执行（仍在同一专用线程上）
        public Task<T> ExecuteAsync<T>(Func<T> action)
        {
            return Task.Run(() => Execute(action));
        }

        // 执行无返回值的操作
        public void Execute(Action action)
        {
            Execute<object>(() =>
            {
                action();
                return null;
            });
        }

        // 线程主循环
        private void ExecuteTasks()
        {
            try
            {
                // 通知主线程当前线程已启动并就绪
                _threadReady.Set();

                // 持续处理队列中的任务
                foreach (var workItem in _taskQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        workItem.Invoke();
                    }
                    catch (Exception ex)
                    {
                        workItem.Exception = ex;
                    }
                    finally
                    {
                        workItem.Completion.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                // 处理线程异常（例如记录日志）
                Console.WriteLine($"执行器线程异常: {ex}");
            }
        }

        // 释放资源
        public void Dispose()
        {
            if (!_disposed)
            {
                _taskQueue.CompleteAdding();

                // 等待线程结束（最多500ms）
                if (!_executionThread.Join(500))
                {
                    Console.WriteLine("警告: 执行器线程未能及时终止");
                }

                _taskQueue.Dispose();
                _threadReady.Dispose();
                _disposed = true;
            }
        }

        // 工作项抽象类
        private abstract class WorkItem
        {
            public ManualResetEventSlim Completion { get; } = new ManualResetEventSlim(false);
            public Exception Exception { get; set; }
            public abstract void Invoke();
        }

        // 带返回值的工作项
        private sealed class WorkItem<T> : WorkItem
        {
            private readonly Func<T> _action;
            public T Result { get; private set; }

            public WorkItem(Func<T> action)
            {
                _action = action ?? throw new ArgumentNullException(nameof(action));
            }

            public override void Invoke()
            {
                Result = _action();
            }
        }
    }
}
