using Auction.UserAuthService.Contracts.Dtos;
using Auction.UserAuthService.Core.Abstractions;
using Auction.UserAuthService.Core.Models;
using CSharpFunctionalExtensions;
namespace Auction.UserAuthService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtProvider _jwtProvider;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtProvider jwtProvider)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtProvider = jwtProvider;
        }

        public async Task<Result> Register(string userName, string email, string password,string contacts)
        {
            if (await _userRepository.UserExistsByEmail(email))
            {
                var error = new ErrorDto
                {
                    Error = "A user with this email already exists.",
                    Details = $"The email '{email}' is already taken by another account.",
                   Status = 409
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure(errorMessage);
            }

            var hashedPassword = _passwordHasher.Generate(password);
            var user = User.Create(Guid.NewGuid(), userName, hashedPassword, email, contacts);
            await _userRepository.Add(user);

            return Result.Success("User registered successfully.");
        }

        public async Task<Result<LoginTokenDto>> Login(string email, string password)
        {
            var user = await _userRepository.GetByEmail(email);

            if (user == null || !_passwordHasher.Verify(password, user.PasswordHash))
            {
                var error = new ErrorDto
                {
                    Error = "Invalid login credentials.",
                    Details = "Email or password is incorrect.",
                    Status = 401
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure<LoginTokenDto>(errorMessage);
            }

            var userPermissions = await _userRepository.GetUsersPermissions(user.UserId);
            var token = _jwtProvider.GenerateToken(user, userPermissions);

            return Result.Success(new LoginTokenDto { Token = token });
        }

        public async Task<Result> ChangePassword(Guid userId, string newPassword, string previousPassword)
        {
            var user = await _userRepository.GetByUserId(userId);

            if (user == null)
            {
                var error = new ErrorDto
                {
                    Error = "User not found.",
                    Details = "The specified user ID does not exist in the system.",
                    Status = 404
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure(errorMessage);
            }

            if (!_passwordHasher.Verify(previousPassword, user.PasswordHash))
            {
                var error = new ErrorDto
                {
                    Error = "Incorrect current password.",
                    Details = "The provided password does not match the current password.",
                    Status = 400
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure(errorMessage);
            }

            var newPasswordHash = _passwordHasher.Generate(newPassword);
            var previousPasswordHash = _passwordHasher.Generate(previousPassword);

            if (!user.ChangePassword(newPasswordHash))
            {
                var error = new ErrorDto
                {
                    Error = "Password change failed.",
                    Details = "Password hash mismatch or invalid password state.",
                    Status = 400
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure(errorMessage);
            }

            await _userRepository.Update(user);
            return Result.Success();
        }

        public async Task<Result> ChangeUserName(Guid userId, string newUserName)
        {
            var user = await _userRepository.GetByUserId(userId);

            if (user == null)
            {
                var error = new ErrorDto
                {
                    Error = "User not found.",
                    Details = "The specified user ID does not exist in the system.",
                    Status = 404
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure(errorMessage);
            }

            user.ChangeUserName(newUserName);
            await _userRepository.Update(user);
            return Result.Success();
        }
        public async Task<Result> ChangeContacts(Guid userId, string newContacts)
        {
            var user = await _userRepository.GetByUserId(userId);

            if (user == null)
            {
                var error = new ErrorDto
                {
                    Error = "User not found.",
                    Details = "The specified user ID does not exist in the system.",
                    Status = 404
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure(errorMessage);
            }

            user.ChangeContacts(newContacts);
            await _userRepository.Update(user);
            return Result.Success();
        }
        public async Task<Result> ChangeEmail(Guid userId, string newEmail)
        {
            var user = await _userRepository.GetByUserId(userId);

            if (user == null)
            {
                var error = new ErrorDto
                {
                    Error = "User not found.",
                    Details = "The specified user ID does not exist in the system.",
                    Status = 404
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure(errorMessage);
            }

            user.ChangeEmail(newEmail);
            await _userRepository.Update(user);
            return Result.Success();
        }

        public async Task<bool> IsUserByUserId(Guid userId)
        {
            var user = await _userRepository.GetByUserId(userId);
            return user != null;
        }
        public async Task<Result<UserInfoDto>> GetUserInfo(Guid userId)
        {
            var user = await _userRepository.GetByUserId(userId);
            if (user == null)
            {
                var error = new ErrorDto
                {
                    Error = "User not found.",
                    Details = "The specified user ID does not exist in the system.",
                    Status = 404
                };
                string errorMessage = $"Error: {error.Error}, Details: {error.Details}, Status: {error.Status}";
                return Result.Failure<UserInfoDto>(errorMessage);
            }
            var userInfoDto = new UserInfoDto
            {
                Email = user.Email,
                UserName = user.UserName,
                Contacts = user.Contacts,
            };
                return Result.Success(userInfoDto);
        }
    }
}