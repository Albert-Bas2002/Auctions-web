using Auction.AuctionService.Contracts;
using Auction.AuctionService.Contracts.Dtos;
using Auction.AuctionService.Core.Abstractions;
using Auction.AuctionService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Auction.AuctionService.Core.Models;
using CSharpFunctionalExtensions;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionCreateUpdateService> _mockCreateUpdateService = new();
    private readonly Mock<IAuctionStatusService> _mockStatusService = new();
    private readonly Mock<IUserCategoryService> _mockUserCategoryService = new();
    private readonly Mock<IAuctionGetDetailsService> _mockGetDetailsService = new();
    private readonly Mock<IBidService> _mockBidService = new();

    private AuctionController CreateController() =>
        new AuctionController(_mockCreateUpdateService.Object,
                              _mockStatusService.Object,
                              _mockUserCategoryService.Object,
                              _mockGetDetailsService.Object,
                              _mockBidService.Object);

    [Fact]
    public async Task GetActiveAuctionsInfo_ReturnsOkWithData_WhenAuctionsExist()
    {
     
        var controller = CreateController();

        var auctionDetailsList = new List<AuctionDetails>
    {
        AuctionDetails.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            "Test Auction",
            "Description for test auction",
            100
        )
    };

        _mockGetDetailsService.Setup(s => s.GetAll(It.IsAny<string>(), true, It.IsAny<int>()))
            .ReturnsAsync(auctionDetailsList);

        _mockBidService.Setup(s => s.GetMaxByAuctionIds(It.IsAny<List<Guid>>()))
            .ReturnsAsync(new List<Bid>());

        var result = await controller.GetActiveAuctionsInfo("sort", 1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<List<AuctionListItemDto>>>(okResult.Value);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data);
    }


    [Fact]
    public async Task GetActiveAuctionsInfo_ReturnsEmptyList_WhenNoAuctions()
    {
       
        var controller = CreateController();
        _mockGetDetailsService.Setup(s => s.GetAll(It.IsAny<string>(), true, It.IsAny<int>()))
            .ReturnsAsync((List<AuctionDetails>)null);

       
        var result = await controller.GetActiveAuctionsInfo("sort", 1);

       
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<List<AuctionListItemDto>>>(okResult.Value);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
    }

    [Fact]
    public async Task GetAuctionCount_ReturnsOkWithData_WhenCountIsReturned()
    {
       
        var controller = CreateController();
        var countDto = new AuctionCountDto { Count = 5 };
        _mockGetDetailsService.Setup(s => s.GetAuctionCount(It.IsAny<bool>()))
            .ReturnsAsync(countDto);

        
        var result = await controller.GetAuctionCount(true);

      
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<AuctionCountDto>>(okResult.Value);
        Assert.Equal(5, response.Data.Count);
    }

    [Fact]
    public async Task GetAuctionCount_ReturnsBadRequest_WhenCountIsNull()
    {
        
        var controller = CreateController();
        _mockGetDetailsService.Setup(s => s.GetAuctionCount(It.IsAny<bool>()))
            .ReturnsAsync((AuctionCountDto)null);

       
        var result = await controller.GetAuctionCount(true);

        
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Assert.IsType<AuctionServiceApiResponse<AuctionCountDto>>(badRequestResult.Value);
        Assert.NotNull(response.Error);
    }

    [Fact]
    public async Task GetAuctionDetails_ReturnsBadRequest_WhenAuctionNotActive()
    {
        
        var controller = CreateController();
        var auctionId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var creationTime = DateTime.UtcNow.AddDays(-5);
        var endTime = creationTime.AddDays(3);

        var auctionDetails = AuctionDetails.Create(auctionId, creatorId, creationTime, endTime, "Title", "Description", 100);
        auctionDetails.SetFalseActive();

        _mockGetDetailsService.Setup(s => s.GetById(auctionId))
            .ReturnsAsync(auctionDetails);

        
        var result = await controller.GetAuctionDetails(auctionId);

        
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Assert.IsType<AuctionServiceApiResponse<AuctionPageBaseDto>>(badRequestResult.Value);
        Assert.NotNull(response.Error);
    }


    [Fact]
    public async Task CreateBid_ReturnsBadRequest_WhenValidationFails()
    {
        var controller = CreateController();
        var userId = Guid.NewGuid();
        var auctionId = Guid.NewGuid();
        var bidDto = new BidCreateDto { BidValue = -1 };

        var result = await controller.CreateBid(userId, auctionId, bidDto);


        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);

        var response = Assert.IsType<AuctionServiceApiResponse<object>>(badRequestResult.Value);
        Assert.NotNull(response.Error);
    }

    [Fact]
    public async Task CreateBid_ReturnsOk_WhenBidCreatedSuccessfully()
    {
        
        var controller = CreateController();
        var userId = Guid.NewGuid();
        var auctionId = Guid.NewGuid();
        var bidDto = new BidCreateDto { BidValue = 100 };

        _mockBidService.Setup(s => s.Create(auctionId, userId, bidDto.BidValue))
            .ReturnsAsync(Result.Success());

        var result = await controller.CreateBid(userId, auctionId, bidDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<object>>(okResult.Value);
        Assert.Null(response.Error);
    }


    [Fact]
    public async Task GetAuctionDetailsWithStatus_ReturnsBadRequest_WhenAuctionNotFound()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();

        _mockGetDetailsService.Setup(s => s.GetById(auctionId))
            .ReturnsAsync((AuctionDetails)null);

        var result = await controller.GetAuctionDetailsWithStatus(auctionId);

        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Assert.IsType<AuctionServiceApiResponse<AuctionPageStatusDto>>(badRequestResult.Value);
        Assert.NotNull(response.Error);
    }

    [Fact]
    public async Task GetAuctionDetailsWithStatus_ReturnsOk_WithActiveStatus()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();
        var auctionCreatorId = Guid.NewGuid();
        var creationTime = DateTime.UtcNow;
        var endTime = creationTime.AddDays(5);
        var title = "Test Auction Title";
        var description = "Description of the auction.";
        int reserve = 100;

        var auctionDetails = AuctionDetails.Create(
            auctionId,
            auctionCreatorId,
            creationTime,
            endTime,
            title,
            description,
            reserve
        );

        _mockGetDetailsService.Setup(s => s.GetById(auctionId)).ReturnsAsync(auctionDetails);
        _mockBidService.Setup(s => s.GetMaxByAuctionId(auctionId)).ReturnsAsync((Bid)null);

        var result = await controller.GetAuctionDetailsWithStatus(auctionId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<AuctionPageStatusDto>>(okResult.Value);
        Assert.Equal("Active", response.Data.Status);
        Assert.Equal(auctionId, response.Data.AuctionId);
    }

    [Fact]
    public async Task GetAuctionDetailsForBidder_ReturnsBadRequest_WhenAuctionInactive()
    {
        var controller = CreateController();
        var userId = Guid.NewGuid();
        var auctionId = Guid.NewGuid();
        var auctionDetails = AuctionDetails.Create(
            auctionId,
            Guid.NewGuid(), 
            DateTime.UtcNow.AddDays(-10), 
            DateTime.UtcNow.AddDays(1),  
            "Test Title",
            "Test Description",
            100 
            );

        auctionDetails.Close();

        _mockGetDetailsService.Setup(s => s.GetById(auctionId)).ReturnsAsync(auctionDetails);


        var result = await controller.GetAuctionDetailsForBidder(auctionId, userId);

        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Assert.IsType<AuctionServiceApiResponse<AuctionPageBidderDto>>(badRequestResult.Value);
        Assert.NotNull(response.Error);
    }

    [Fact]
    public async Task GetAuctionDetailsForBidder_ReturnsOk_WithData()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();
        var auctionCreatorId = Guid.NewGuid();
        var creationTime = DateTime.UtcNow;
        var endTime = creationTime.AddDays(5);
        var title = "Test Auction Title";
        var description = "Description of the auction.";
        int reserve = 100;

        var userId = Guid.NewGuid();  

        var auctionDetails = AuctionDetails.Create(
            auctionId,
            auctionCreatorId,
            creationTime,
            endTime,
            title,
            description,
            reserve
        );

        _mockGetDetailsService.Setup(s => s.GetById(auctionId)).ReturnsAsync(auctionDetails);

        var maxBid = Bid.Create(Guid.NewGuid(), Guid.NewGuid(), auctionId, 200, DateTime.UtcNow.AddMinutes(-10));
        _mockBidService.Setup(s => s.GetMaxByAuctionId(auctionId)).ReturnsAsync(maxBid);

        var userBid = Bid.Create(Guid.NewGuid(), userId, auctionId, 150, DateTime.UtcNow.AddMinutes(-5));
        _mockBidService.Setup(s => s.GetUserBidForAuction(auctionId, userId)).ReturnsAsync(userBid);

        var result = await controller.GetAuctionDetailsForBidder(auctionId, userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<AuctionPageBidderDto>>(okResult.Value);

        Assert.Equal(auctionId, response.Data.AuctionId);
        Assert.Equal(200, response.Data.CurrentBid);
        Assert.Equal(150, response.Data.BiddersBid);
    }


    [Fact]
    public async Task GetAuctionsForUser_ReturnsBadRequest_WhenCategoryInvalid()
    {
        var controller = CreateController();
        var userId = Guid.NewGuid();

        var result = await controller.GetAuctionsForUser(userId, "invalid_category", null);

        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Assert.IsType<AuctionServiceApiResponse<object>>(badRequestResult.Value);
        Assert.NotNull(response.Error);
    }

    [Fact]
    public async Task GetAuctionsForUser_ReturnsOk_WithEmptyList_WhenNoAuctions()
    {
        var controller = CreateController();
        var userId = Guid.NewGuid();

        _mockGetDetailsService.Setup(s => s.GetByCreatorId(userId, true))
            .ReturnsAsync(new List<AuctionDetails>());

        var result = await controller.GetAuctionsForUser(userId, "creator", true);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<List<AuctionListItemDto>>>(okResult.Value);
        Assert.Empty(response.Data);
    }

    [Fact]
    public async Task GetUserCategoryForAuction_ReturnsOk_WithCategory()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockUserCategoryService.Setup(s => s.GetUserCategoryForAuction(userId, auctionId))
            .ReturnsAsync("Bidder");

        var result = await controller.GetUserCategoryForAuction(auctionId, userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<UserCategoryForAuctionDto>>(okResult.Value);

        Assert.Equal("Bidder", response.Data.Category);
    }

    [Fact]
    public async Task DeleteAuctionBid_ReturnsOk()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockBidService.Setup(s => s.Delete(auctionId, userId)).Returns(Task.CompletedTask);

        var result = await controller.DeleteAuctionBid(userId, auctionId);

        var actionResult = Assert.IsType<ActionResult<AuctionServiceApiResponse<object>>>(result);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        var response = Assert.IsType<AuctionServiceApiResponse<object>>(okResult.Value);
    }


    [Fact]
    public async Task CloseAuctionByUserType_ReturnsBadRequest_WhenUserTypeEmpty()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();

        var result = await controller.CloseAuctionByUserType(auctionId, "");

        var actionResult = Assert.IsType<ActionResult<AuctionServiceApiResponse<object>>>(result);

        var badRequestResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(400, badRequestResult.StatusCode);

        var response = Assert.IsType<AuctionServiceApiResponse<object>>(badRequestResult.Value);
        Assert.NotNull(response.Error);
    }


    [Fact]
    public async Task CloseAuctionByUserType_ReturnsOk_WhenUserTypeValid()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();

        _mockStatusService.Setup(s => s.CloseAuction(auctionId, "creator")).Returns(Task.CompletedTask);

        var result = await controller.CloseAuctionByUserType(auctionId, "creator");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<object>>(okResult.Value);
    }

    [Fact]
    public async Task AuctionDealComplete_ReturnsOk()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockStatusService.Setup(s => s.AuctionDealComplete(auctionId, userId)).Returns(Task.CompletedTask);

        var result = await controller.AuctionDealComplete(auctionId, userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<object>>(okResult.Value);
    }


    [Fact]
    public async Task IsAuctionActive_ReturnsTrue_WhenStatusIsActive()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();

        _mockStatusService.Setup(s => s.GetAuctionStatus(auctionId)).ReturnsAsync("Active");

        var result = await controller.IsAuctionActive(auctionId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.True((bool)okResult.Value);
    }

    [Fact]
    public async Task IsAuctionActive_ReturnsFalse_WhenStatusIsNotActive()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();

        _mockStatusService.Setup(s => s.GetAuctionStatus(auctionId)).ReturnsAsync("Closed");

        var result = await controller.IsAuctionActive(auctionId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.False((bool)okResult.Value);
    }

    [Fact]
    public async Task GetCreatorIdForWinner_ReturnsBadRequest_WhenContactIdEmpty()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _mockStatusService.Setup(s => s.GetWinnerOrCreatorId(auctionId, userId))
            .ReturnsAsync(new ContactDto { ContactId = Guid.Empty });

        var result = await controller.GetCreatorIdForWinner(auctionId, userId);

        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
        var response = Assert.IsType<AuctionServiceApiResponse<ContactDto>>(badRequestResult.Value);
        Assert.NotNull(response.Error);
    }

    [Fact]
    public async Task GetCreatorIdForWinner_ReturnsOk_WhenContactIdValid()
    {
        var controller = CreateController();
        var auctionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        _mockStatusService.Setup(s => s.GetWinnerOrCreatorId(auctionId, userId))
            .ReturnsAsync(new ContactDto { ContactId = contactId });

        var result = await controller.GetCreatorIdForWinner(auctionId, userId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AuctionServiceApiResponse<ContactDto>>(okResult.Value);

        Assert.Equal(contactId, response.Data.ContactId);
    }

}
