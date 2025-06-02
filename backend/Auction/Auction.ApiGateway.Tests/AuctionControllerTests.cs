using Auction.ApiGateway.Controllers;
using Auction.ApiGateway.Contracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts.Dtos;
using Auction.ApiGateway.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Auction.ApiGateway.Contracts.ApiGatewayContracts;
using Auction.ApiGateway.Contracts.AuctionServiceContracts;
using CSharpFunctionalExtensions;

public class AuctionControllerTests
{
    private readonly Mock<IAuctionService> _auctionServiceMock;
    private readonly Mock<IErrorMessageParser> _errorMessageParserMock;
    private readonly AuctionController _controller;

    public AuctionControllerTests()
    {
        _auctionServiceMock = new Mock<IAuctionService>();
        _errorMessageParserMock = new Mock<IErrorMessageParser>();
        _controller = new AuctionController(_auctionServiceMock.Object, _errorMessageParserMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("userId", Guid.NewGuid().ToString())
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task GetAuctionPage_ReturnsOk_WhenSuccess()
    {

        var auctionId = Guid.NewGuid();
        var auctionPage = new AuctionPageDto { AuctionId = auctionId };
        _auctionServiceMock.Setup(x => x.GetAuctionPage(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result.Success(auctionPage));


        var result = await _controller.GetAuctionPage(auctionId);


        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<AuctionPageDto>>(okResult.Value);
        Assert.Equal(auctionId, response.Data.AuctionId);
    }

    [Fact]
    public async Task CreateAuction_ReturnsOk_WhenSuccess()
    {
        
        var request = new AuctionCreateRequest
        {
            Title = "Auction",
            Description = "Test",
            AuctionDurationInDays = 3,
            Reserve = 100
        };

        _auctionServiceMock.Setup(x => x.CreateAuction(It.IsAny<Guid>(), It.IsAny<AuctionCreateDto>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.CreateAuction(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<object>>(okResult.Value);
        Assert.Null(response.Error);
    }

    [Fact]
    public async Task CloseAuctionByModerator_ReturnsOk_WhenSuccess()
    {
        var auctionId = Guid.NewGuid();
        _auctionServiceMock.Setup(x => x.CloseAuction(auctionId, "Moderator", null))
            .ReturnsAsync(Result.Success());

        var result = await _controller.CloseAuctionByModerator(auctionId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<object>>(okResult.Value);
        Assert.Null(response.Error);
    }

    [Fact]
    public async Task CloseAuctionByCreator_ReturnsOk_WhenSuccess()
    {
        var auctionId = Guid.NewGuid();
        _auctionServiceMock.Setup(x => x.CloseAuction(auctionId, "Creator", It.IsAny<Guid>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.CloseAuctionByCreator(auctionId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult);
    }

    [Fact]
    public async Task AuctionDealComplete_ReturnsOk_WhenSuccess()
    {
        var auctionId = Guid.NewGuid();
        _auctionServiceMock.Setup(x => x.AuctionCompleteDeal(auctionId, It.IsAny<Guid>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.AuctionDealComplete(auctionId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult);
    }

    [Fact]
    public async Task GetCreatorOrWinnerId_ReturnsOk_WhenSuccess()
    {
        var auctionId = Guid.NewGuid();
        var contact = new ContactDto { ContactId = Guid.NewGuid() };

        _auctionServiceMock.Setup(x => x.GetCreatorOrWinnerId(auctionId, It.IsAny<Guid>()))
            .ReturnsAsync(Result.Success(contact));

        var result = await _controller.GetCreatorOrWinnerId(auctionId);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<ContactDto>>(okResult.Value);
        Assert.Equal(contact.ContactId, response.Data.ContactId);
    }

    [Fact]
    public async Task GetAuctionForUser_ReturnsAuctions_WhenSuccess()
    {
        var auctions = new List<AuctionListItemDto>
        {
            new AuctionListItemDto { AuctionId = Guid.NewGuid() }
        };

        _auctionServiceMock.Setup(x => x.GetAuctionsForUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool?>()))
            .ReturnsAsync(Result.Success(auctions));

        var result = await _controller.GetAuctionForUser("Cars", true);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<List<AuctionListItemDto>>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task GetAuction_ReturnsAuctionList_WhenSuccess()
    {
        var auctions = new List<AuctionListItemDto>
        {
            new AuctionListItemDto { AuctionId = Guid.NewGuid() }
        };

        _auctionServiceMock.Setup(x => x.GetAuctions(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Success(auctions));

        var result = await _controller.GetAuction(null, 1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ApiGatewayResponse<List<AuctionListItemDto>>>(okResult.Value);
        Assert.Single(response.Data);
    }

    [Fact]
    public async Task UploadPhotos_ReturnsOk_WhenSuccess()
    {
        var auctionId = Guid.NewGuid();
        var photos = new List<IFormFile>(); 

        _auctionServiceMock.Setup(x => x.UploadPhotosForAuction(auctionId, photos, It.IsAny<Guid>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.UploadPhotos(auctionId, photos);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult);
    }

    [Fact]
    public async Task DeletePhotos_ReturnsOk_WhenSuccess()
    {
        var auctionId = Guid.NewGuid();
        var indexesToDelete = new[] { 0, 1 };

        _auctionServiceMock.Setup(x => x.DeletePhotosForAuction(auctionId, indexesToDelete, It.IsAny<Guid>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.DeletePhotos(auctionId, indexesToDelete);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult);
    }

    [Fact]
    public void GetPhotoByIndex_ReturnsNotFound_WhenFolderDoesNotExist()
    {
        var result = _controller.GetPhotoByIndex(Guid.NewGuid(), 0);

        Assert.IsType<NotFoundResult>(result);
    }
}
