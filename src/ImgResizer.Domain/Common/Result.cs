namespace ImgResizer.Domain.Common;

/// <summary>
/// 処理結果を表す基底クラス（値なし）。
/// 正常系と異常系を明示的に表現するためのResultパターン実装。
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error code (only when failed).
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the error message (only when failed).
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">成功フラグ.</param>
    /// <param name="errorCode">エラーコード.</param>
    /// <param name="errorMessage">エラーメッセージ.</param>
    /// <exception cref="InvalidOperationException">成功時にエラー情報が設定されている、または失敗時にエラーコードがない場合.</exception>
    protected Result(bool isSuccess, string errorCode, string errorMessage)
    {
        if (isSuccess && (!string.IsNullOrEmpty(errorCode) || !string.IsNullOrEmpty(errorMessage)))
        {
            throw new InvalidOperationException("成功時にエラー情報は設定できません");
        }

        if (!isSuccess && string.IsNullOrEmpty(errorCode))
        {
            throw new InvalidOperationException("失敗時にはエラーコードが必須です");
        }

        IsSuccess = isSuccess;
        ErrorCode = errorCode ?? string.Empty;
        ErrorMessage = errorMessage ?? string.Empty;
    }

    /// <summary>
    /// 成功結果を作成します。
    /// </summary>
    /// <returns>成功を表すResult.</returns>
    public static Result Success() => new(true, string.Empty, string.Empty);

    /// <summary>
    /// 失敗結果を作成します。
    /// </summary>
    /// <param name="errorCode">エラーコード.</param>
    /// <param name="errorMessage">エラーメッセージ.</param>
    /// <returns>失敗を表すResult.</returns>
    public static Result Failure(string errorCode, string errorMessage)
        => new(false, errorCode, errorMessage);

    /// <summary>
    /// 成功結果を作成します（値あり）。
    /// </summary>
    /// <typeparam name="T">値の型.</typeparam>
    /// <param name="value">成功時の値.</param>
    /// <returns>成功を表すResult&lt;T&gt;.</returns>
    public static Result<T> Success<T>(T value)
        => new(value, true, string.Empty, string.Empty);

    /// <summary>
    /// 失敗結果を作成します（値あり）。
    /// </summary>
    /// <typeparam name="T">値の型.</typeparam>
    /// <param name="errorCode">エラーコード.</param>
    /// <param name="errorMessage">エラーメッセージ.</param>
    /// <returns>失敗を表すResult&lt;T&gt;.</returns>
    public static Result<T> Failure<T>(string errorCode, string errorMessage)
        => new(default!, false, errorCode, errorMessage);
}

/// <summary>
/// 処理結果を表すクラス（値あり）。
/// </summary>
/// <typeparam name="T">成功時の値の型.</typeparam>
#pragma warning disable SA1402 // File may only contain a single type
public class Result<T> : Result
#pragma warning restore SA1402 // File may only contain a single type
{
    /// <summary>
    /// Gets the value when successful.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">成功時の値.</param>
    /// <param name="isSuccess">成功フラグ.</param>
    /// <param name="errorCode">エラーコード.</param>
    /// <param name="errorMessage">エラーメッセージ.</param>
    protected internal Result(T value, bool isSuccess, string errorCode, string errorMessage)
        : base(isSuccess, errorCode, errorMessage)
    {
        Value = value;
    }
}
