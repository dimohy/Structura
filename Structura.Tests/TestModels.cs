namespace Structura.Tests.TestModels
{
    /// <summary>
    /// 테스트용 개인 정보 모델
    /// </summary>
    public class PersonalInfo
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int Age { get; set; }
        public string Password { get; set; } = ""; // 제외 테스트용
        public DateTime BirthDate { get; set; }
    }

    /// <summary>
    /// 테스트용 연락처 정보 모델
    /// </summary>
    public class ContactInfo
    {
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string Country { get; set; } = "";
    }

    /// <summary>
    /// 테스트용 사용자 모델
    /// </summary>
    public class User
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// 테스트용 제품 모델
    /// </summary>
    public class Product
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Category { get; set; } = "";
        public int StockQuantity { get; set; }
    }

    /// <summary>
    /// 테스트용 간단한 모델
    /// </summary>
    public class SimpleModel
    {
        public string Value { get; set; } = "";
    }
}