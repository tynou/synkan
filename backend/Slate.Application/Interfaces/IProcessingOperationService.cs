using Slate.Application.Services;

namespace Slate.Application.Interfaces;

public interface IProcessingOperationService
{
    ProcessingOperation BeginOperation(string connectionId);

    void CancelOperation(string connectionId);

    void CompleteOperation(string connectionId, Guid operationId);
}