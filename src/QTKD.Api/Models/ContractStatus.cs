namespace QTKD.Api.Models
{
    public enum ContractStatus
    {
        Draft = 0,
        PendingApproval = 1,
        Approved = 2,
        Rejected = 3,
        RevisionRequested = 4 //Yêu cầu chỉnh sửa lại hợp đồng
    }
}