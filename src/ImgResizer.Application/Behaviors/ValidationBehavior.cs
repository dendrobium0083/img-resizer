using FluentValidation;
using ImgResizer.Domain.Common;
using MediatR;

namespace ImgResizer.Application.Behaviors;

/// <summary>
/// MediatRパイプラインでバリデーションを実行するビヘイビア。
/// FluentValidationを使用してリクエストのバリデーションを行います。
/// </summary>
/// <typeparam name="TRequest">リクエストの型。</typeparam>
/// <typeparam name="TResponse">レスポンスの型。</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="validators">リクエストに対するバリデーターのコレクション。</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// パイプラインを処理します。
    /// バリデーションエラーがある場合は、次のハンドラーを呼び出さずに失敗結果を返します。
    /// </summary>
    /// <param name="request">リクエスト。</param>
    /// <param name="next">次のハンドラーへのデリゲート。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>レスポンス。</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var firstFailure = failures[0];
            return CreateFailureResult<TResponse>(firstFailure.ErrorCode, firstFailure.ErrorMessage);
        }

        return await next();
    }

    /// <summary>
    /// 失敗結果を作成します。
    /// </summary>
    /// <typeparam name="T">レスポンスの型。</typeparam>
    /// <param name="errorCode">エラーコード。</param>
    /// <param name="errorMessage">エラーメッセージ。</param>
    /// <returns>失敗結果。</returns>
    private static T CreateFailureResult<T>(string errorCode, string errorMessage)
        where T : Result
    {
        // Result<TValue>型の場合
        var resultType = typeof(T);
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = resultType.GetGenericArguments()[0];
            var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure), 1, new[] { typeof(string), typeof(string) });
            var genericFailureMethod = failureMethod!.MakeGenericMethod(valueType);
            return (T)genericFailureMethod.Invoke(null, new object[] { errorCode, errorMessage })!;
        }

        // 単純なResult型の場合
        return (T)Result.Failure(errorCode, errorMessage);
    }
}
