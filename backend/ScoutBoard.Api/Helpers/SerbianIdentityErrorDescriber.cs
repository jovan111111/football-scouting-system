using Microsoft.AspNetCore.Identity;

namespace ScoutBoard.Api.Helpers;

public class SerbianIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() =>
        Create(nameof(DefaultError), "Došlo je do greške. Pokušajte ponovo.");

    public override IdentityError InvalidEmail(string? email) =>
        Create(nameof(InvalidEmail), $"E-mail adresa „{email}“ nije ispravna.");

    public override IdentityError DuplicateEmail(string email) =>
        Create(nameof(DuplicateEmail), "Korisnik sa ovom e-mail adresom već postoji.");

    public override IdentityError DuplicateUserName(string userName) =>
        Create(nameof(DuplicateUserName), "Korisnik sa ovom e-mail adresom već postoji.");

    public override IdentityError PasswordTooShort(int length) =>
        Create(nameof(PasswordTooShort), $"Lozinka mora imati najmanje {length} karaktera.");

    public override IdentityError PasswordRequiresDigit() =>
        Create(nameof(PasswordRequiresDigit), "Lozinka mora sadržati najmanje jedan broj.");

    public override IdentityError PasswordRequiresLower() =>
        Create(nameof(PasswordRequiresLower), "Lozinka mora sadržati najmanje jedno malo slovo.");

    public override IdentityError PasswordRequiresUpper() =>
        Create(nameof(PasswordRequiresUpper), "Lozinka mora sadržati najmanje jedno veliko slovo.");

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        Create(nameof(PasswordRequiresNonAlphanumeric), "Lozinka mora sadržati specijalni karakter.");

    private static IdentityError Create(string code, string description) =>
        new() { Code = code, Description = description };
}
