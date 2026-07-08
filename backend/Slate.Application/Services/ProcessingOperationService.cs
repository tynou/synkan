using System.Collections.Concurrent;
using Slate.Application.Interfaces;

namespace Slate.Application.Services;

public class ProcessingOperationService : IProcessingOperationService
{
    private readonly Dictionary<string, ProcessingOperation> activeOperations = new();
    private readonly Lock syncRoot = new();

    public ProcessingOperation BeginOperation(string connectionId)
    {
        lock (syncRoot)
        {
            var operation = new ProcessingOperation(Guid.NewGuid(), new CancellationTokenSource());

            if (activeOperations.TryGetValue(connectionId, out var existingOperation))
            {
                existingOperation.CancellationTokenSource.Cancel();
                existingOperation.CancellationTokenSource.Dispose();
            }

            activeOperations[connectionId] = operation;
            return operation;
        }
    }

    public void CancelOperation(string connectionId)
    {
        lock (syncRoot)
        {
            if (!activeOperations.Remove(connectionId, out var operation))
                return;

            operation.CancellationTokenSource.Cancel();
            operation.CancellationTokenSource.Dispose();
        }
    }

    public void CompleteOperation(string connectionId, Guid operationId)
    {
        lock (syncRoot)
        {
            if (!activeOperations.TryGetValue(connectionId, out var operation))
                return;
            
            if (operation.OperationId != operationId)
                return;

            activeOperations.Remove(connectionId);
            operation.CancellationTokenSource.Dispose();
        }
    }
}

public record ProcessingOperation(
    Guid OperationId,
    CancellationTokenSource CancellationTokenSource
);