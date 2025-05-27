using Auction.AuctionService.Contracts;
using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Core.Models;
using Auction.AuctionService.Core.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Auction.AuctionService.Controllers
{
    [ApiController]
    [Route("api-auctions/")]
    public class AuctionController : ControllerBase
    {
        private readonly IAuctionCreateUpdateService _auctionCreateUpdateService;
        private readonly IAuctionStatusService _auctionStatusService;
        private readonly IUserCategoryService _userCategoryService;
        private readonly IAuctionGetDetailsService _auctionGetDetailsService;
        private readonly IBidService _bidService;

        public AuctionController(IAuctionCreateUpdateService auctionCreateUpdateService,
            IAuctionStatusService auctionStatusService,
            IUserCategoryService userCategoryService,
            IAuctionGetDetailsService auctionGetService,
            IBidService bidService)
        {
            _auctionCreateUpdateService = auctionCreateUpdateService;
            _userCategoryService = userCategoryService;
            _auctionStatusService = auctionStatusService;
            _auctionGetDetailsService = auctionGetService;
            _bidService = bidService;
           
        }

        [HttpGet("auctions")]
        public async Task<ActionResult<AuctionServiceApiResponse<List<AuctionListItemDto>>>> GetActiveAuctionsInfo(
             [FromQuery] string sortType,
             [FromQuery] int page = 1)
        {
            var auctionsDetails = await _auctionGetDetailsService.GetAll(sortType, true, page);

            if (auctionsDetails == null)
            {
                return Ok(new AuctionServiceApiResponse<List<AuctionListItemDto>>
                {
                    Data = new List<AuctionListItemDto>()
                });
            }

            var auctionIds = auctionsDetails.Select(ad => ad.AuctionId).ToList();
            var currentBids = await _bidService.GetMaxByAuctionIds(auctionIds);

            var auctionListDto = auctionsDetails.Select(detail =>
            {
                var bid = currentBids.FirstOrDefault(b => b.AuctionId == detail.AuctionId);
                return new AuctionListItemDto
                {
                    AuctionId = detail.AuctionId,
                    CreationTime = detail.CreationTime,
                    EndTime = detail.EndTime,
                    Title = detail.GetTitle(),
                    Reserve = detail.Reserve,
                    CurrentBid = bid?.GetValue() ?? 0
                };
            }).ToList();

            return Ok(new AuctionServiceApiResponse<List<AuctionListItemDto>>
            {
                Data = auctionListDto
            });
        }

        [HttpGet("auctions/count")]
        public async Task<ActionResult<AuctionServiceApiResponse<AuctionCountDto>>> GetAuctionCount([FromQuery] bool isActive)
        {
            var auctionCount = await _auctionGetDetailsService.GetAuctionCount(isActive);

            if (auctionCount == null)
            {
                return StatusCode(400, new AuctionServiceApiResponse<AuctionCountDto>
                {
                    Error = new ErrorDto
                    {
                        Error = "Unable to retrieve auction count",
                        Details = "An error occurred while fetching the auction count.",
                        Status = 400
                    }
                });
            }

            var auctionServiceApiResponse = new AuctionServiceApiResponse<AuctionCountDto>
            {
                Data = auctionCount
            };
            return Ok(auctionServiceApiResponse);
        }

        [HttpGet("auction/{auctionId:guid}")]//это когда гость заходит на страницу аукциона
        public async Task<ActionResult<AuctionServiceApiResponse<AuctionPageBaseDto>>> GetAuctionDetails(Guid auctionId)
        {

            var auctionDetails = await _auctionGetDetailsService.GetById(auctionId);
            if (auctionDetails == null || !auctionDetails.IsActive)
            {
                return StatusCode(400, new AuctionServiceApiResponse<AuctionPageBaseDto>
                {
                    Error = new ErrorDto
                    {
                        Error = "Auction not available",
                        Details = "The auction either does not exist or is not currently active.",
                        Status = 400
                    }
                });

            }
            var currentBid = await _bidService.GetMaxByAuctionId(auctionId);

            var auctionPageDto = new AuctionPageBaseDto
            {
                AuctionId = auctionDetails.AuctionId,
                Title = auctionDetails.GetTitle(),
                Description = auctionDetails.GetDescription(),
                CreationTime = auctionDetails.CreationTime,
                EndTime = auctionDetails.EndTime,
                Reserve = auctionDetails.Reserve,
                CurrentBid = currentBid?.GetValue() ?? 0
            };
            var auctionServiceApiResponse = new AuctionServiceApiResponse<AuctionPageBaseDto>
            {
                Data = auctionPageDto,
            };
            return Ok(auctionServiceApiResponse);
        }
        [HttpGet("auction/creator-winner/auction/{auctionId:guid}")]//для создателя и победителя посмотреть статус конткретного аукциона
        public async Task<ActionResult<AuctionServiceApiResponse<AuctionPageStatusDto>>> GetAuctionDetailsWithStatus(Guid auctionId)
        {

            var auctionDetails = await _auctionGetDetailsService.GetById(auctionId);

            if (auctionDetails == null)
            {
                return StatusCode(400, new AuctionServiceApiResponse<AuctionPageStatusDto>
                {
                    Error = new ErrorDto
                    {
                        Error = "Auction not available",
                        Details = "The auction either does not exist or is not currently active.",
                        Status = 400
                    }
                });
            }
            string status = "";
            var currentBid = await _bidService.GetMaxByAuctionId(auctionId);

            if (auctionDetails.IsActive)

            { status = "Active"; }
            else
            {
                status = await _auctionStatusService.GetAuctionStatus(auctionId);
            }

            var auctionPageStatusDto = new AuctionPageStatusDto
            {
                AuctionId = auctionDetails.AuctionId,
                Title = auctionDetails.GetTitle(),
                Description = auctionDetails.GetDescription(),
                CreationTime = auctionDetails.CreationTime,
                EndTime = auctionDetails.EndTime,
                Reserve = auctionDetails.Reserve,
                CurrentBid = currentBid?.GetValue() ?? 0,
                Status = status
            };
            var auctionServiceApiResponse = new AuctionServiceApiResponse<AuctionPageStatusDto>
            {
                Data = auctionPageStatusDto,
            };
            return Ok(auctionServiceApiResponse);
        }

        [HttpGet("auction/bidder/{userId:guid}/auction/{auctionId:guid}")]//для bidder посмотреть страница аукциона тоесть если user биддер то это
        public async Task<ActionResult<AuctionServiceApiResponse<AuctionPageBidderDto>>> GetAuctionDetailsForBidder(Guid auctionId, Guid userId)
        {
            var auctionDetails = await _auctionGetDetailsService.GetById(auctionId);
            if (auctionDetails == null || !auctionDetails.IsActive)
            {
                return StatusCode(400, new AuctionServiceApiResponse<AuctionPageBidderDto>
                {
                    Error = new ErrorDto
                    {
                        Error = "Auction not available",
                        Details = "The auction either does not exist or is not currently active.",
                        Status = 400
                    }
                });
            }
            var currentBid = await _bidService.GetMaxByAuctionId(auctionId);
            var biddersBid = await _bidService.GetUserBidForAuction(auctionId, userId);

            var auctionPageBidderDto = new AuctionPageBidderDto
            {
                AuctionId = auctionDetails.AuctionId,
                Title = auctionDetails.GetTitle(),
                Description = auctionDetails.GetDescription(),
                CreationTime = auctionDetails.CreationTime,
                EndTime = auctionDetails.EndTime,
                Reserve = auctionDetails.Reserve,
                CurrentBid = currentBid?.GetValue() ?? 0,
                BiddersBid = biddersBid?.GetValue() ?? 0
            };
            var auctionServiceApiResponse = new AuctionServiceApiResponse<AuctionPageBidderDto>
            {
                Data = auctionPageBidderDto,
            };
            return Ok(auctionServiceApiResponse);
        }
        [HttpGet("auctions/user/{userId:guid}")]
        public async Task<ActionResult<AuctionServiceApiResponse<List<AuctionListItemDto>>>> GetAuctionsForUser(
             Guid userId,
             [FromQuery] string category,
             [FromQuery] bool? active = null)
        {
            List<AuctionDetails> auctionsDetails;

            switch (category?.ToLower())
            {
                case "creator":
                    if (active == null)
                        auctionsDetails = await _auctionGetDetailsService.GetByCreatorId(userId, true);
                    else
                    {
                        auctionsDetails = await _auctionGetDetailsService.GetByCreatorId(userId, active.Value);
                    }
                    break;

                case "winner":
                    auctionsDetails = await _auctionGetDetailsService.GetByWinnerId(userId);

                    break;

                case "bidder":
                    auctionsDetails = await _auctionGetDetailsService.GetByBiddersId(userId);
                    break;

                default:
                    return StatusCode(400, new AuctionServiceApiResponse<object>
                    {
                        Error = new ErrorDto
                        {
                            Error = "Category failed",
                            Details = "Invalid or missing category",
                            Status = 400
                        }
                    });
            }

            if (auctionsDetails == null || !auctionsDetails.Any())
            {
                return Ok(new AuctionServiceApiResponse<List<AuctionListItemDto>>
                {
                    Data = new List<AuctionListItemDto>()
                });
            }

            var auctionIds = auctionsDetails.Select(ad => ad.AuctionId).ToList();
            var currentBids = await _bidService.GetMaxByAuctionIds(auctionIds);

            var auctionListDto = auctionsDetails.Select(detail =>
            {
                var bid = currentBids.FirstOrDefault(b => b.AuctionId == detail.AuctionId);
                return new AuctionListItemDto
                {
                    AuctionId = detail.AuctionId,
                    CreationTime = detail.CreationTime,
                    EndTime = detail.EndTime,
                    Title = detail.GetTitle(),
                    Reserve = detail.Reserve,
                    CurrentBid = bid?.GetValue() ?? 0
                };
            }).ToList();

            return Ok(new AuctionServiceApiResponse<List<AuctionListItemDto>>
            {
                Data = auctionListDto
            });
        }

        [HttpGet("get-category/auction/{auctionId:guid}/user/{userId:guid}")]
        public async Task<ActionResult<AuctionServiceApiResponse<UserCategoryForAuctionDto>>> GetUserCategoryForAuction(Guid auctionId, Guid userId)
        {
            string category = await _userCategoryService.GetUserCategoryForAuction(userId, auctionId);
            var userCategoryForAuctionDto = new UserCategoryForAuctionDto { Category = category };
            return Ok(new AuctionServiceApiResponse<UserCategoryForAuctionDto> { Data = userCategoryForAuctionDto });
        }


        [HttpPost("auction/createBy/user/{userId:guid}")]
        public async Task<ActionResult> CreateAuction(Guid userId, [FromBody] AuctionCreateDto auctionCreateDto)
        {
            var validator = new AuctionCreateDtoValidator();
            var validationResult = await validator.ValidateAsync(auctionCreateDto);
            if (!validationResult.IsValid)
            {
                return StatusCode(400, new AuctionServiceApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });

            }
            await _auctionCreateUpdateService.Create(userId, auctionCreateDto.AuctionDurationInDays, auctionCreateDto.Title, auctionCreateDto.Description, auctionCreateDto.Reserve);

            return Ok();
        }
        [HttpPost("auction/{auctionId:guid}/create/bid/user/{userId:guid}")]
        public async Task<ActionResult<AuctionServiceApiResponse<object>>> CreateBid(Guid userId, Guid auctionId, [FromBody] BidCreateDto bidCreateDto)
        {
            var validator = new BidCreateDtoValidator();
            var validationResult = await validator.ValidateAsync(bidCreateDto);
            if (!validationResult.IsValid)
            {
                return StatusCode(400, new AuctionServiceApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });

            }
            var bidCreateResult = await _bidService.Create(auctionId, userId, bidCreateDto.BidValue);
            if (bidCreateResult.IsSuccess)
            {
                return Ok(new AuctionServiceApiResponse<object>());
            }
            else return StatusCode(400, new AuctionServiceApiResponse<object>
            {
                Error = new ErrorDto
                {
                    Error = "Bid failed",
                    Details = bidCreateResult.Error,
                    Status = 400
                }
            });
        }
        [HttpDelete("auction/{auctionId:guid}/delete/bid/user/{userId:guid}")]
        public async Task<ActionResult<AuctionServiceApiResponse<object>>> DeleteAuctionBid(Guid userId, Guid auctionId)
        {
            await _bidService.Delete(auctionId, userId);
            return Ok(new AuctionServiceApiResponse<object>());

        }
     



        [HttpPut("auction/update/{auctionId:guid}/user/{userId:guid}")]
        public async Task<ActionResult<AuctionServiceApiResponse<object>>> UpdateAuction(Guid auctionId, Guid userId, [FromBody] AuctionUpdateDto auctionUpdateDto)
        {
            var validator = new AuctionUpdateDtoValidator();
            var validationResult = await validator.ValidateAsync(auctionUpdateDto);
            if (!validationResult.IsValid)
            {
                return StatusCode(400, new AuctionServiceApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Validation failed",
                        Details = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        Status = 400
                    }
                });

            }
            var auctionUpdateResult = await _auctionCreateUpdateService.Update(auctionId, userId, auctionUpdateDto.Title, auctionUpdateDto.Description);
            if (auctionUpdateResult.IsSuccess)
            {
                return Ok(new AuctionServiceApiResponse<object>());

            }
            else
            {
                return StatusCode(400, new AuctionServiceApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Update failed",
                        Details = auctionUpdateResult.Error,
                        Status = 400
                    }
                });
            }
        }

        [HttpPut("auction/close/{auctionId:guid}")]
        public async Task<ActionResult<AuctionServiceApiResponse<object>>> CloseAuctionByUserType(Guid auctionId, [FromQuery] string userType)
        {
            if (string.IsNullOrWhiteSpace(userType))
            {
                return StatusCode(400, new AuctionServiceApiResponse<object>
                {
                    Error = new ErrorDto
                    {
                        Error = "Argument failed",
                        Details = "UserType is required.",
                        Status = 400
                    }
                });
            }
            await _auctionStatusService.CloseAuction(auctionId, userType);
            return Ok(new AuctionServiceApiResponse<object>());
        }

        [HttpPut("auction/deal-complete/{auctionId:guid}/user/{userId:guid}")]
        public async Task<ActionResult<AuctionServiceApiResponse<object>>> AuctionDealComplete(Guid auctionId, Guid userId)
        {
            await _auctionStatusService.AuctionDealComplete(auctionId, userId);
            return Ok(new AuctionServiceApiResponse<object>());
        }
        [HttpGet("isActive/auction/{auctionId:guid}")]
        public async Task<ActionResult<bool>> IsAuctionActive(Guid auctionId)
        {
            var auctionStatus = await _auctionStatusService.GetAuctionStatus(auctionId);
            if (auctionStatus == "Active")
            {
                return Ok(true);
            }
            else { return Ok(false); }
        }
        [HttpGet("auction/{auctionId:guid}/user/{userId:guid}/creator-winner-info")]
        public async Task<ActionResult<AuctionServiceApiResponse<ContactDto>>> GetCreatorIdForWinner(Guid auctionId,Guid userId)
        {
           var creatorOrCreatorId = await _auctionStatusService.GetWinnerOrCreatorId(auctionId, userId);
            if (creatorOrCreatorId.ContactId == Guid.Empty)
            {
                return StatusCode(400, new AuctionServiceApiResponse<ContactDto>
                {
                    Error = new ErrorDto
                    {
                        Error = "Id failed",
                        Details = "Id is required.",
                        Status = 400
                    }
                });
            }
            return Ok(new AuctionServiceApiResponse<ContactDto> { Data = creatorOrCreatorId });
            
        }
        




    }
}
