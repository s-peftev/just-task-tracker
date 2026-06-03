using FluentValidation;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using System.Reflection;

namespace JustTaskTracker.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates the incoming request using all registered
/// <see cref="IValidator{TRequest}"/> implementations before the handler runs.
/// When validation fails the behavior short-circuits the pipeline and returns a
/// <typeparamref name="TResponse"/> failure carrying <see cref="GeneralErrors.InvalidRequest"/>
/// with per-field messages attached as <see cref="Error.Details"/>.
/// No exceptions are thrown; errors flow through the Result pattern.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!validators.Any())
            return await next(ct);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(e => e is not null)
            .ToList();

        if (failures.Count == 0)
            return await next(ct);

        var error = GeneralErrors.InvalidRequest with
        {
            Details = failures.Select(e => e.ErrorMessage)
        };

        return CreateFailure(error);
    }

    private static TResponse CreateFailure(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        var failureMethod = typeof(TResponse).GetMethod(
            nameof(Result<object>.Failure),
            BindingFlags.Public | BindingFlags.Static,
            [typeof(Error)])!;

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}
