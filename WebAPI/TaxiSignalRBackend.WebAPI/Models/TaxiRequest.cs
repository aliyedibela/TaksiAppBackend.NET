using System.ComponentModel.DataAnnotations;

namespace TaxiSignalRBackend.WebAPI.Models
{
    public class TaxiRequest
    {
        [Key]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string TaxiStandId { get; set; }
        public double FromLat { get; set; }
        public double FromLng { get; set; }
        public double ToLat { get; set; }
        public double ToLng { get; set; }
        public double EstimatedFare { get; set; }
        public DateTime RequestTime { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; 
        public string? DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? DriverPlate { get; set; }
    }

    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? VerificationCode { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<UserCard> Cards { get; set; } = new();
    }

    public class UserCard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string CardCode { get; set; } = string.Empty;
        public string CardNickname { get; set; } = "Kartım";  
        public decimal Balance { get; set; } = 0;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
    }

    public class PaymentTransaction
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string CardId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string Description { get; set; } = "RFID Ödeme";

        public decimal OldBalance { get; set; }
        public decimal NewBalance { get; set; }

        public string? DeviceId { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public UserCard? Card { get; set; }
        public User? User { get; set; }
    }
    public class Driver
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string TaxiStandId { get; set; }
        public string TaxiStandName { get; set; } 
        public string DriverName { get; set; } 
        public string VehiclePlate { get; set; } 
        public string? ConnectionId { get; set; }
        public bool IsOnline { get; set; }
        public bool IsVerified { get; set; }
        public string? VerificationCode { get; set; }
    }
}