namespace ImgResizer.Domain.Common;

/// <summary>
/// 処理結果を表す基底クラス（値なし）
/// 正常系と異常系を明示的に表現するためのResultパターン実装
/// </summary>
public class Result
{
    /// <summary>
    /// 処理が成功したかどうか
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 処理が失敗したかどうか
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// エラーコード（失敗時のみ）
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// エラーメッセージ（失敗時のみ）
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// コンストラクタ（protected）
    /// </summary>
    /// <param name="isSuccess">成功フラグ</param>
    /// <param name="errorCode">エラーコード</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <exception cref="InvalidOperationException">成功時にエラー情報が設定されている、または失敗時にエラーコードがない場合</exception>
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
    /// 成功結果を作成
    /// </summary>
    /// <returns>成功を表すResult</returns>
    public static Result Success() => new(true, string.Empty, string.Empty);

    /// <summary>
    /// 失敗結果を作成
    /// </summary>
    /// <param name="errorCode">エラーコード</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>失敗を表すResult</returns>
    public static Result Failure(string errorCode, string errorMessage)
        => new(false, errorCode, errorMessage);

    /// <summary>
    /// 成功結果を作成（値あり）
    /// </summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="value">成功時の値</param>
    /// <returns>成功を表すResult&lt;T&gt;</returns>
    public static Result<T> Success<T>(T value)
        => new(value, true, string.Empty, string.Empty);

    /// <summary>
    /// 失敗結果を作成（値あり）
    /// </summary>
    /// <typeparam name="T">値の型</typeparam>
    /// <param name="errorCode">エラーコード</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>失敗を表すResult&lt;T&gt;</returns>
    public static Result<T> Failure<T>(string errorCode, string errorMessage)
        => new(default!, false, errorCode, errorMessage);
}

/// <summary>
/// 処理結果を表すクラス（値あり）
/// </summary>
/// <typeparam name="T">成功時の値の型</typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// 成功時の値
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// コンストラクタ（internal）
    /// </summary>
    /// <param name="value">成功時の値</param>
    /// <param name="isSuccess">成功フラグ</param>
    /// <param name="errorCode">エラーコード</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    protected internal Result(T value, bool isSuccess, string errorCode, string errorMessage)
        : base(isSuccess, errorCode, errorMessage)
    {
        Value = value;
    }
}

