using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using QTKD.Api.DTOs;
using QTKD.Api.Models;

namespace QTKD.Tests.IntegrationTests;

public class ContractApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ContractApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // Tạo Customer qua API, trả về CustomerResponse
    private async Task<CustomerResponse> CreateCustomerAsync(string code = "KH_IT_001", string name = "Công ty Integration Test")
    {
        var request = new CreateCustomerRequest
        {
            Code = code,
            Name = name,
            TaxCode = "1234567890",
            Address = "99 Test Street"
        };

        var response = await _client.PostAsJsonAsync("/api/customers", request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CustomerResponse>(body, _jsonOpts)!;
    }

    // Tạo Contract qua API, trả về ContractResponse
    private async Task<ContractResponse> CreateContractAsync(
        int customerId,
        string? attachmentPath = null,
        string contractNo = "HD-IT-001")
    {
        var request = new CreateContractRequest
        {
            ContractNo = contractNo,
            CustomerId = customerId,
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 10000000m,
            AttachmentPath = attachmentPath
        };

        var response = await _client.PostAsJsonAsync("/api/contracts", request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ContractResponse>(body, _jsonOpts)!;
    }

    // TC01 — Tạo Customer hợp lệ → 201 Created, đọc lại từ DB được
    [Fact]
    public async Task CreateCustomer_ValidRequest_Returns201AndCanBeReadBack()
    {
        // Gọi POST /api/customers
        var request = new CreateCustomerRequest
        {
            Code = "KH_TC01",
            Name = "Công ty TC01",
            TaxCode = "1111111111"
        };
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // HTTP Status phải là 201 Created
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Response body phải có dữ liệu đúng
        var body = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerResponse>(body, _jsonOpts);
        Assert.NotNull(customer);
        Assert.Equal("KH_TC01", customer.Code);
        Assert.Equal("Công ty TC01", customer.Name);
        Assert.Equal(CustomerStatus.Active, customer.Status);

        // Đọc lại từ DB được (GET /api/customers/{id})
        var getResponse = await _client.GetAsync($"/api/customers/{customer.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var fetchedCustomer = JsonSerializer.Deserialize<CustomerResponse>(getBody, _jsonOpts);
        Assert.NotNull(fetchedCustomer);
        Assert.Equal("KH_TC01", fetchedCustomer.Code);
    }

    // TC02 — Tạo Customer thiếu thông tin bắt buộc → 400 Bad Request
    [Fact]
    public async Task CreateCustomer_MissingRequiredFields_Returns400()
    {
        // Gửi request với Code và Name rỗng
        var json = """{"Code": "", "Name": ""}""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/customers", content);

        // Phải là 400 Bad Request (ModelState validation thất bại)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Response body phải có thông tin lỗi (không phải rỗng)
        var body = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(body);
    }

    // TC03 — Sửa Contract ở trạng thái Draft → 200 OK, thành công
    [Fact]
    public async Task UpdateContract_WhenDraft_Returns200()
    {
        // Tạo Customer rồi tạo Contract Draft
        var customer = await CreateCustomerAsync("KH_TC03", "Công ty TC03");
        var contract = await CreateContractAsync(customer.Id, "/attachments/test.pdf", "HD-TC03-001");

        // Xác nhận Contract đang ở Draft
        Assert.Equal(ContractStatus.Draft, contract.Status);

        // Gọi PUT /api/contracts/{id}
        var updateRequest = new UpdateContractRequest
        {
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 20000000m,
            AttachmentPath = "/attachments/updated.pdf"
        };
        var response = await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", updateRequest);

        // 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Đọc lại trạng thái vẫn là Draft (không bị đổi khi sửa)
        var getResponse = await _client.GetAsync($"/api/contracts/{contract.Id}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var updated = JsonSerializer.Deserialize<ContractResponse>(getBody, _jsonOpts);
        Assert.NotNull(updated);
        Assert.Equal(ContractStatus.Draft, updated.Status);
    }

    // TC04 — Sửa Contract ở PendingApproval → 400 Bad Request, dữ liệu giữ nguyên
    [Fact]
    public async Task UpdateContract_WhenPendingApproval_Returns400AndDataUnchanged()
    {
        // Tạo Customer, Contract Draft đầy đủ điều kiện, rồi Submit
        var customer = await CreateCustomerAsync("KH_TC04", "Công ty TC04");
        var contract = await CreateContractAsync(customer.Id, "/attachments/tc04.pdf", "HD-TC04-001");

        // Submit → chuyển sang PendingApproval
        var submitResp = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);
        Assert.Equal(HttpStatusCode.OK, submitResp.StatusCode);

        // Xác nhận trạng thái đã là PendingApproval
        var getBeforeUpdate = await _client.GetAsync($"/api/contracts/{contract.Id}");
        var bodyBefore = await getBeforeUpdate.Content.ReadAsStringAsync();
        var contractBefore = JsonSerializer.Deserialize<ContractResponse>(bodyBefore, _jsonOpts);
        Assert.Equal(ContractStatus.PendingApproval, contractBefore!.Status);

        // Thử sửa Contract đang PendingApproval
        var updateRequest = new UpdateContractRequest
        {
            StartDate = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            TotalAmount = 99999999m
        };
        var response = await _client.PutAsJsonAsync($"/api/contracts/{contract.Id}", updateRequest);

        // 400 Bad Request (CTR-01)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Dữ liệu Contract giữ nguyên (vẫn PendingApproval, TotalAmount không đổi)
        var getAfterUpdate = await _client.GetAsync($"/api/contracts/{contract.Id}");
        var bodyAfter = await getAfterUpdate.Content.ReadAsStringAsync();
        var contractAfter = JsonSerializer.Deserialize<ContractResponse>(bodyAfter, _jsonOpts);
        Assert.Equal(ContractStatus.PendingApproval, contractAfter!.Status);
        Assert.NotEqual(99999999m, contractAfter.TotalAmount); // TotalAmount không được thay đổi
    }

    // TC05 — Submit Contract thiếu AttachmentPath → 400 Bad Request, vẫn là Draft
    [Fact]
    public async Task SubmitContract_MissingAttachment_Returns400AndRemainsAsDraft()
    {
        // Tạo Contract Draft KHÔNG có AttachmentPath
        var customer = await CreateCustomerAsync("KH_TC05", "Công ty TC05");
        var contract = await CreateContractAsync(
            customer.Id,
            attachmentPath: null,  // thiếu tệp đính kèm
            contractNo: "HD-TC05-001");

        // Gọi POST /api/contracts/{id}/submit
        var response = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);

        // 400 Bad Request (CTR-02 bị vi phạm)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Thông báo lỗi không rỗng
        var body = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(body);

        // Đọc lại DB — Contract vẫn là Draft
        var getResponse = await _client.GetAsync($"/api/contracts/{contract.Id}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var contractAfter = JsonSerializer.Deserialize<ContractResponse>(getBody, _jsonOpts);
        Assert.Equal(ContractStatus.Draft, contractAfter!.Status);
    }

    // TC06 — Submit Contract đủ điều kiện → 200 OK, chuyển sang PendingApproval (CTR-03)
    [Fact]
    public async Task SubmitContract_ValidContract_Returns200AndStatusIsPendingApproval()
    {
        // Tạo Contract Draft đầy đủ: Customer Active, có AttachmentPath, StartDate <= EndDate
        var customer = await CreateCustomerAsync("KH_TC06", "Công ty TC06");
        var contract = await CreateContractAsync(
            customer.Id,
            attachmentPath: "/attachments/hop-dong-tc06.pdf",
            contractNo: "HD-TC06-001");

        // Gọi POST /api/contracts/{id}/submit
        var response = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);

        // 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Đọc lại DB — trạng thái phải là PendingApproval (CTR-03)
        var getResponse = await _client.GetAsync($"/api/contracts/{contract.Id}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var contractAfter = JsonSerializer.Deserialize<ContractResponse>(getBody, _jsonOpts);
        Assert.Equal(ContractStatus.PendingApproval, contractAfter!.Status);

        // Status KHÔNG được là Approved (CTR-03)
        Assert.NotEqual(ContractStatus.Approved, contractAfter.Status);
    }

    // TC07 — Không chuyển Draft thẳng sang Approved (CTR-03) — Integration version
    // Submit thành công chỉ được đặt PendingApproval, không bao giờ Approved
    [Fact]
    public async Task SubmitContract_ValidContract_NeverSetsApproved()
    {
        var customer = await CreateCustomerAsync("KH_TC07", "Công ty TC07");
        var contract = await CreateContractAsync(customer.Id, "/attachments/tc07.pdf", "HD-TC07-001");

        var response = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Đọc lại trạng thái từ DB
        var getResponse = await _client.GetAsync($"/api/contracts/{contract.Id}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var contractAfter = JsonSerializer.Deserialize<ContractResponse>(getBody, _jsonOpts);

        // Dứt khoát không phải Approved
        Assert.NotEqual(ContractStatus.Approved, contractAfter!.Status);
        Assert.Equal(ContractStatus.PendingApproval, contractAfter.Status);
    }

    // TC08 — Submit lặp cùng một Contract → 400 Bad Request lần 2
    [Fact]
    public async Task SubmitContract_SubmitTwice_Returns400OnSecondSubmit()
    {
        // Tạo Contract hợp lệ
        var customer = await CreateCustomerAsync("KH_TC08", "Công ty TC08");
        var contract = await CreateContractAsync(customer.Id, "/attachments/tc08.pdf", "HD-TC08-001");

        // Submit lần 1 — phải thành công
        var firstSubmit = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);
        Assert.Equal(HttpStatusCode.OK, firstSubmit.StatusCode);

        // Submit lần 2 (cùng Contract đang PendingApproval)
        var secondSubmit = await _client.PostAsync($"/api/contracts/{contract.Id}/submit", null);

        // Lần 2 phải là 400 Bad Request
        Assert.Equal(HttpStatusCode.BadRequest, secondSubmit.StatusCode);

        // Thông báo lỗi có nội dung hợp lý
        var body = await secondSubmit.Content.ReadAsStringAsync();
        Assert.NotEmpty(body);

        // DB vẫn là PendingApproval (không tạo thêm bản xử lý)
        var getResponse = await _client.GetAsync($"/api/contracts/{contract.Id}");
        var getBody = await getResponse.Content.ReadAsStringAsync();
        var contractAfter = JsonSerializer.Deserialize<ContractResponse>(getBody, _jsonOpts);
        Assert.Equal(ContractStatus.PendingApproval, contractAfter!.Status);
    }
}