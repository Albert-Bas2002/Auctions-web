using System;

namespace Auction.UserAuthService.Core.Models
{
    public class User
    {
        public const int USER_NAME_MAX_LENGTH = 50;
        public const int EMAIL_MAX_LENGTH = 100;
        public const int CONTACTS_MAX_LENGTH = 100;

        public const int PASSWORD_MAX_LENGTH = 100;
        public const int PASSWORD_MiN_LENGTH = 6;


        public Guid UserId { get; }
        public string UserName { get; private set; }
        public string PasswordHash { get; private set; }
        public string Email { get; private set; }
        public string Contacts { get; private set; }


        private User(Guid userId, string userName, string passwordHash, string email, string contacts)
        {
            Validate(userId, userName, passwordHash, email,contacts);

            UserId = userId;
            UserName = userName;
            PasswordHash = passwordHash;
            Email = email;
            Contacts = contacts;
        }

        public static User Create(Guid userId, string userName, string passwordHash, string email, string contacts)
        {
            return new User(userId, userName, passwordHash, email,contacts);
        }

        public void ChangeUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name cannot be null or empty.", nameof(userName));

            if (userName.Length > USER_NAME_MAX_LENGTH)
                throw new ArgumentException($"User name cannot be longer than {USER_NAME_MAX_LENGTH} characters.", nameof(userName));

            UserName = userName;
        }
        public void ChangeContacts(string contacts)
        {
            if (string.IsNullOrWhiteSpace(contacts))
                throw new ArgumentException("Contracts cannot be null or empty.", nameof(contacts));

            if (contacts.Length > CONTACTS_MAX_LENGTH)
                throw new ArgumentException($"Contracts cannot be longer than {CONTACTS_MAX_LENGTH} characters.", nameof(contacts));

            Contacts = contacts;
        }

        public void ChangeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            if (email.Length > EMAIL_MAX_LENGTH)
                throw new ArgumentException($"Email cannot exceed {EMAIL_MAX_LENGTH} characters.", nameof(email));

            if (!email.Contains('@'))
                throw new ArgumentException("Email must contain '@' symbol.", nameof(email));
            Email = email;
        }
        public bool ChangePassword(string newPasswordHash)
        {
            if (newPasswordHash != PasswordHash)
            {
                PasswordHash = newPasswordHash;
                return true;
            }
            else return false;

        }

        private static void Validate(Guid userId, string userName, string passwordHash, string email, string contacts)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            if (string.IsNullOrWhiteSpace(userName))
                throw new ArgumentException("User name cannot be null or empty.", nameof(userName));

            if (userName.Length > USER_NAME_MAX_LENGTH)
                throw new ArgumentException($"User name cannot exceed {USER_NAME_MAX_LENGTH} characters.", nameof(userName));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty.", nameof(passwordHash));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            if (contacts.Length > CONTACTS_MAX_LENGTH)
                throw new ArgumentException($"Contracts cannot exceed {CONTACTS_MAX_LENGTH} characters.", nameof(contacts));

            if (email.Length > EMAIL_MAX_LENGTH)
                throw new ArgumentException($"Email cannot exceed {EMAIL_MAX_LENGTH} characters.", nameof(email));

            if (!email.Contains('@'))
                throw new ArgumentException("Email must contain '@' symbol.", nameof(email));
        }
    }
}
