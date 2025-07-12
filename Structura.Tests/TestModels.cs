namespace Structura.Tests.TestModels
{
    /// <summary>
    /// Test model for personal information
    /// </summary>
    public class PersonalInfo
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public string Password { get; set; } = ""; // Sensitive property for exclusion testing
        public DateTime BirthDate { get; set; }
    }

    /// <summary>
    /// Test model for contact information
    /// </summary>
    public class ContactInfo
    {
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string Country { get; set; } = "";
    }

    /// <summary>
    /// Test model for user information
    /// </summary>
    public class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Test model for product information
    /// </summary>
    public class Product
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public int StockQuantity { get; set; }
    }

    /// <summary>
    /// Test model for simple scenarios
    /// </summary>
    public class SimpleModel
    {
        public string Value { get; set; } = "";
    }
}