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

This looks deceptively simple.  Just 8 methods.  In reality, this is anything but simple.  The major issue
that a prospective domain manager implementor has to grapple with is the translation of an `IQueryable` into
something that the backend data source can understand.

Before we get into implementing a domain manager, let's look at a couple of implementations that are provided
by the Azure Mobile team.

## Relationships with the MappedEntityDomainManager

One of the key areas that is weak when using the default `EntityDomainManager` is relationships.  Relationships
are core to the SQL database world and we want to project those relationships into the mobile client, allowing
the backend to preserve any relationships that have been configured while still using the standard offline client
capabilities.  To handle this case, one can use the `MappedEntityDomainManager`.  

The `MappedEntityDomainManager` is an `IDomainManager` implementation targetting SQL as the backend store where
there is not a 1:1 mapping between the data object (DTO) exposed through the TableController and the domain
model managed by Entity Framework.  If there is a 1:1 mapping, use `EntityDomainManager`.  The 
`MappedEntityDomainManager` uses [AutoMapper][1] to map between the DTO and the domain model.  It assumes
that AutoMapper has already been initialized with appropriate mappings that map from DTO to domain model
and from the domain model to the DTO.

Let's take a small example.  If I am producing an enterprise mobile app that field engineers can use - the ones
that, for example, visit your house to install cable.  I can define an Entity Framework model map as follows:

```csharp
[Table("Customers")]
public class Customer
{
    public Customer()
    {
        this.Jobs = new HashSet<Job>();
    }

    [StringLength(50)]
    public string Id { get; set; }

    [StringLength(50)]
    public string FullName { get; set; }

    [StringLength(250)]
    public string Address { get; set; 

    public decimal? Latitude { get; set; }
    
    public decimal? Longitude { get; set; }

    public ICollection<Job> Jobs { get; set; }
}

[Table("Equipment")]
public class Equipment : ITableData
{
    public Equipment()
    {
        this.Jobs = new HashSet<Job>();
    }

    [StringLength(50)]
    public string Name { get; set; }

    [StringLength(28)]
    public string Asset { get; set; }

    [StringLength(250)]
    public string Description { get; set; }

    #region ITableData
    public DateTimeOffset? CreatedAt { get; set; }
    public bool Deleted { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Timestamp]
    public byte[] Version { get; set; }
    #endregion

    public ICollection<Job> Jobs { get; set; }
}

[Table("Jobs")]
public class Job : ITableData
{
    public Job()
    {
        this.Equipments = new HashSet<Equipment>();
    }

    [StringLength(50)]
    public string CustomerId { get; set; }

    [StringLength(50)]
    public string AgentId { get; set; }

    public DateTimeOffset? StartTime { get; set; }

    public DateTimeOffset? EndTime { get; set; }

    [StringLength(50)]
    public string Status { get; set; }

    [StringLength(250)]
    public string Description { get; set; }

    #region ITableData
    public DateTimeOffset? CreatedAt { get; set; }
    public bool Deleted { get; set; }
    
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public DateTimeOffset? UpdatedAt { get; set; }

    [Timestamp]
    public byte[] Version { get; set; }
    #endregion

    public ICollection<Equipment> Equipments { get; set; }
}
```

This is the representation of the tables within the database.  They don't have to map to what the client
requires.  We can wire up the relationships in the normal [Entity Framework way][2], within the `DbContext`:

```csharp
public partial class ExistingDbContext : DbContext
{
    public FieldDbContext() : base("name=MS_TableConnectionString")
    {            
    }

    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Equipment> Equipments { get; set; }
    public virtual DbSet<Job> Jobs { get; set; } 

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {            
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.Conventions.Add(
            new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
            "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));

        modelBuilder.Entity<Customer>().Property(e => e.Address).IsUnicode(false);
        modelBuilder.Entity<Customer>().Property(e => e.FullName).IsUnicode(false);
        modelBuilder.Entity<Customer>().Property(e => e.Id).IsUnicode(false);
        modelBuilder.Entity<Customer>().Property(e => e.Latitude).HasPrecision(9, 6);
        modelBuilder.Entity<Customer>().Property(e => e.Longitude).HasPrecision(9, 6);

        modelBuilder.Entity<Equipment>().Property(e => e.Asset).IsUnicode(false);
        modelBuilder.Entity<Equipment>().Property(e => e.Description).IsUnicode(false);
        modelBuilder.Entity<Equipment>().Property(e => e.Name).IsUnicode(false);
        modelBuilder.Entity<Equipment>().Property(e => e.Id).IsUnicode(false);

        modelBuilder.Entity<Job>().Property(e => e.CustomerId).IsUnicode(false);
        modelBuilder.Entity<Job>().Property(e => e.AgentId).IsUnicode(false);
        modelBuilder.Entity<Job>().Property(e => e.Id).IsUnicode(false);
        modelBuilder.Entity<Job>().Property(e => e.Status).IsUnicode(false);
        modelBuilder.Entity<Job>().Property(e => e.Description).IsUnicode(false);

        modelBuilder.Entity<Equipment>()
            .HasMany(e => e.Jobs)
            .WithMany(e => e.Equipments)
            .Map(m => m.ToTable("EquipmentIds").MapLeftKey("EquipmentId").MapRightKey("JobId"));
    }
}
```

We can see the relationship (a Many:Many relationship) at the end of the `modelBuilder` within
the DbContext.  The 1:Many and 1:1 relationships are handled within the models themselves, per the
normal Entity Framework methods.  Thus far, this is pure Entity Framework - we have defined the
structure of the database.

Moving on to the data that the client expects, we need to define the Data Transfer Objects (DTOs) 
for these elements:

```csharp
public class CustomerDTO
{
    public string FullName { get; set; }
    public string Address { get; set; 
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class EquipmentDTO
{
    public string Name { get; set; }
    public string Asset { get; set; }
    public string Description { get; set; }
}

public class JobDTO : EntityData
{
    public string AgentId { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }  

    public virtual CustomerDTO Customer { get; set; }
    public virtual List<EquipmentDTO> Equipments { get; set; }
}
```

Note that the DTOs are similar, but definitely not the same.  They don't have the same relationships between
the records, for example.  `MappedEntityDomainManager` requires that AutoMapper is already configured and
initialized, so that's the next step.  Set up an AutoMapper configuration in the `App_Start` directory:

```csharp
using AutoMapper;
using FieldEngineer.Service.DataObjects;
using FieldEngineer.Service.Models;

namespace FieldEngineer.Service
{
    public class AutomapperConfiguration
    {
        public static void CreateMapping(IConfiguration cfg)
        {
            // Apply some name changes from the entity to the DTO
            cfg.CreateMap<Job, JobDTO>()                
                .ForMember(jobDTO => jobDTO.Equipments, map => map.MapFrom(job => job.Equipments));

            // For incoming requests, ignore the relationships
            cfg.CreateMap<JobDTO, Job>()                                            
                .ForMember(job => job.Customer, map => map.Ignore())
                .ForMember(job => job.Equipments, map => map.Ignore());

            cfg.CreateMap<Customer, CustomerDTO>();            
            cfg.CreateMap<Equipment, EquipmentDTO>();
        }
    }
}
```

You will also need to initialize the AutoMapper - this can be done where you also configure the Azure Mobile Apps:

```csharp
using System;
using System.Web.Http;
using AutoMapper;
using Microsoft.WindowsAzure.Mobile.Service;

namespace FieldEngineer.Service
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            // This is the line that initializes AutoMapper
            Mapper.Initialize(cfg => { AutomapperConfiguration.CreateMapping(cfg); });                                
        }
    }
}
```

Finally, we can create a controller that allows the receipt and update of jobs:

```csharp
namespace FieldEngineer.Service.Controllers
{
    [Authorize]  
    public class JobController : TableController<JobDTO>
    {      
        private FieldDbContext context;        

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.context = new FieldDbContext();
            
            this.DomainManager = new DefaultMappedEntityDomainManager<JobDTO,Job>(this.context, Request, Services);            
        }
        
        [ExpandProperty("Customer")]
        [ExpandProperty("Equipments")]
        public async Task<IQueryable<JobDTO>> GetAllJobs()
        {                        
            var jobs = this.context.Jobs
                .Include("Customer")
                .Include("Equipments")
                .Project().To<JobDTO>();            
            return jobs;
        }

        [ExpandProperty("Customer")]
        [ExpandProperty("Equipments")]
        public SingleResult<JobDTO> GetJob(string id)
        {
            return this.Lookup(id);
        }
        
        public async Task<JobDTO> PatchJob(string id, Delta<JobDTO> patch)
        {
            return await this.UpdateAsync(id, patch);                   
        }        
    }
}
```

We are using `[ExpandProperty]` to expand the Customer and Equipment data so that it is transferred with the Job object.
We also have to use a sub-classed version of the MappedEntityDomainManager to do some of the work.  The `MappedEntityDomainManager`
does not deal with replacements nor optimistic concurrency - features we want.  We can sub-class to `DefaultMappedEntityDomainManager`
to handle this for us:

```csharp
namespace FieldEngineerLite.Service.Helpers
{
    public class DefaultMappedEntityDomainManager<TData, TModel>
            : MappedEntityDomainManager<TData, TModel>
        where TData : class, ITableData
        where TModel : class, ITableData
    {
        public DefaultMappedEntityDomainManager(DbContext context, HttpRequestMessage request, ApiServices services)
            : base(context, request, services)
        {            
        }

        public override Task<bool> DeleteAsync(string id)
        {
            return this.DeleteItemAsync(id);
        }

        public override Task<TData> UpdateAsync(string id, Delta<TData> patch)
        {
            return this.UpdateEntityAsync(patch, id);
        }

        public override SingleResult<TData> Lookup(string id)
        {
            return this.LookupEntity(model => model.Id == id);
        }

        protected override void SetOriginalVersion(TModel model, byte[] version)
        {            
            this.Context.Entry(model).OriginalValues["Version"] = version;
        }
    }
}
```

The primary thing that the `DefaultMappedEntityDomainManager` does that the original doesn't is in the `SetOriginalVersion`
method.  This causes the model to be updated with a new version, allowing for conflict detection in the domain manager.

If we move now to the models on the mobile client, we see some fairly standard models:

```csharp
public class Customer
{
    public string Id { get; set; }
    public string FullName { get; set; }
    public string Address { get; set; 
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

public class Equipment
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Asset { get; set; }
    public string Description { get; set; }
}

public class Job
{
    public const string CompleteStatus = "Completed";
    public const string InProgressStatus = "On Site";
    public const string PendingStatus = "Not Started";

    public string Id { get; set; }
    public string AgentId { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffSet? EndTime { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }

    public Customer Customer { get; set; }
    public List<Equipment> Equipments { get; set; }

    [Version]
    public string Version { get; set; }
}
```

In this case, we only need to sync the Job table, so we can define it in the `InitializeAsync()` method on the client:

```csharp
public async Task InitializeAsync()
{
        var store = new MobileServiceSQLiteStore("localdata.db");
        store.DefineTable<Job>();
        await MobileService.SyncContext.InitializeAsync(store);
}
```

You can use `GetSyncTable<Job>()` to get a reference to the table and deal with it as you normally would.  I'd expect in this
case that the Customer and Equipment would be handled elsewhere - maybe a separate web application that customer service
agents use, for example.

It is well worth getting to grips with the [MappedEntityDomainManager][3] if you intend to do any serious work with relationships.
However, it is equally important to note that the Mobile Client does not support relationships.  Relationships are available within
the SQL server only and hence must be dealt with by the mobile backend.

## NoSQL Storage with the StorageDomainManager

## Implementing A Domain Manager

<!-- Links -->
[1]: http://automapper.org/
[2]: http://www.entityframeworktutorial.net/code-first/configure-one-to-one-relationship-in-code-first.aspx
[3]: https://github.com/Azure/azure-mobile-apps-net-server/blob/cc0c591e7a852f95cb3682b57b729e3876343338/src/Microsoft.Azure.Mobile.Server.Entity/MappedEntityDomainManager.cs
