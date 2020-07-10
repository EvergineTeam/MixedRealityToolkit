using System;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace WaveEngine_MRTK_Demo.Common.Extensions
{
    public static class DispatcherExtensions
    {
        public static async Task RunAsync(this CoreDispatcher dispatcher, Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) =>
        await dispatcher.RunAsync(priority, new DispatchedHandler(action));

        public static async Task<T> RunAsync<T>(this CoreDispatcher dispatcher, Func<T> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, () =>
            {
                try
                {
                    taskCompletionSource.SetResult(func());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }

        public static async Task RunAsync(this CoreDispatcher dispatcher, Func<Task> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) =>
        await RunAsync(dispatcher, async () => { await func(); return false; }, priority);

        public static async Task<T> RunAsync<T>(this CoreDispatcher dispatcher, Func<Task<T>> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            await dispatcher.RunAsync(priority, async () =>
            {
                try
                {
                    taskCompletionSource.SetResult(await func());
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });
            return await taskCompletionSource.Task;
        }
    }
}
