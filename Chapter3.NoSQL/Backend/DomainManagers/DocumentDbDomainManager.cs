using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.Azure.Documents.Linq;

namespace Backend.DomainManagers
{
    public class DocumentDbDomainManager<TData> : IDomainManager<TData> where TData : class, ITableData
    {
        /// <summary>
        /// The default name of the connection string within Azure App Service
        /// </summary>
        private const string DefaultConnectionStringName = "MS_AzureDocumentDbConnectionString";

        /// <summary>
        /// The default name of the database that the table data will be stored in
        /// </summary>
        private const string DefaultDatabaseName = "AzureMobile";

        /// <summary>
        /// The default name of the collection that the table data will be stored in.  Use null for "the model name"
        /// </summary>
        private const string DefaultCollectionName = null;

        private Regex DefaultIdentifierPattern = new Regex("^[A-Za-z0-9]{2,255}$", RegexOptions.Compiled);

        #region Constructors
        /// <summary>
        /// Create a new instance of the <see cref="DocumentDbDomainManager{TData}"/> using all defaults.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> object</param>
        public DocumentDbDomainManager(HttpRequestMessage request)
            : this(DefaultConnectionStringName, DefaultDatabaseName, DefaultCollectionName, request)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="DocumentDbDomainManager{TData}"/> with a specific collection name
        /// </summary>
        /// <param name="collectionName">The DocumentDb Collection to store the data in</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> object</param>
        public DocumentDbDomainManager(string collectionName, HttpRequestMessage request)
            : this(DefaultConnectionStringName, DefaultDatabaseName, collectionName, request)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="DocumentDbDomainManager{TData}"/>.
        /// </summary>
        /// <param name="databaseName">The DocumentDb database to store the data in</param>
        /// <param name="collectionName">The DocumentDb collection to store the data in</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> object</param>
        public DocumentDbDomainManager(string databaseName, string collectionName, HttpRequestMessage request)
            : this(DefaultConnectionStringName, databaseName, collectionName, request)
        {
        }

        /// <summary>
        /// Create a new instance of the <see cref="DocumentDbDomainManager{TData}"/>.
        /// </summary>
        /// <param name="connectionString">The connection string or the name of the connection string app setting.</param>
        /// <param name="databaseName">The name of the DocumentDb database to store the data in</param>
        /// <param name="collectionName">The name of the DocumentDb collection to store the data in</param>
        /// <param name="request">The <see cref="HttpRequestMessage"/> object</param>
        public DocumentDbDomainManager(string connectionString, string databaseName, string collectionName, HttpRequestMessage request)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (databaseName == null)     throw new ArgumentNullException(nameof(databaseName));
            if (request == null)          throw new ArgumentNullException(nameof(request));

            // Store the request for later usage.
            Request = request;

            // Transform the connection string name into the connection string.
            if (connectionString.StartsWith("AccountEndpoint="))
            {
                ConnectionString = connectionString;
            }
            else
            {
                var settings = Request
                    .GetConfiguration()
                    .GetMobileAppSettingsProvider()
                    .GetMobileAppSettings();
                ConnectionSettings connectionSettings;
                if (!settings.Connections.TryGetValue(connectionString, out connectionSettings))
                {
                    throw new ArgumentException($"Connection String {connectionString} not found", connectionString);
                }
                ConnectionString = connectionSettings.ConnectionString;
            }

            // Transform the connection string into the service endpoint and the authKey
            Regex connectionStringPattern = new Regex("^AccountEndpoint=(.*?);AccountKey=(.*)");
            Match m = connectionStringPattern.Match(ConnectionString);
            if (!m.Success)
            {
                throw new ArgumentException($"Connection String {ConnectionString} is invalid (bad form)", connectionString);
            }
            try
            {
                ServiceEndpoint = new Uri(m.Groups[1].Value);
                AuthKey = m.Groups[2].Value;
            }
            catch (UriFormatException)
            {
                throw new ArgumentException($"Connection String {ConnectionString} has a bad URI for the AccountEndpoint", connectionString);
            }

            // Check the format of the database - for consistency, the rules are checked here.
            DatabaseName = databaseName;
            if (!IsValidDocumentDbIdentifier(DatabaseName))
            {
                throw new ArgumentException($"Database {DatabaseName} is invalid", databaseName);
            }

            // Check the format of the collection - for consistency, the rules are checked here.
            CollectionName = collectionName ?? typeof(TData).Name;
            if (!IsValidDocumentDbIdentifier(CollectionName))
            {
                throw new ArgumentException($"Collection {CollectionName} is invalid", collectionName);
            }

            // Create the client connection to DocumentDb
            Client = new DocumentClient(ServiceEndpoint, AuthKey);

            // Create the reference for the database
            try
            {
                Database = Client.CreateDatabaseQuery().Where(db => db.Id == DatabaseName).AsEnumerable().FirstOrDefault();
                if (Database == null)
                {
                    Database = Client.CreateDatabaseAsync(new Database { Id = DatabaseName }).Result;
                }
            }
            catch (DocumentClientException dbCreationResult)
            {
                if (dbCreationResult.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new ArgumentException($"Database {DatabaseName} exists, but cannot be read", databaseName);
                }
                if (dbCreationResult.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new ArgumentException($"Database {DatabaseName} is invalid (check Validator)", databaseName);
                }
                throw new ArgumentException($"Database Creation for {DatabaseName} failed", databaseName, dbCreationResult);
            }

            // Create the reference for the collection
            try
            {
                Collection = Client.CreateDocumentCollectionQuery(Database.SelfLink).Where(coll => coll.Id == CollectionName).AsEnumerable().FirstOrDefault();
                if (Collection == null)
                {
                    Collection = Client.CreateDocumentCollectionAsync(Database.SelfLink, new DocumentCollection { Id = CollectionName }).Result;
                }
            }
            catch (DocumentClientException collCreationResult)
            {
                if (collCreationResult.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new ArgumentException($"Collection {CollectionName} exists, but cannot be read", collectionName);
                }
                if (collCreationResult.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new ArgumentException($"Collection {CollectionName} could not be created - quota exceeded", collectionName);
                }
                if (collCreationResult.StatusCode == HttpStatusCode.BadRequest)
                {
                    throw new ArgumentException($"Collection {CollectionName} is invalid (check Validator)", collectionName);
                }
                throw new ArgumentException($"Collection Creation for {CollectionName} failed", collectionName, collCreationResult);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="HttpRequestMessage"/> used to create this instance
        /// </summary>
        public HttpRequestMessage Request { get; }

        /// <summary>
        /// The connection string to use to connect to the service endpoint
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The DocumentDb service endpoint
        /// </summary>
        public Uri ServiceEndpoint { get; set; }

        /// <summary>
        /// The authentication key for the DocumentDb service endpoint
        /// </summary>
        public string AuthKey { get; set; }

        /// <summary>
        /// The name of the database within the DocumentDb service
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// The name of the collection within the DocumentDb database
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// The client for DocumentDb endpoint
        /// </summary>
        private DocumentClient Client { get; set; }

        /// <summary>
        /// The reference to the DocumentDb database
        /// </summary>
        private Database Database { get; set; }

        /// <summary>
        /// The reference to the DocumentDb collection
        /// </summary>
        private DocumentCollection Collection { get; set; }
        #endregion

        #region Private Methods
        /// <summary>
        /// Determine if the provided Id is a valid DocumentDb Identifier
        /// </summary>
        /// <param name="id">The identifier to test</param>
        /// <returns>true if the identifier is valid</returns>
        private bool IsValidDocumentDbIdentifier(string id)
        {
            if (id.Length < 2 || id.Length > 255)
            {
                return false;
            }
            var match = DefaultIdentifierPattern.Match(id);
            return match.Success;
        }
        #endregion

        #region IDomainManager{TData} Interface
        /// <summary>
        /// Builds an <see cref="IQueryable{T}"/> to be executed against a store supporting <see cref="IQueryable{T}"/> for querying data.
        /// </summary>
        /// <remarks>
        /// See also <see cref="M:Lookup"/> which is the companion method for creating an <see cref="IQueryable{T}"/> representing a single item.
        /// </remarks>
        /// <returns>An <see cref="IQueryable{T}"/> which has not yet been executed.</returns>
        public IQueryable<TData> Query()
        {
            try
            {
                return Client.CreateDocumentQuery<TData>(Collection.SelfLink);
            }
            catch (DocumentQueryException queryException)
            {
                throw new HttpResponseException(queryException.StatusCode ?? HttpStatusCode.InternalServerError);
            }
            catch (DocumentClientException clientException)
            {
                throw new HttpResponseException(clientException.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Builds an <see cref="IQueryable{T}"/> to be executed against a store supporting <see cref="IQueryable{T}"/> for looking up a single item.
        /// </summary>
        /// <param name="id">The id representing the item. The id is provided as part of the <see cref="ITableData"/> and is visible to the client.
        /// However, depending on the backend store and the domain model, the particular implementation may map the id to some other form of unique
        /// identifier.</param>
        /// <remarks>
        /// See also <see cref="M:Query"/> which is the companion method for creating an <see cref="IQueryable{T}"/> representing multiple items.
        /// </remarks>
        /// <returns>A <see cref="SingleResult{T}"/> containing the <see cref="IQueryable{T}"/> which has not yet been executed.</returns>
        public SingleResult<TData> Lookup(string id)
        {
            throw TableUtils.GetNoQueryableLookupException(this.GetType(), "Lookup");
        }

        /// <summary>
        /// Executes the provided <paramref name="query"/> against a store.
        /// </summary>
        /// <remarks>
        /// See also <see cref="M:LookupAsync"/> which is the companion method for executing a lookup for a single item.
        /// </remarks>
        /// <param name="query">The <see cref="ODataQueryOptions"/> query to execute.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> representing the result of the query.</returns>
        public Task<IEnumerable<TData>> QueryAsync(ODataQueryOptions query)
        {
            throw TableUtils.GetQueryableOnlyQueryException(this.GetType(), "Query");
        }

        /// <summary>
        /// Looks up a single item in the backend store.
        /// </summary>
        /// <param name="id">The id representing the item. The id is provided as part of the <see cref="ITableData"/> and is visible to the client.
        /// However, depending on the backend store and the domain model, the particular implementation may map the id to some other form of unique
        /// identifier.</param>
        /// <remarks>
        /// See also <see cref="M:QueryAsync"/> which is the companion method for executing a query for multiple items.
        /// </remarks>
        /// <returns>A <see cref="SingleResult{T}"/> representing the result of the lookup. A <see cref="SingleResult{T}"/> represents an
        /// <see cref="IQueryable"/> containing zero or one entities. This allows it to be composed with further querying such as <c>$select</c>.</returns>
        public async Task<SingleResult<TData>> LookupAsync(string id)
        {
            var list = new List<TData>();
            try
            {
                var response = await Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(Database.Id, Collection.Id, id));
                list.Add((TData)(dynamic)response.Resource);

            }
            catch (DocumentClientException clientException)
            {
                // If the request results in a 404 Not Found, fall-through.  Otherwise, throw the same status code.
                // If we don't have a status code, something bad happened - throw a 500 Internal Server Error
                if (clientException.StatusCode != HttpStatusCode.NotFound)
                {
                    throw new HttpResponseException(clientException.StatusCode ?? HttpStatusCode.InternalServerError);
                }

            }
            return SingleResult.Create<TData>(list.AsQueryable());
        }

        /// <summary>
        /// Inserts an item to the backend store.
        /// </summary>
        /// <param name="data">The data to be inserted</param>
        /// <returns>The inserted item.</returns>
        public async Task<TData> InsertAsync(TData data)
        {
            try
            {
                data.CreatedAt = data.CreatedAt ?? DateTimeOffset.UtcNow;
                data.UpdatedAt = DateTimeOffset.UtcNow;
                data.Version = Guid.NewGuid().ToByteArray();
                var response = await Client.CreateDocumentAsync(Collection.SelfLink, data);
                return (TData)(dynamic)response.Resource;
            }
            catch (DocumentClientException clientException)
            {
                throw new HttpResponseException(clientException.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Updates an existing item by applying a <see cref="Delta{T}"/> patch to it. The <see cref="Delta{T}"/>
        /// abstraction keeps track of which properties have changed which avoids problems with default values and
        /// the like.
        /// </summary>
        /// <param name="id">The id of the item to patch.</param>
        /// <param name="patch">The patch to apply.</param>
        /// <returns>The patched item.</returns>
        public async Task<TData> UpdateAsync(string id, Delta<TData> patch)
        {
            try
            {
                // Get the link to the document
                var document = (await Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(Database.Id, Collection.Id, id))).Resource;

                // Update the data based on the patch
                var tdata = (TData)(dynamic)document;
                patch.Patch(tdata);
                tdata.UpdatedAt = DateTimeOffset.UtcNow;
                tdata.Version = Guid.NewGuid().ToByteArray();

                // Replace the document in the store
                var response = await Client.ReplaceDocumentAsync(document.SelfLink, tdata);
                return (TData)(dynamic)response.Resource;
            }
            catch (DocumentClientException clientException)
            {
                throw new HttpResponseException(clientException.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Completely replaces an existing item.
        /// </summary>
        /// <param name="id">The id of the item to replace.</param>
        /// <param name="data">The replacement</param>
        /// <returns>The replaced item</returns>
        public async Task<TData> ReplaceAsync(string id, TData data)
        {
            if (data == null || data.Id != id)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            try
            {
                var document = (await Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(Database.Id, Collection.Id, id))).Resource;
                data.UpdatedAt = DateTimeOffset.UtcNow;
                data.Version = Guid.NewGuid().ToByteArray();
                var response = await Client.ReplaceDocumentAsync(document.SelfLink, data);
                return (TData)(dynamic)response.Resource;
            }
            catch (DocumentClientException clientException)
            {
                throw new HttpResponseException(clientException.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Deletes an existing item
        /// </summary>
        /// <param name="id">The id of the item to delete.</param>
        /// <returns><c>true</c> if item was deleted; otherwise <c>false</c></returns>
        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var document = (await Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(Database.Id, Collection.Id, id))).Resource;
                await Client.DeleteDocumentAsync(document.SelfLink);
                return true;
            }
            catch (DocumentClientException clientException)
            {
                if (clientException.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw new HttpResponseException(clientException.StatusCode ?? HttpStatusCode.InternalServerError);
            }
        }
        #endregion
    }
}