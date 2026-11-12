using Moq;
using QTKD.Api.DTOs;
using QTKD.Api.Models;
using QTKD.Api.Repositories;
using QTKD.Api.Services;

namespace QTKD.Tests.UnitTests;

public class ContractServiceTests
{
    // Tạo ContractService với Mock Repository
    private static (ContractService service,
                    Mock<IContractRepository> mockContractRepo,
                    Mock<ICustomerRepository> mockCustomerRepo)
        BuildService()
    {
        var mockContractRepo = new Mock<IContractRepository>();
        var mockCustomerRepo = new Mock<ICustomerRepository>();
        var service = new ContractService(mockContractRepo.Object, mockCustomerRepo.Object);
        return (service, mockContractRepo, mockCustomerRepo);
    }

    // Tạo Customer Active mẫu
    private static Customer MakeActiveCustomer(int id = 1) => new Customer
    {
        Id = id,
        Code = "KH00001",
        Name = "Công ty TNHH Minh Phát",
        Status = CustomerStatus.Active
    };

    // Tạo Contract Draft mẫu (đầy đủ điều kiện submit)
    private static Contract MakeDraftContract(int id = 1, CustomerStatus customerStatus = CustomerStatus.Active) => new Contract
    {
        Id = id,
        ContractNo = "HD-TEST-001",
        CustomerId = 1,
        Customer = new Customer { Id = 1, Code = "KH00001", Name = "Công ty TNHH Minh Phát", Status = customerStatus },
        StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        Status = ContractStatus.Draft,
        TotalAmount = 10000000m,
        AttachmentPath = "/attachments/hop-dong-mau.pdf"
    };

    // TC03 — Sửa Contract ở trạng thái Draft → Thành công (CTR-01)
    [Fact]
    public async Task UpdateAsync_ContractIsDraft_ReturnsTrue()
    {
        // Arrange
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract();
        mockContractRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(contract);

        var request = new UpdateContractRequest
        {
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 20000000m,
            AttachmentPath = "/attachments/hop-dong-mau-v2.pdf"
        };

        var result = await service.UpdateAsync(1, request);

        Assert.True(result);

        // Xác nhận repository.UpdateAsync đã được gọi đúng 1 lần
        mockContractRepo.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Once);
    }

    // TC03 — Sửa Contract ở trạng thái RevisionRequested → Thành công (CTR-01)
    [Fact]
    public async Task UpdateAsync_ContractIsRevisionRequested_ReturnsTrue()
    {
        // Arrange
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract();
        contract.Status = ContractStatus.RevisionRequested; // trạng thái RevisionRequested
        mockContractRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(contract);

        var request = new UpdateContractRequest
        {
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 15000000m,
            AttachmentPath = "/attachments/revision.pdf"
        };

        var result = await service.UpdateAsync(1, request);

        Assert.True(result);
    }

    // TC04 — Sửa Contract ở PendingApproval → Bị từ chối (CTR-01)
    [Fact]
    public async Task UpdateAsync_ContractIsPendingApproval_ThrowsInvalidOperationException()
    {
        // Arrange
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract();
        contract.Status = ContractStatus.PendingApproval; // trạng thái bị chặn
        mockContractRepo
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(contract);

        var request = new UpdateContractRequest
        {
            StartDate = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 20000000m
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateAsync(1, request));

        // Kiểm tra thông báo lỗi có đề cập đến trạng thái bị chặn
        Assert.Contains("PendingApproval", ex.Message);

        // Xác nhận UpdateAsync của repository KHÔNG được gọi (dữ liệu giữ nguyên)
        mockContractRepo.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Never);
    }

    // TC05 — Submit thiếu AttachmentPath → Bị từ chối, vẫn Draft (CTR-02)
    [Fact]
    public async Task SubmitAsync_MissingAttachmentPath_ThrowsInvalidOperationException()
    {
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract();
        contract.AttachmentPath = null; // thiếu tệp đính kèm
        mockContractRepo
            .Setup(r => r.GetByIdWithCustomerAsync(1))
            .ReturnsAsync(contract);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(1));

        // Thông báo lỗi phải đề cập đến tệp tài liệu
        Assert.Contains("tệp tài liệu", ex.Message);

        // Xác nhận trạng thái hợp đồng KHÔNG thay đổi (vẫn Draft)
        Assert.Equal(ContractStatus.Draft, contract.Status);

        // Xác nhận UpdateAsync của repository KHÔNG được gọi
        mockContractRepo.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Never);
    }

    // TC05 — Submit với Customer Inactive → Bị từ chối (CTR-02)
    [Fact]
    public async Task SubmitAsync_CustomerInactive_ThrowsInvalidOperationException()
    {
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract(customerStatus: CustomerStatus.Inactive);
        mockContractRepo
            .Setup(r => r.GetByIdWithCustomerAsync(1))
            .ReturnsAsync(contract);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(1));

        Assert.Contains("Inactive", ex.Message);
        mockContractRepo.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Never);
    }

    // TC05 — Submit với StartDate > EndDate → Bị từ chối (CTR-02)
    [Fact]
    public async Task SubmitAsync_StartDateAfterEndDate_ThrowsInvalidOperationException()
    {
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract();
        contract.StartDate = new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        contract.EndDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc); // ngày kết thúc < ngày bắt đầu
        mockContractRepo
            .Setup(r => r.GetByIdWithCustomerAsync(1))
            .ReturnsAsync(contract);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(1));

        Assert.Contains("ngày", ex.Message.ToLower());
        mockContractRepo.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Never);
    }

    // TC06 — Submit hợp lệ → Chuyển sang PendingApproval (CTR-02, CTR-03)
    [Fact]
    public async Task SubmitAsync_ValidContract_ChangesToPendingApproval()
    {
        // Arrange
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract(); // Draft, Customer Active, có AttachmentPath, StartDate <= EndDate
        mockContractRepo
            .Setup(r => r.GetByIdWithCustomerAsync(1))
            .ReturnsAsync(contract);

        var result = await service.SubmitAsync(1);

        // TC06: trạng thái phải là PendingApproval
        Assert.Equal(ContractStatus.PendingApproval, result.Status);

        // TC07: trạng thái KHÔNG ĐƯỢC là Approved (CTR-03)
        Assert.NotEqual(ContractStatus.Approved, result.Status);

        // Xác nhận UpdateAsync của repository đã được gọi đúng 1 lần
        mockContractRepo.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Once);
    }

    // TC07 — Không chuyển Draft thẳng sang Approved (CTR-03)
    // Được đảm bảo bởi: SubmitAsync chỉ set PendingApproval,
    // không có code path nào đặt thẳng Approved.
    [Fact]
    public async Task SubmitAsync_ValidContract_NeverSetApproved()
    {
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract();
        mockContractRepo
            .Setup(r => r.GetByIdWithCustomerAsync(1))
            .ReturnsAsync(contract);

        var result = await service.SubmitAsync(1);

        //Sau Submit, kết quả KHÔNG là Approved
        Assert.NotEqual(ContractStatus.Approved, result.Status);
        // phải là PendingApproval
        Assert.Equal(ContractStatus.PendingApproval, result.Status);
    }

    // TC08 — Submit lặp hợp đồng đang PendingApproval → Bị từ chối
    [Fact]
    public async Task SubmitAsync_AlreadyPendingApproval_ThrowsInvalidOperationException()
    {
        var (service, mockContractRepo, _) = BuildService();

        var contract = MakeDraftContract();
        contract.Status = ContractStatus.PendingApproval; // đã gửi trước đó rồi
        mockContractRepo
            .Setup(r => r.GetByIdWithCustomerAsync(1))
            .ReturnsAsync(contract);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SubmitAsync(1));

        // Thông báo lỗi phải đề cập đến việc đã gửi rồi
        Assert.Contains("đã được gửi", ex.Message);

        // Xác nhận UpdateAsync KHÔNG được gọi thêm lần nào
        mockContractRepo.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Never);
    }

    // Submit hợp đồng không tồn tại → 404
    [Fact]
    public async Task SubmitAsync_ContractNotFound_ThrowsKeyNotFoundException()
    {
        var (service, mockContractRepo, _) = BuildService();

        mockContractRepo
            .Setup(r => r.GetByIdWithCustomerAsync(999))
            .ReturnsAsync((Contract?)null); // không tìm thấy

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.SubmitAsync(999));
    }
}