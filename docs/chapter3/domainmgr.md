# The Domain Manager

As a request comes in to the mobile backend, it is processed through several layers.  First, ASP.NET
processes the request, handling things like Authentication and Authorization.  It is then processed
through the `Microsoft.Web.Http.OData` controller, which compiles the requested query.  Then it is 
passed to the Domain Manager, which is responsible for converting the request into a response.  The
response is then passed back up the stack to be finally given back to the mobile client.

The Domain Manager is a central part of this process.  It is a class that implements the `IDomainManager`
interface:

```csharp
namespace Microsoft.Azure.Mobile.Server.Tables
{
    public interface IDomainManager<TData> where TData : class, ITableData
    {
        IQueryable<TData> Query();
        SingleResult<TData> Lookup(string id);
        Task<IEnumerable<TData>> QueryAsync(ODataQueryOptions query);
        Task<SingleResult<TData>> LookupAsync(string id);
        Task<TData> InsertAsync(TData data);
        Task<TData> UpdateAsync(string id, Delta<TData> patch);
        Task<TData> ReplaceAsync(string id, TData data);
        Task<bool> DeleteAsync(string id);
    }
}
```

## Relationships with the MappedEntityDomainManager

## NoSQL Storage with the StorageDomainManager

## Implementing A Domain Manager
