using Jarstat.Domain.Primitives;
using Jarstat.Domain.Shared;
using MediatR;

namespace Jarstat.Application.Abstractions;

public interface IService<TRequest, TResult, TEntity>
    where TRequest : IRequest<TResult>
    where TResult : Result<TEntity>
    where TEntity : Entity
{
}
