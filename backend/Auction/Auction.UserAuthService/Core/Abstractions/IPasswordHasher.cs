namespace Auction.UserAuthService.Core.Abstractions
{
    public interface IPasswordHasher
    {
        string Generate(string password);
        bool Verify(string password,string hashedPassword);   
    }
}