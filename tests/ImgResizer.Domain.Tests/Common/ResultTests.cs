using FluentAssertions;
using ImgResizer.Domain.Common;

namespace ImgResizer.Domain.Tests.Common;

/// <summary>
/// Resultクラスのテスト。
/// テストプロジェクトの構造確認用のサンプルテストです。
/// </summary>
public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorCode.Should().BeEmpty();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange
        const string errorCode = "TEST_ERROR";
        const string errorMessage = "テストエラー";

        // Act
        var result = Result.Failure(errorCode, errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(errorCode);
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessResultWithValue()
    {
        // Arrange
        const string testValue = "テスト値";

        // Act
        var result = Result.Success(testValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(testValue);
        result.ErrorCode.Should().BeEmpty();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithValue_ShouldCreateFailureResult()
    {
        // Arrange
        const string errorCode = "TEST_ERROR";
        const string errorMessage = "テストエラー";

        // Act
        var result = Result.Failure<string>(errorCode, errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be(errorCode);
        result.ErrorMessage.Should().Be(errorMessage);
        result.Value.Should().BeNull();
    }
}

