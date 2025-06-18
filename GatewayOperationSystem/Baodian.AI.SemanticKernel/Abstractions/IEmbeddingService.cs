using System.Threading;
using System.Threading.Tasks;

namespace Baodian.AI.SemanticKernel.Abstractions
{
    /// <summary>
    /// Interface for generating text embeddings. 
    /// </summary>
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    }
}
