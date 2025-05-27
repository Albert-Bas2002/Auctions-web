using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using Auction.ApiGateway.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts.Dto;
using Auction.ApiGateway.Contracts.UserAuthServiceContracts;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CSharpFunctionalExtensions;
namespace Auction.ApiGateway.Application.Services
{
    public class UserService : IUserService
    {

        private readonly HttpClient _httpClient;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("UserAuthService");
        }


        public async Task<Result> Register(RegisterUserDto registerUserDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api-users/register", registerUserDto);
            var jsonString = await response.Content.ReadAsStringAsync();
            var userAuthApiRegisterResponse = JsonConvert.DeserializeObject<UserAuthApiResponse<object>>(jsonString);

            var error = userAuthApiRegisterResponse?.Error;

            if (response.IsSuccessStatusCode && error == null)
                return Result.Success();

            var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";

            return Result.Failure(errorMessage);
        }

        public async Task<Result<LoginTokenDto>> Login(LoginUserDto loginUserDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api-users/login", loginUserDto);
            var jsonString = await response.Content.ReadAsStringAsync();
            var userAuthApiLoginResponse = JsonConvert.DeserializeObject<UserAuthApiResponse<LoginTokenDto>>(jsonString);

            var error = userAuthApiLoginResponse?.Error;

            if (response.IsSuccessStatusCode && error == null)
                if (userAuthApiLoginResponse != null)
                {
                    if (userAuthApiLoginResponse.Data != null)
                    {
                        return Result.Success(userAuthApiLoginResponse.Data);
                    }
                }
            var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";

            return Result.Failure<LoginTokenDto>(errorMessage);
        }
        public async Task<Result<UserInfoDto>> GetUserInfo(Guid userId)
        {
            var url = $"api-users/user/{userId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _httpClient.SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            var userAuthApiUserResponse = JsonConvert.DeserializeObject<UserAuthApiResponse<UserInfoDto>>(jsonString);
            var error = userAuthApiUserResponse?.Error;

            if (response.IsSuccessStatusCode && error == null)
                if (userAuthApiUserResponse != null)
                {
                    if (userAuthApiUserResponse.Data != null)
                    {
                        return Result.Success(userAuthApiUserResponse.Data);
                    }
                }
            var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";

            return Result.Failure<UserInfoDto>(errorMessage);
        }
        public async Task<Result> ChangeUserName(ChangeUserNameDto changeUserNameDto)
        {

            var response = await _httpClient.PutAsJsonAsync("api-users/change-username", changeUserNameDto);
            var jsonString = await response.Content.ReadAsStringAsync();
            var userAuthApiChangeUserNameResponse = JsonConvert.DeserializeObject<UserAuthApiResponse<object>>(jsonString);

            var error = userAuthApiChangeUserNameResponse?.Error;

            if (response.IsSuccessStatusCode && error == null)
                return Result.Success();

            var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";

            return Result.Failure(errorMessage);
        }
        public async Task<Result> ChangeContacts(ChangeContactsDto changeContactsDto)
        {

            var response = await _httpClient.PutAsJsonAsync("api-users/change-contacts", changeContactsDto);
            var jsonString = await response.Content.ReadAsStringAsync();
            var userAuthApiChangeContactsResponse = JsonConvert.DeserializeObject<UserAuthApiResponse<object>>(jsonString);

            var error = userAuthApiChangeContactsResponse?.Error;

            if (response.IsSuccessStatusCode && error == null)
                return Result.Success();

            var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";

            return Result.Failure(errorMessage);
        }
        public async Task<Result> ChangeEmail(ChangeEmailDto changeEmailDto)
        {

            var response = await _httpClient.PutAsJsonAsync("api-users/change-email", changeEmailDto);
            var jsonString = await response.Content.ReadAsStringAsync();
            var userAuthApiChangeEmailResponse = JsonConvert.DeserializeObject<UserAuthApiResponse<object>>(jsonString);

            var error = userAuthApiChangeEmailResponse?.Error;

            if (response.IsSuccessStatusCode && error == null)
                return Result.Success();

            var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";

            return Result.Failure(errorMessage);
        }
        public async Task<Result> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var response = await _httpClient.PutAsJsonAsync("api-users/change-password", changePasswordDto);
            var jsonString = await response.Content.ReadAsStringAsync();
            var userAuthApiChangePasswordResponse = JsonConvert.DeserializeObject<UserAuthApiResponse<object>>(jsonString);

            var error = userAuthApiChangePasswordResponse?.Error;

            if (response.IsSuccessStatusCode && error == null)
                return Result.Success();

            var errorMessage = error != null
                ? $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}"
                : "Error: Server Error, Details: Internal Server Error. Please try again later., Status: 500";

            return Result.Failure(errorMessage);
        }
       

    }
}
