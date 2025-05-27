using Auction.UserAuthService.Contracts.Dtos;

namespace Auction.UserAuthService.Core.Abstractions;

    public interface IErrorMessageParser
    {
       public ErrorDto ParseMessageToErrorDto(string errorMessage);
    }
