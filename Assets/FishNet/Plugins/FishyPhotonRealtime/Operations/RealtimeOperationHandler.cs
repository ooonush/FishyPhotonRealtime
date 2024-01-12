using System;
using System.Threading;
using System.Threading.Tasks;
using Photon.Realtime;
using UnityEngine;

namespace FishNet.Transporting.PhotonRealtime
{
    internal abstract class RealtimeOperationHandler : RealtimeOperationHandler<object>
    {
        public new Task Task => base.Task;

        protected RealtimeOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default) : base(client, cancellationToken)
        {
        }

        protected void Complete()
        {
            Complete(null);
        }
    }

    internal abstract class RealtimeOperationHandler<T>
    {
        private readonly TaskCompletionSource<T> _completionSource = new TaskCompletionSource<T>();
        private readonly CancellationTokenSource _timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(30.0));
        protected readonly LoadBalancingClient Client;
        public Task<T> Task => _completionSource.Task;

        private bool _isEnded;
        private readonly CancellationTokenRegistration _timeoutRegistration;
        private readonly CancellationTokenRegistration _cancellationRegistration;

        protected RealtimeOperationHandler(LoadBalancingClient client, CancellationToken cancellationToken = default)
        {
            Debug.Log("Start: " + GetType().Name);
            Client = client;
            Client.AddCallbackTarget(this);
            _timeoutRegistration = _timeoutSource.Token.Register(OnTimeout);
            _cancellationRegistration = cancellationToken.Register(Cancel);
        }

        private void Cleanup()
        {
            Debug.Log("End: " + GetType().Name);
            if (_isEnded)
            {
                Debug.LogWarning("Operation already cleaned.");
                return;
            }
            _isEnded = true;
            _timeoutSource.Dispose();
            _timeoutRegistration.Dispose();
            _cancellationRegistration.Dispose();
            Client.RemoveCallbackTarget(this);
        }

        private void OnTimeout()
        {
            SetException(new TimeoutException("Realtime Operation timed out."));
        }

        protected void Complete(T result)
        {
            Cleanup();
            _completionSource.SetResult(result);
        }

        public void Cancel()
        {
            Cleanup();
            OnCancel();
            _completionSource.SetCanceled();
        }

        protected virtual void OnCancel()
        {
        }

        protected void SetException(Exception e)
        {
            Cleanup();
            _completionSource.SetException(e);
        }
    }
}