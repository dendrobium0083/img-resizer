using FluentAssertions;
using FluentValidation;
using ImgResizer.Application.Behaviors;
using ImgResizer.Application.Commands;
using ImgResizer.Application.DTOs;
using ImgResizer.Domain.Common;
using MediatR;
using Moq;

namespace ImgResizer.Application.Tests.Behaviors;

/// <summary>
/// ValidationBehaviorのテスト。
/// </summary>
public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_ExecutesNext()
    {
        // Arrange
        var validators = new List<IValidator<ResizeImageCommand>>();
        var behavior = new ValidationBehavior<ResizeImageCommand, Result<ResizeImageResponse>>(validators);
        var request = new ResizeImageCommand("test.jpg", "fit");
        var expectedResult = Result.Success(new ImgResizer.Application.DTOs.ResizeImageResponse
        {
            Success = true,
            Message = "テスト"
        });

        RequestHandlerDelegate<Result<ResizeImageResponse>> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ValidRequest_ExecutesNext()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<ResizeImageCommand>>();
        var validationResult = new FluentValidation.Results.ValidationResult();
        mockValidator
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new List<IValidator<ResizeImageCommand>> { mockValidator.Object };
        var behavior = new ValidationBehavior<ResizeImageCommand, Result<ResizeImageResponse>>(validators);
        var request = new ResizeImageCommand("test.jpg", "fit");
        var expectedResult = Result.Success(new ImgResizer.Application.DTOs.ResizeImageResponse
        {
            Success = true,
            Message = "テスト"
        });

        RequestHandlerDelegate<Result<ResizeImageResponse>> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        mockValidator.Verify(
            v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ReturnsFailure()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<ResizeImageCommand>>();
        var validationFailure = new FluentValidation.Results.ValidationFailure("FilePath", "ファイルパスが指定されていません")
        {
            ErrorCode = "VALIDATION_ERROR"
        };
        var validationResult = new FluentValidation.Results.ValidationResult(new[] { validationFailure });
        mockValidator
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new List<IValidator<ResizeImageCommand>> { mockValidator.Object };
        var behavior = new ValidationBehavior<ResizeImageCommand, Result<ResizeImageResponse>>(validators);
        var request = new ResizeImageCommand(string.Empty, "fit");

        RequestHandlerDelegate<Result<ResizeImageResponse>> next = () => Task.FromResult(
            Result.Success(new ImgResizer.Application.DTOs.ResizeImageResponse()));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.ErrorMessage.Should().Be("ファイルパスが指定されていません");

        // 次のハンドラーは呼ばれない
        // (nextは呼ばれないことを確認するため、呼び出し回数を検証できないが、
        // 失敗結果が返されることで間接的に確認できる)
    }

    [Fact]
    public async Task Handle_MultipleValidators_AllPass_ExecutesNext()
    {
        // Arrange
        var mockValidator1 = new Mock<IValidator<ResizeImageCommand>>();
        var mockValidator2 = new Mock<IValidator<ResizeImageCommand>>();
        var validationResult = new FluentValidation.Results.ValidationResult();

        mockValidator1
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        mockValidator2
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new List<IValidator<ResizeImageCommand>>
        {
            mockValidator1.Object,
            mockValidator2.Object
        };
        var behavior = new ValidationBehavior<ResizeImageCommand, Result<ResizeImageResponse>>(validators);
        var request = new ResizeImageCommand("test.jpg", "fit");
        var expectedResult = Result.Success(new ImgResizer.Application.DTOs.ResizeImageResponse
        {
            Success = true,
            Message = "テスト"
        });

        RequestHandlerDelegate<Result<ResizeImageResponse>> next = () => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockValidator1.Verify(
            v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        mockValidator2.Verify(
            v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleValidators_OneFails_ReturnsFailure()
    {
        // Arrange
        var mockValidator1 = new Mock<IValidator<ResizeImageCommand>>();
        var mockValidator2 = new Mock<IValidator<ResizeImageCommand>>();
        var validationResult1 = new FluentValidation.Results.ValidationResult();
        var validationFailure = new FluentValidation.Results.ValidationFailure("FilePath", "ファイルパスが指定されていません")
        {
            ErrorCode = "VALIDATION_ERROR"
        };
        var validationResult2 = new FluentValidation.Results.ValidationResult(new[] { validationFailure });

        mockValidator1
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult1);

        mockValidator2
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult2);

        var validators = new List<IValidator<ResizeImageCommand>>
        {
            mockValidator1.Object,
            mockValidator2.Object
        };
        var behavior = new ValidationBehavior<ResizeImageCommand, Result<ResizeImageResponse>>(validators);
        var request = new ResizeImageCommand(string.Empty, "fit");

        RequestHandlerDelegate<Result<ResizeImageResponse>> next = () => Task.FromResult(
            Result.Success(new ImgResizer.Application.DTOs.ResizeImageResponse()));

        // Act
        var result = await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        result.ErrorMessage.Should().Be("ファイルパスが指定されていません");
    }

    [Fact]
    public async Task Handle_CancellationToken_PropagatesCancellation()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<ResizeImageCommand>>();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        mockValidator
            .Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<ResizeImageCommand>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var validators = new List<IValidator<ResizeImageCommand>> { mockValidator.Object };
        var behavior = new ValidationBehavior<ResizeImageCommand, Result<ResizeImageResponse>>(validators);
        var request = new ResizeImageCommand("test.jpg", "fit");

        RequestHandlerDelegate<Result<ResizeImageResponse>> next = () => Task.FromResult(
            Result.Success(new ImgResizer.Application.DTOs.ResizeImageResponse()));

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await behavior.Handle(request, next, cancellationTokenSource.Token));
    }
}

