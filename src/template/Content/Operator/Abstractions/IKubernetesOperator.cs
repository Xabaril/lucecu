using System;
using System.Threading;
using System.Threading.Tasks;

namespace __ProjectName__.Operator.Abstractions
{
    internal interface IKubernetesOperator : IDisposable
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}
