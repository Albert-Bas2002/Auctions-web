
using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Core.Abstractions;

namespace Auction.ApiGateway.Infrastructure
{
    public class ErrorMessageParser : IErrorMessageParser
    {
        public  ErrorDto ParseMessageToErrorDto(string errorMessage)
        {
            var parts = errorMessage.Split(new[] { ", " }, StringSplitOptions.None);
            var errorDto = new ErrorDto();

            foreach (var part in parts)
            {
                if (part.StartsWith("Error:"))
                {
                    errorDto.Error = part.Substring("Error:".Length).Trim();
                }
                else if (part.StartsWith("Details:"))
                {
                    errorDto.Details = part.Substring("Details:".Length).Trim();
                }
                else if (part.StartsWith("Status:"))
                {
                    if (int.TryParse(part.Substring("Status:".Length).Trim(), out var status))
                    {
                        errorDto.Status = status;
                    }
                }
            }

            return errorDto;
        }
    }


}
