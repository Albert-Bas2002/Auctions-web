using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Contracts.ApiGatewayContracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts.Dtos;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto;
using Auction.ApiGateway.Core.Abstractions;
using Auction.ApiGateway.Infrastructure.Authorisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auction.ApiGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionService _auctionService;
        private readonly IErrorMessageParser _errorMessageParser;
        private readonly string _baseUploadPath;


        public AuctionController(IAuctionService auctionService, IErrorMessageParser errorMessageParser)
        {
            _auctionService = auctionService;
            _errorMessageParser = errorMessageParser;
            var currentDir = Directory.GetCurrentDirectory();
            _baseUploadPath = Path.Combine(currentDir, "Photos");

        }
        [HttpGet("auction/{auctionId:guid}")]
        public async Task<ActionResult<ApiGatewayResponse<AuctionPageDto>>> GetAuctionPage(Guid auctionId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userId))
               {
                userId= Guid.Empty.ToString();
                }
            var auctionPageResult = await _auctionService.GetAuctionPage(auctionId, Guid.Parse(userId));
 
            if (auctionPageResult.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<AuctionPageDto>{
                Data= auctionPageResult.Value
                });
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(auctionPageResult.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });

            }


        }
            [Authorize]
            [Permission("User-Permission")]
            [HttpPost("auction/create")]
            public async Task<ActionResult<ApiGatewayResponse<object>>> CreateAuction( AuctionCreateRequest auctionCreateRequest)
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiGatewayResponse<object>
                    {
                        Error = new ErrorDto
                        {
                            Error = "Invalid token",
                            Details = "UserId not found in token",
                            Status = 401
                        }
                    });
            var auctionCreateDto = new AuctionCreateDto { 
                Title = auctionCreateRequest.Title,
                Description = auctionCreateRequest.Description,
                AuctionDurationInDays= auctionCreateRequest.AuctionDurationInDays,
                Reserve = auctionCreateRequest.Reserve
            };
                var auctionCreateResult = await _auctionService.CreateAuction(Guid.Parse(userId), auctionCreateDto);

                if (auctionCreateResult.IsSuccess)
                {
                    return Ok(new ApiGatewayResponse<object>());

                }
                else
                {
                    var error = _errorMessageParser.ParseMessageToErrorDto(auctionCreateResult.Error);
                    return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });

                }

            }
      
            [Authorize]
            [Permission("Moderator-Permission")]
            [HttpPost("auction/close/moderator/{auctionId:guid}")]
            public async Task<ActionResult<ApiGatewayResponse<object>>> CloseAuctionByModerator(Guid auctionId)
            {
                var auctionUpdateResult = await _auctionService.CloseAuction(auctionId,"Moderator");

                if (auctionUpdateResult.IsSuccess)
                {
                    return Ok(new ApiGatewayResponse<object>());
                }
                else
                {
                    var error = _errorMessageParser.ParseMessageToErrorDto(auctionUpdateResult.Error);
                    return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });

                }

            }
        [Authorize]
        [Permission("User-Permission")]
        [HttpPost("auction/close/{auctionId:guid}")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> CloseAuctionByCreator(Guid auctionId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });
            var auctionUpdateResult = await _auctionService.CloseAuction(auctionId,"Creator", Guid.Parse(userId));
           
            if (auctionUpdateResult.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<object>());
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(auctionUpdateResult.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });

            }

        }
        [Authorize]
        [Permission("User-Permission")]
        [HttpPost("auction/winner-creator-info/{auctionId:guid}")]
        public async Task<ActionResult<ApiGatewayResponse<ContactDto>>> GetCreatorOrWinnerId(Guid auctionId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<ContactDto>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });
            var creatorOrWinnerIdResult = await _auctionService.GetCreatorOrWinnerId(auctionId, Guid.Parse(userId));
            
            if (creatorOrWinnerIdResult.IsSuccess)
            {
            return Ok(new ApiGatewayResponse<ContactDto> { Data = creatorOrWinnerIdResult.Value });
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(creatorOrWinnerIdResult.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<ContactDto> { Error = error });

            }

        }

        [Authorize]
        [Permission("User-Permission")]
        [HttpPost("auction/complete-deal/{auctionId:guid}")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> AuctionDealComplete(Guid auctionId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });

            var auctionUpdateResult = await _auctionService.AuctionCompleteDeal(auctionId, Guid.Parse(userId));

            if (auctionUpdateResult.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<object>());

            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(auctionUpdateResult.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<object> { Error = error });

            }

        }
       
            [Authorize]
            [Permission("User-Permission")]
            [HttpGet("auctions/category")]
            public async Task<ActionResult<ApiGatewayResponse<List<AuctionListItemDto>>>> GetAuctionForUser([FromQuery] string category,[FromQuery] bool? active = null)
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new ApiGatewayResponse<object>
                    {
                        Error = new ErrorDto
                        {
                            Error = "Invalid token",
                            Details = "UserId not found in token",
                            Status = 401
                        }
                    });

                var auctionList = await _auctionService.GetAuctionsForUser(Guid.Parse(userId),category,active);

                if (auctionList.IsSuccess)
                {
                return Ok(new ApiGatewayResponse<List<AuctionListItemDto>> {
                    Data = auctionList.Value
                });

                }
                else
                {
                    var error = _errorMessageParser.ParseMessageToErrorDto(auctionList.Error);
                    return StatusCode(error.Status, new ApiGatewayResponse<List<AuctionListItemDto>> { Error = error });

                }
            }

        [HttpGet("auctions")]
        public async Task<ActionResult<ApiGatewayResponse<List<AuctionListItemDto>>>> GetAuction(
            [FromQuery] string? sortType,
            [FromQuery] int page = 1)
        {
            var auctionList = await _auctionService.GetAuctions(sortType, page);

            if (auctionList.IsSuccess)
            {
                return Ok(new ApiGatewayResponse<List<AuctionListItemDto>>
                {
                    Data = auctionList.Value
                });
            }
            else
            {
                var error = _errorMessageParser.ParseMessageToErrorDto(auctionList.Error);
                return StatusCode(error.Status, new ApiGatewayResponse<List<AuctionListItemDto>> { Error = error });
            }
        }
        [Authorize]
        [Permission("User-Permission")]
        [HttpPost("auction/{auctionId:guid}/upload-photos")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> UploadPhotos(Guid auctionId, [FromForm] List<IFormFile> photos)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });
            var uploadResult = await _auctionService.UploadPhotosForAuction(auctionId, photos, Guid.Parse(userId));
            if (uploadResult.IsFailure)
            {
                return StatusCode(400, new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Upload failed",
                        Details = uploadResult.Error,
                        Status = 400
                    }
                });
            }
            else return Ok(new ApiGatewayResponse<object>());
        }
        [Authorize]
        [Permission("User-Permission")]
        [HttpPost("auction/{auctionId:guid}/delete-photos")]
        public async Task<ActionResult<ApiGatewayResponse<object>>> DeletePhotos(Guid auctionId, [FromBody] int[] indexesToDelete)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Invalid token",
                        Details = "UserId not found in token",
                        Status = 401
                    }
                });
            var deleteResult = await _auctionService.DeletePhotosForAuction(auctionId, indexesToDelete, Guid.Parse(userId));

            if (deleteResult.IsFailure)
            {
                return StatusCode(400, new ApiGatewayResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Delete failed",
                        Details = deleteResult.Error,
                        Status = 400
                    }
                });
            }
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            Response.Headers["Surrogate-Control"] = "no-store";
            return Ok(new ApiGatewayResponse<object>());
        }
        [HttpGet("photo/auction/{auctionId:guid}/index/{index:int}")]
        public IActionResult GetPhotoByIndex(Guid auctionId, int index)
        {
            var auctionFolder = Path.Combine(_baseUploadPath, auctionId.ToString());

            if (!Directory.Exists(auctionFolder))
                return NotFound();

            var files = Directory.GetFiles(auctionFolder)
                                 .OrderBy(f => f)
                                 .ToList();

            if (index < 0 || index >= files.Count)
                return NotFound();

            var filePath = files[index];
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
            Response.Headers["Surrogate-Control"] = "no-store";
            return PhysicalFile(filePath, contentType);
        }



    }
}
