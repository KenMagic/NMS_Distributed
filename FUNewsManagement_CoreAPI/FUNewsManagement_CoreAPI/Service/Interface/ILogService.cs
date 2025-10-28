namespace FUNewsManagement_CoreAPI.Service.Interface
{
    public interface ILogService
    {
        void AddLog(int? accountId, string entityName, string action, object? before, object? after);
    }
}
