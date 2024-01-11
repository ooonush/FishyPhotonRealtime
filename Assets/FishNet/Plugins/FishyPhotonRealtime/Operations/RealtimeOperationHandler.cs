using System;
using System.Threading;
using System.Threading.Tasks;
using Photon.Realtime;

namespace FishNet.Transporting.PhotonRealtime
{
    internal abstract class RealtimeOperationHandler
    {
        private readonly TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource(TimeSpan.FromSeconds(30.0));
        protected readonly LoadBalancingClient Client;
        public Task Task => _completionSource.Task;

        protected RealtimeOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default)
        {
            Client = client;
            Client.AddCallbackTarget(this);
            _cts.Token.Register(OnTimeout);
            if (cancellationToken != default)
            {
                cancellationToken.Register(Cancel);
            }
        }

        private void End()
        {
            Client.RemoveCallbackTarget(this);
        }

        private void OnTimeout()
        {
            SetException(new TimeoutException("Realtime Operation timed out."));
        }

        protected void Complete()
        {
            End();
            _completionSource.TrySetResult(true);
        }

        public void Cancel()
        {
            End();
            _completionSource.TrySetCanceled();
        }

        protected void SetException(Exception e)
        {
            End();
            if (!_completionSource.TrySetException(e))
            {
                return;
            }
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            _cts.Dispose();
        }
    }
}