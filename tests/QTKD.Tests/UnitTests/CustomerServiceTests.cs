using System.ComponentModel.DataAnnotations;
using Moq;
using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;
using QTKD.Api.Services;

namespace QTKD.Tests.UnitTests;

public class CustomerServiceTests
{
    // Tạo CustomerService với Mock Repository
    private static (CustomerService service, Mock<ICustomerRepository> mockRepo)
        BuildService()
    {
        var mockRepo = new Mock<ICustomerRepository>();
        var service = new CustomerService(mockRepo.Object);
        return (service, mockRepo);
    }

    // TC01 — Tạo Customer hợp lệ → Tạo được, trả về response đúng
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCustomerResponse()
    {
        var (service, mockRepo) = BuildService();

        var request = new CreateCustomerRequest
        {
            Code = "KH99999",
            Name = "Công ty Kiểm Thử",
            TaxCode = "1234567890",
            Address = "99 Đường Test, Quận 1, TP.HCM"
        };

        // Giả lập: Code chưa tồn tại (trả về null)
        mockRepo
            .Setup(r => r.GetByCodeAsync("KH99999"))
            .ReturnsAsync((Customer?)null);

        // Giả lập: AddAsync lưu thành công và trả về entity có Id
        mockRepo
            .Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync((Customer c) =>
            {
                c.Id = 99; // giả lập DB gán Id
                return c;
            });

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(99, result.Id);
        Assert.Equal("KH99999", result.Code);        // Code được lưu đúng
        Assert.Equal("Công ty Kiểm Thử", result.Name);
        Assert.Equal(CustomerStatus.Active, result.Status); // Status mặc định là Active

        // Xác nhận AddAsync được gọi đúng 1 lần
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
    }

    // TC01 — Đọc lại Customer vừa tạo theo Id → Trả về đúng dữ liệu
    [Fact]
    public async Task GetByIdAsync_ExistingCustomer_ReturnsCustomerResponse()
    {
        var (service, mockRepo) = BuildService();

        var customer = new Customer
        {
            Id = 1,
            Code = "KH00001",
            Name = "Công ty TNHH Minh Phát",
            Status = CustomerStatus.Active
        };

        mockRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(customer);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("KH00001", result.Code);
        Assert.Equal("Công ty TNHH Minh Phát", result.Name);
        Assert.Equal(CustomerStatus.Active, result.Status);
    }

    // TC02 — Tạo Customer thiếu trường bắt buộc → Validation thất bại
    // Kiểm tra Data Annotations trên CreateCustomerRequest
    [Fact]
    public void CreateCustomerRequest_MissingCode_FailsValidation()
    {
        var request = new CreateCustomerRequest
        {
            Code = "",       // Code bị bỏ trống, vi phạm
            Name = "Tên hợp lệ"
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateCustomerRequest.Code)));
    }

    [Fact]
    public void CreateCustomerRequest_MissingName_FailsValidation()
    {
        var request = new CreateCustomerRequest
        {
            Code = "KH00001",
            Name = ""        // Name bị bỏ trống, vi phạm
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateCustomerRequest.Name)));
    }

    [Fact]
    public void CreateCustomerRequest_BothCodeAndName_PassesValidation()
    {
        // Dữ liệu hợp lệ
        var request = new CreateCustomerRequest
        {
            Code = "KH00001",
            Name = "Công ty TNHH ABC"
        };

        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(request, context, validationResults, validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(validationResults);
    }

    // TC02 — Tạo Customer trùng mã → Bị từ chối bởi Service
    [Fact]
    public async Task CreateAsync_DuplicateCode_ThrowsInvalidOperationException()
    {
        var (service, mockRepo) = BuildService();

        var existingCustomer = new Customer { Id = 1, Code = "KH00001", Name = "Khách hàng cũ" };

        // Giả lập: Code đã tồn tại
        mockRepo
            .Setup(r => r.GetByCodeAsync("KH00001"))
            .ReturnsAsync(existingCustomer);

        var request = new CreateCustomerRequest
        {
            Code = "KH00001", // trùng mã
            Name = "Khách hàng mới"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(request));

        // Thông báo lỗi phải đề cập đến mã trùng
        Assert.Contains("KH00001", ex.Message);

        // Xác nhận AddAsync KHÔNG được gọi
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Never);
    }

    // GetByIdAsync Customer không tồn tại → Trả null
    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var (service, mockRepo) = BuildService();

        mockRepo
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Customer?)null);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }
}