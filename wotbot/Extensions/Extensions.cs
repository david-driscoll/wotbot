using System;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace wotbot
{
    public static class Extensions
    {
        public static Task WaitUntilCancelled(this CancellationToken cancellationToken)
        {
            var cts = new TaskCompletionSource<Unit>();
            cancellationToken.Register(() => { cts.TrySetResult(Unit.Default); });

            return cts.Task;
        }
    }
}
