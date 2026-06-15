# Proposal: Second Edition

**Working title:** *Develop Cloud-Connected Apps with .NET MAUI and the Datasync Toolkit*

> This is a living proposal for the second edition of the book. It exists so we can
> iterate on the structure, the chapter contents, and the topics before any prose is
> written. Nothing here is final. Add comments, strike things out, move things around.

---

## Why a second edition?

The first edition — *Develop Cloud Connected Mobile Apps with Xamarin and Microsoft
Azure* — was written between 2017 and 2020. The world has moved on:

- **Xamarin became .NET MAUI.** Xamarin.Forms reached end of support; .NET MAUI is the
  modern, single-project, multi-target successor. UWP/Windows Phone heads are gone,
  replaced by WinUI 3 / the Windows App SDK.
- **Azure Mobile Apps became the Community Datasync Toolkit.** The `Microsoft.Azure.Mobile.Server.*`
  stack (ASP.NET WebAPI on .NET Framework + OWIN, OData v3, EF6, Domain Managers) has been
  replaced by the **Community Toolkit Datasync** libraries running on **ASP.NET Core**,
  **OData v4**, and **EF Core**, with a pluggable **repository** model.
- **.NET 10 is here.** The toolkit targets `net10.0`; the library version tracks the .NET
  version. The first edition targeted .NET Framework 4.6 / .NET Standard 2.0.
- **Authentication changed completely.** App Service "Easy Auth" + ZUMO tokens + ADAL are
  out. The toolkit delegates entirely to standard **ASP.NET Core authentication** with
  **Microsoft Entra ID** and **MSAL**. Auth is now "bring your own."
- **Batteries are no longer included.** The old service bundled push notifications, custom
  APIs, social-login endpoints, and a token store. The toolkit is focused: it does data and
  offline sync. Push, real-time, and auth are now things you wire up yourself with modern,
  first-class .NET components.
- **The supporting cast aged out.** Azure Media Services retired; DocumentDB became Cosmos
  DB; Azure Search became Azure AI Search; WebJobs gave way to isolated-worker Azure
  Functions; ARM templates gave way to Bicep and the Azure Developer CLI (`azd`); Visual
  Studio Mobile Center / App Center is retiring.
- **The book is no longer an "Azure" book.** The first edition leaned on Azure because of
  the author's association with Microsoft at the time; that is no longer the case. The
  Datasync Toolkit is just an ASP.NET Core app plus a database, so the *client and the core
  concepts are entirely cloud-agnostic*. The main narrative still uses a single hyperscaler
  as its worked example (so the deployment steps stay concrete), but the second edition
  explicitly treats the cloud as a swappable detail and adds a chapter surveying the
  alternatives.

The Community Datasync Toolkit is the **spiritual successor** to Azure Mobile Apps — and
notably, a Datasync Toolkit server is still **protocol-compatible** with Azure Mobile Apps
v6 clients. There is, today, **no migration guide anywhere** for readers coming from the
first edition. That is a gap this edition fills.

## Goals and approach

- **Keep the first edition's structure**; replace the content and modernize the technology.
- **Preserve the author's voice**: first-person, conversational, opinionated about
  trade-offs, generous with admonitions (`!!! tip` / `!!! info` / `!!! warning`), emphatic
  about hard requirements (MUST / NOT), and warm at the seams ("Now go and make awesome
  apps!").
- **Target stack throughout:** .NET 10, ASP.NET Core + Community Datasync Toolkit server,
  EF Core, **.NET MAUI** as the primary client (the toolkit client is platform-agnostic, so
  WPF / Blazor / Avalonia / Uno / WinUI 3 get callouts), Microsoft Entra ID + MSAL, `azd` +
  Bicep, Application Insights / OpenTelemetry.
- **Fresh sample code in this repo**, replacing the old Xamarin/WebAPI `Chapter1..8`
  solutions. A progressively-built sample (Datasync server + MAUI client, plus a Blazor
  client and an Azure Functions project introduced in their chapters), organized as
  chapter-aligned snapshots so readers can jump in anywhere.

## Decisions taken so far

| Decision | Choice |
|---|---|
| Book scope | Keep breadth; modernize every chapter |
| Primary client | .NET MAUI (with platform-agnostic callouts) |
| Migration content | A dedicated front-matter section for v1 readers |
| Sample code | Author fresh samples in this repo |
| First-app PC/Mac | Unify into one walkthrough with OS callouts |
| Repo & title | Rewrite `docs/` on a `second-edition` branch; retitle |
| Chapter 3 | Split into Basics + Advanced; renumber later chapters |
| Cloud positioning | Cloud-neutral core; one hyperscaler as the worked example; add an "Other Clouds" comparison chapter |
| Cloudflare | Covered only as an *augmentation* to a chosen hyperscaler (edge/supporting services), **not** as a standalone host for the .NET server |

## Open questions (to iterate on)

- Is **breadth** still the right call, or should some later chapters (push, web, other
  services) be trimmed to keep the book maintainable and correct over time?
- **Chapter 6 (Real-time & Push):** the toolkit has no push. Is SignalR + push-to-sync the
  right center of gravity, with classic push (Notification Hubs / APNS / FCM) as secondary?
  Or should push move to an appendix?
- **Chapter 7 (Web & Other Clients):** lead with Blazor WASM only, or also show a
  JavaScript/TypeScript client hitting the OData endpoints directly?
- **Chapter 8 (Other Useful Services):** which services earn a place now? Candidates: Cosmos
  DB, Azure AI Search, Azure OpenAI / AI, Blob + CDN, App Configuration. Anything to add or
  cut? Media Services is retired and proposed for removal.
- **Sample layout:** chapter-aligned snapshots vs. one evolving solution with git tags.
- **Title** confirmation and **branch name** (`second-edition` proposed).
- **Screenshots:** all first-edition screenshots are obsolete. New ones must be captured by
  hand; the rewrite will mark every placeholder.

---

# Table of contents

**Front matter**

- Introduction (`index.md`) — including *"What's changed since the first edition"*
- Migrating from Azure Mobile Apps (un-numbered front section)

**Chapters**

1. Introduction — Your First App
2. Authentication
3. Data Access & Offline Sync: The Basics
4. Advanced Data Access & Offline Sync
5. Server-Side Code
6. Real-time & Push Notifications
7. Web & Other Clients
8. Other Useful Services
9. Developing an App
10. Going to Production
11. Other Clouds

**Appendices**

- .NET MAUI Tips
- Android Developer Notes
- Visual Studio / VS Code Extensions
- References
- Credits

---

# Chapter-by-chapter detail

## Front matter

### Introduction (`index.md`)

**Overview.** Rewrite of the first edition's welcome. Reset expectations for the modern
stack and re-establish who the book is for (intermediate-to-experienced C# developers who
want to connect apps to the cloud). Open with the new *"What's changed since the first
edition"* section so returning readers immediately understand the landscape shift.

**Topics to cover.**

- What are cloud-connected apps (refreshed; not just phones — desktop and web too).
- *What's changed since the first edition* — the Xamarin→MAUI, Azure Mobile Apps→Datasync
  Toolkit, .NET Framework→.NET 10, Easy Auth/ZUMO→ASP.NET Core+MSAL, ARM→azd/Bicep mapping.
- Why .NET MAUI for cross-platform; why the Community Datasync Toolkit.
- Who this book is for; what you should already know.
- What you'll need: hardware, software (VS 2022 / VS Code / Rider, .NET 10 SDK, MAUI
  workload), an Azure account, source control.

### Migrating from Azure Mobile Apps

**Overview.** Original, high-value content with no equivalent anywhere today. For readers of
the first edition (or anyone on Azure Mobile Apps) who needs to move to the Datasync
Toolkit. Emphasize that a new server remains protocol-compatible with Azure Mobile Apps v6
clients, so migration can be incremental.

**Topics to cover.**

- The big picture: what stayed the same (the wire protocol, system fields, offline
  push/pull) and what changed (server framework, client API, auth, push).
- Server migration: `Microsoft.Azure.Mobile.Server` (WebAPI/.NET Framework) → ASP.NET Core +
  `TableController<T>` + repositories.
- Client API mapping table: `MobileServiceClient` / `IMobileServiceSyncTable` /
  `IMobileServiceTable` → `DatasyncServiceClient<T>` / `OfflineDbContext` + `PushAsync` /
  `PullAsync`.
- Auth migration: Easy Auth / ZUMO tokens → MSAL + Entra ID (with `X-ZUMO-AUTH` and
  `X-ZUMO-INSTALLATION-ID` compatibility notes).
- What's gone and what replaces it: push (DIY / Notification Hubs), custom APIs (plain
  ASP.NET Core controllers), Domain Managers (custom `IRepository<T>` / AutoMapper mapped
  repository), file sync.
- A staged migration strategy (stand up the new server, point old clients at it, migrate
  clients).

## Chapter 1 — Introduction: Your First App

**Overview.** The "wouldn't it be nice to get something working" chapter. A single, unified
walkthrough (PC and Mac, with OS-specific admonitions) that builds an end-to-end app: a
Datasync server and a .NET MAUI client. Replaces the first edition's separate
`firstapp_pc.md` / `firstapp_mac.md`.

**Topics to cover.**

- Project layout and tooling for the modern stack.
- Build the backend: ASP.NET Core project, EF Core `DbContext`, an entity on
  `EntityTableData`, a `TableController<T>`, an initial migration, OpenAPI.
- Run it locally; provision and deploy to Azure with `azd up`.
- Build the MAUI client: shared model, `DatasyncServiceClient<T>`, online CRUD against the
  table endpoint.
- A first taste of offline sync to whet the appetite for Chapter 3.
- Some final thoughts / where we go next.

## Chapter 2 — Authentication

**Overview.** Modernized and slimmed relative to the first edition's nine-section deep dive.
The toolkit delegates auth to standard ASP.NET Core, so the chapter teaches the modern
patterns: JWT bearer on the server, MSAL on the client, and Entra ID as the default IdP.

**Topics to cover.**

- Concepts: OAuth 2.0 / OIDC actors, server-flow vs. client-flow, JWT bearer tokens.
- Server authentication: `Microsoft.Identity.Web` (`AddMicrosoftIdentityWebApi`),
  `[Authorize]` / `[AllowAnonymous]`.
- Enterprise authentication: Microsoft Entra ID app registrations and scopes.
- Social authentication: Google, Facebook, Apple (provider SDKs or ASP.NET Core).
- Custom / external auth: Entra External ID (formerly B2C), Auth0, OpenIddict.
- Claims and authorization: getting claims server-side; intro to per-user/per-group access
  (full treatment in Chapter 4).
- Tokens in real apps: MSAL caching and refresh, MAUI `WebAuthenticator`, `SecureStorage`,
  plugging tokens in via `GenericAuthenticationProvider`; `X-ZUMO-AUTH` compatibility.
- Best practices.

## Chapter 3 — Data Access & Offline Sync: The Basics

**Overview.** The heart of the book, part one. Everything you need to build a working
synchronized app with what's in the toolkit today, without the advanced extensibility.

**Topics to cover.**

- Concepts: structured data, the system fields (Id / UpdatedAt / Version / Deleted), OData
  v4 as the data-access protocol, conditional requests / ETags / optimistic concurrency, and
  what offline sync actually is (push, pull, the operations queue, delta-tokens).
- The datasync server: `TableController<T>`, the EF Core repository
  (`EntityTableRepository<T>`), Code-First migrations, `TableControllerOptions`, exposing
  OpenAPI (NSwag / Swashbuckle / native .NET 10).
- The online client: `DatasyncServiceClient<T>`, CRUD with `ServiceResponse<T>`, LINQ→OData
  queries (`Where` / `OrderBy` / `Skip` / `Take` / `Select`), paging with `IAsyncPageable`.
- The offline client: `OfflineDbContext`, `OnDatasyncInitialization`, `PushAsync` /
  `PullAsync`, the operations queue, and the built-in `ClientWins` / `ServerWins` conflict
  resolvers.

## Chapter 4 — Advanced Data Access & Offline Sync

**Overview.** The heart of the book, part two: extensibility and the harder problems. This is
also where readers who relied on the old Domain Manager and per-user filtering will find
their replacements.

**Topics to cover.**

- Repository patterns: the standard repositories (In-Memory, EF Core and its `ITableData`
  variants, LiteDb, MongoDB, Cosmos via EF Core *and* via the Cosmos SDK, AutoMapper
  `MappedTableRepository`) and how to choose.
- Custom repositories: implementing `IRepository<T>` for legacy / auto-increment schemas,
  mapping to MobileId / Version / Deleted, signalling errors with `HttpException`.
- Access control providers: `IAccessControlProvider<T>`, `GetDataView()`,
  `IsAuthorizedAsync()`, `PreCommitHookAsync` / `PostCommitHookAsync`; per-user, per-group,
  and "friends" data patterns (cross-referenced with Chapter 2).
- Advanced sync: custom `IConflictResolver<T>`, per-query pulls (`QueryId` +
  `PullRequestBuilder`), parallel push/pull, `SaveAfterEveryServiceRequest`, delta-token
  reset and management, and handling `PushResult.FailedRequests`.
- Relationships and the flat-entity constraint: why the toolkit syncs flat entities only,
  and the patterns to model related data.
- Spatial / geo applications: `GeographyPoint` + GeoJSON on the client, the `geo.distance`
  server filter, a "near me" query end-to-end, and the honest limitations (points only;
  database-provider support and SQLite precision caveats).

## Chapter 5 — Server-Side Code

**Overview.** Modernizes the first edition's "Server Side Code" chapter. Covers the code you
write *alongside* your table controllers, plus background processing.

**Topics to cover.**

- Options for server code: extra ASP.NET Core controllers and minimal APIs alongside table
  controllers; when to use background processing.
- Custom HTTP endpoints: plain ASP.NET Core controllers sharing the same app, database, and
  auth as the datasync server (replacing Azure Mobile Apps "Custom APIs").
- Background processing with Azure Functions (isolated worker): scheduled jobs, queue/blob
  triggers — replacing the old WebJobs material.
- Recipes: Blob storage with `Azure.Storage.Blobs`, SAS tokens, file upload/download from
  MAUI with progress, and webhooks / Event Grid.

## Chapter 6 — Real-time & Push Notifications

**Overview.** Reframes the first edition's push chapter. Because the toolkit does not include
push, the modern center of gravity is real-time via SignalR, with classic push notifications
as a "bring your own" topic.

**Topics to cover.**

- Real-time updates with SignalR: the server hub, the `RepositoryUpdated` event /
  `PostCommitHookAsync`, broadcasting changes; a JS and a MAUI consumer.
- Push-to-sync: using a silent notification to trigger an offline pull.
- Classic push notifications (bring your own): Azure Notification Hubs with **FCM HTTP v1**
  (Android), **APNS token auth (.p8)** (iOS), WNS notes, wired from MAUI.
- Tags, templates, and targeting; testing and common problems.

## Chapter 7 — Web & Other Clients

**Overview.** Reframes the first edition's "Combining Web and Mobile" chapter, which covered
AngularJS / jQuery / MVC 5. The modern story is that the same backend serves a Blazor web
client and other .NET clients.

**Topics to cover.**

- A Blazor WebAssembly client against the same datasync backend (online-only — and why
  offline isn't supported in the browser).
- Sharing one backend across mobile and web (shared models, shared auth).
- Accessing the OData endpoints from JavaScript/TypeScript (optional).
- A note on the other supported clients (WPF, Avalonia, Uno, WinUI 3) and the
  platform-agnostic client.

## Chapter 8 — Other Useful Services

**Overview.** Modernizes the first edition's "Other Useful Services" survey. Drops retired
services (Media Services) and refreshes the rest.

**Topics to cover.**

- A survey of complementary Azure services worth knowing.
- Cosmos DB (formerly DocumentDB) for global-scale data.
- Azure AI Search (formerly Azure Search): indexing your data and searching from the app.
- Azure OpenAI / AI features as a modern addition.
- Blob storage + CDN, and Azure App Configuration.
- *(Removed: Azure Media Services — retired.)*

## Chapter 9 — Developing an App

**Overview.** Modernizes the development-environment and testing chapter.

**Topics to cover.**

- The development environment: running the server locally, SQLite / LocalDB, `user-secrets`,
  and **dev tunnels** for testing against physical devices.
- Handling cloud-only services while developing locally.
- Testing the backend: xUnit + `WebApplicationFactory` integration tests.
- Testing the client: mock `DelegatingHandler`s and an in-memory server.
- UI testing MAUI apps with Appium.
- Distributing to beta users now that App Center is retiring (TestFlight, Play internal
  testing, alternatives).

## Chapter 10 — Going to Production

**Overview.** Modernizes the "Going to Production" chapter: repeatable, safe deployments and
monitoring.

**Topics to cover.**

- Repeatable deployments with the Azure Developer CLI (`azd`) and Bicep (replacing ARM
  templates).
- Safe deployments: deployment slots and CI/CD with GitHub Actions.
- Scaling the backend.
- Monitoring and troubleshooting: Application Insights and OpenTelemetry for the backend;
  client-side telemetry options.
- Closing thoughts.

## Chapter 11 — Other Clouds

**Overview.** The main narrative deploys to one hyperscaler so every step stays concrete,
but the Datasync Toolkit is *just* an ASP.NET Core app plus a database — so it runs anywhere
.NET runs. This chapter steps back and looks at the wider landscape: a **qualitative
comparison** of the equivalent services on the major clouds, and an honest look at **where
Cloudflare can augment** a chosen hyperscaler. It is a survey/decision chapter, not a second
set of step-by-step deployment tutorials — the goal is to help readers map what they learned
onto the cloud they actually use, and to know what to reach for.

This chapter deliberately does **not** present any cloud as a drop-in replacement for the
main narrative. In particular, Cloudflare is framed as an *augmentation layer* in front of /
alongside a chosen hyperscaler, **not** as a standalone host for the .NET datasync server
(see the candid assessment below).

**The key framing — only the infrastructure half is cloud-specific.** The MAUI / Datasync
*client* is 100% cloud-agnostic; it speaks HTTP/OData to an endpoint and does not care who
hosts it. So nothing in the client or sync chapters changes when you change clouds. What
changes is a small set of server-side capability "slots," and two of those (identity and
observability) already have cloud-neutral spines (any OIDC provider; OpenTelemetry).

**Topics to cover.**

- *Why this is even possible* — the toolkit decomposed into capability slots: compute,
  relational database, identity/JWT, object storage, background/serverless, real-time, push,
  search, infrastructure-as-code, and observability.
- *A qualitative capability comparison* across Azure, AWS, and GCP (with Cloudflare noted as
  an augmentation), presented as a single reference table plus prose on the meaningful
  differences:

  | Capability | Azure | AWS | GCP | Cloudflare (augment) | Neutral spine |
  |---|---|---|---|---|---|
  | Compute (host ASP.NET Core) | App Service / Container Apps | App Runner / ECS Fargate / Lambda (container) | Cloud Run | Containers (Worker + DO) ⚠️ | a Linux container |
  | Relational DB (EF Core) | Azure SQL / PostgreSQL | RDS / Aurora | Cloud SQL / AlloyDB | external PG via Hyperdrive ⚠️ (D1 ✗ for EF) | PostgreSQL |
  | Identity / JWT | Entra ID / External ID | Cognito | Identity Platform / Firebase | Access | any OIDC (Auth0, Okta, OpenIddict) |
  | Object storage | Blob | S3 | GCS | R2 (S3-compatible) | S3 API (`AWSSDK.S3`) |
  | Background / serverless | Functions | Lambda | Cloud Functions / Run Jobs | Workers + Queues + Cron | — |
  | Real-time | Azure SignalR | API Gateway WS / AppSync | Pub/Sub + Firestore | Durable Objects + WS | self-hosted SignalR |
  | Push | Notification Hubs | SNS | FCM | — | FCM (cross-platform) |
  | Search | Azure AI Search | OpenSearch / Kendra | Vertex AI Search | Vectorize + Workers AI | Typesense / Meilisearch |
  | Infrastructure-as-code | `azd` + Bicep | CDK / SAM / CloudFormation | gcloud + Terraform | Wrangler + Terraform | Terraform / Pulumi |
  | Observability | Application Insights | CloudWatch + X-Ray | Cloud Monitoring | Workers Logs / Analytics | OpenTelemetry |

- *Portability notes* — what carries over unchanged (the client, EF Core against any managed
  relational database, OIDC auth, OpenTelemetry) and what is genuinely provider-specific
  (IaC, the managed real-time/push/search services).
- *Where Cloudflare augments your hyperscaler* — used **in front of / alongside** a server
  hosted on Azure/AWS/GCP:
  - **R2** — S3-compatible object storage that works with the .NET `AWSSDK.S3`; a single
    code path can target S3, GCS, and R2.
  - **Cloudflare Access** — JWT-based access control in front of the server (validated in
    ASP.NET Core).
  - **Workers + Queues + Cron Triggers** — edge background jobs and webhooks.
  - **Durable Objects + WebSockets** — a strong real-time fit that pairs well with the
    push-to-sync pattern from Chapter 6.
  - **Vectorize + Workers AI**, plus CDN / cache / WAF in front of any origin.
- *The candid assessment — why we don't host the .NET server on Cloudflare.* Two concrete
  reasons: (1) .NET can only run as a **Container** (a Worker fronting a Durable
  Object-managed Linux VM) which cold-starts, sleeps after inactivity, is capped by
  `max_instances`, and has ephemeral disk — you lose the edge/instant-scale value prop; and
  (2) Cloudflare's native database, **D1 (SQLite over HTTP), is not compatible with EF
  Core's SQLite provider**, so the server would still depend on an *external* managed
  Postgres/MySQL (directly or pooled via Hyperdrive). A Workers-native TypeScript
  reimplementation of the datasync protocol on D1 is theoretically possible but is **not the
  .NET toolkit**, so it is out of scope (one honest paragraph).
- *Choosing a cloud* — a short, opinionated decision guide: pick the cloud you already
  operate in; the toolkit will not be the thing that locks you in.

### Other cloud services that are interesting in mobile development

**Overview.** Not every service a mobile developer reaches for belongs to "their"
hyperscaler. Some of the most useful ones are vendor-neutral, cross-cloud, or simply
best-in-class regardless of where the datasync server is hosted. This section surveys those
services so readers know what exists and when to reach for it — keeping the same "augment
your chosen cloud" framing rather than presenting parallel ecosystems. Each gets a short,
opinionated "what it is / when to use it / how it plugs into a MAUI + Datasync app" treatment
rather than a full tutorial.

**Topics to cover.**

- *Push & messaging:*
  - **Firebase Cloud Messaging (FCM)** — the de-facto cross-platform push transport
    (Android *and*, via APNs, iOS). Works no matter which cloud hosts the server; pairs with
    the push-to-sync pattern from Chapter 6. Note its relationship to APNs and to Notification
    Hubs / SNS (which front FCM rather than replace it).
  - **Other Firebase bits worth knowing** — Remote Config (feature flags / kill switches),
    A/B testing, Dynamic Links successors / deep linking, and how they coexist with a
    non-Google backend.
- *Identity as a service:*
  - **Auth0** (and peers: **Okta**, **Clerk**, **Stytch**, self-hosted **OpenIddict** /
    **Keycloak**) — drop-in OIDC providers that work with the toolkit's "bring your own JWT"
    model exactly like Entra ID, with no server lock-in. When a managed IdP beats the
    hyperscaler's native one (social login breadth, B2C/CIAM flows, passwordless, multi-tenant).
- *Analytics, monitoring & product telemetry:*
  - **Google Analytics for Firebase / Google Tag Manager** — mobile-oriented product
    analytics and event funnels; how they differ from infrastructure observability (App
    Insights / CloudWatch / Cloud Monitoring) and why you often want both.
  - **Cross-platform product analytics** — Mixpanel, Amplitude, PostHog (incl. self-hosted)
    for funnels, retention, and feature usage.
  - **Crash & performance monitoring** — Sentry, Firebase Crashlytics, and the
    OpenTelemetry-based options — especially relevant now that App Center is retiring (ties
    back to Chapter 9's distribution/telemetry discussion).
- *Supporting services frequently paired with mobile apps:*
  - **Search** — Algolia / Typesense / Meilisearch as managed, cloud-neutral search that
    complements (or replaces) a hyperscaler's search service.
  - **Feature flags & config** — LaunchDarkly, ConfigCat (and Firebase Remote Config above).
  - **Maps & geo** — Google Maps / Mapbox, tying back to the spatial/geo work in Chapter 4.
  - **Payments & in-app purchase** — Stripe and the App Store / Play billing realities.
- *How to choose* — a short rubric: prefer the vendor-neutral option when it removes lock-in
  or is materially better at the job; prefer the hyperscaler-native option when tight
  integration (identity, networking, billing) outweighs portability.

## Appendices

- **.NET MAUI Tips** — the refreshed successor to "Xamarin Forms Tips": performance,
  platform-conditional UI, common patterns.
- **Android Developer Notes** — modern emulator and tooling notes.
- **Visual Studio / VS Code Extensions** — a refreshed recommended-extensions list.
- **References** — fill in the first edition's empty stub (further reading, API references,
  samples, how to get help).
- **Credits** — updated acknowledgements.

---

# Sample code plan

- Replace the top-level `Chapter1..8` / `Chapter7M` Xamarin + WebAPI solutions with fresh
  samples under `samples/`.
- A progressively-built app mirroring the book: a Datasync **server** (`azd`-deployable,
  EF Core / Azure SQL), a **.NET MAUI** client, plus a **Blazor** client and an **Azure
  Functions** project introduced in their respective chapters.
- Proposed layout: chapter-aligned snapshots (e.g., `samples/chapter1`, `samples/chapter3`,
  …) so readers can start at any chapter — preserving the first edition's "code per chapter"
  affordance. (Alternative: one evolving solution with git tags per chapter.)

# Production / tooling notes

- Keep MkDocs + `mkdocs-material`.
- Update `mkdocs.yml`: new `site_name` / title, copyright year, repo links, drop the dead
  Twitter link.
- Add an internal `STYLE.md` (not published) capturing the voice rules and the recurring
  sample-app spec so every chapter stays consistent.
- Update `README.md` build instructions and intro.
- Plan a dedicated screenshot pass; mark every placeholder during the rewrite.
