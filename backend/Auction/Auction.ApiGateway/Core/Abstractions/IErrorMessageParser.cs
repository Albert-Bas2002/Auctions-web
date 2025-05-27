
using Auction.ApiGateway.Contracts;

namespace Auction.ApiGateway.Core.Abstractions
{
    public interface IErrorMessageParser
    {
        public ErrorDto ParseMessageToErrorDto(string errorMessage);
    }
}