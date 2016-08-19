using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Chapter3.Models
{
    public class MobileServiceContext : DbContext
    {
        private const string connectionStringName = "Name=MS_TableConnectionString";

        public MobileServiceContext() : base(connectionStringName)
        {
            Database.Log = s => WriteLog(s);
        }

        public void WriteLog(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
        }

        public DbSet<DataObjects.TodoItem> TodoItems { get; set; }
        public DbSet<DataObjects.Example> Examples { get; set; }
        public DbSet<DataObjects.User> Users { get; set; }
        public DbSet<DataObjects.Friend> Friends { get; set; }
        public DbSet<DataObjects.Message> Messages { get; set; }
    }
}
