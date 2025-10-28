# FUNewsManagementSystem v2 (FUMS v2)  

**Distributed and Intelligent News Management System**  



---



## Table of Contents

1. [Introduction](#introduction)  

2. [Objectives / Key Features](#objectives--key-features)  

3. [Project Components](#project-components)  

&nbsp;  - [Core API](#core-api)  

&nbsp;  - [Analytics API](#analytics-api)  

&nbsp;  - [AI API](#ai-api)  

&nbsp;  - [Frontend (MVC / Razor Pages)](#frontend-mvc--razor-pages)  

4. [Screen-Level Requirements](#screen-level-requirements)  

5. [UX & Functional Requirements](#ux--functional-requirements)  

6. [Configuration & Running](#configuration--running)  

7. [Test Accounts](#test-accounts)  



---



## Introduction

FUMS v2 is a distributed news management system designed for universities and educational institutions.  

The system includes **four independent components**:  



- **Core API** – manages accounts, news articles, categories, and tags.  

- **Analytics API** – dashboard, statistics, related article recommendations, Excel export.  

- **AI API** – automatically suggests tags based on article content.  

- **Frontend (MVC / Razor Pages)** – user interface, CRUD operations, dashboard, offline mode.  



The system demonstrates distributed architecture, HttpClient communication, JWT authentication, background processing, caching, AI-based tag suggestions, and interactive dashboards with Chart.js.  



---



## Objectives / Key Features



- Built with **.NET 8**, **EF Core**, **ASP.NET Core Web API**, **Razor Pages / MVC**  

- Distributed system design with four projects: Core API, Analytics API, AI API, and Frontend  

- **HttpClientFactory** & **DelegatingHandler** for API communication  

- **Background Worker (HostedService)** to refresh cached data periodically  

- AI-powered tag suggestions with learning cache  

- **JWT + Refresh Token** to automatically renew access tokens  

- **Audit Logging** for all CRUD actions: User, Action, Entity, Before/After JSON  

- **SignalR Notification** system for new article alerts  

- **Excel Export** from the dashboard  

- Responsive UI, Bootstrap Modals, Chart.js dashboards, Offline Mode  



---



## Project Components



### Core API

- CRUD operations for **Account, Category, Tag, NewsArticle** (EF Core)  

- JWT Authentication + Refresh Token (`/api/auth/refresh`)  

- Image upload with file type and size validation  

- Audit logging for all user actions  

- Business rules: prevent deletion of related records  

- Standardized JSON responses and HTTP status codes  



### Analytics API

- `/api/analytics/dashboard` – total articles by category and status  

- `/api/analytics/trending` – trending articles  

- `/api/recommend/{id}` – related article recommendations  

- Advanced filtering: date, category, author  

- Excel export: `/api/analytics/export`  



### AI API

- `/api/ai/suggest-tags` – suggest tags from article content  

- Accepts JSON: `{ "content": "..." }`  

- Can connect to OpenAI or simulate keyword extraction  

- Maintains learning cache for frequently selected tags  



### Frontend (MVC / Razor Pages)

- Communicates with APIs **only via HttpClient**, no direct DB access  

- Multiple HttpClient instances for Core, Analytics, and AI APIs  

- **Polly Retry Policy** and error alerts for API failures  

- Background Worker refreshes cache every 6 hours  

- Dashboard with Chart.js, CRUD via Bootstrap Modals  

- **Offline Mode**: reads cached JSON, disables CRUD when API is down  

- SignalR Notifications for live updates  



---



## Screen-Level Requirements  


| Page / Feature | API Endpoint | Description | Notes |
|----------------|--------------|-------------|-------|
| Login | `/api/auth/login` | User authentication | Store access_token & refresh_token, show Toast on failure, auto-refresh JWT |
| Dashboard (Admin) | `/api/analytics/dashboard` | Display article statistics | Chart.js Pie/Bar, filter by date/category/status, Excel export |
| News List (Staff) | `/api/news` | List articles | Pagination, search, sort by creation date, Active/Inactive color |
| Create/Edit News | `/api/news`, `/api/news/{id}` | Add or update articles | Bootstrap Modal, validate fields, AI tag suggestions |
| News Detail | `/api/news/{id}`, `/api/recommend/{id}` | Article details | Show up to 3 related articles, responsive reading layout |
| Category Management | `/api/category` | Manage categories | Prevent deletion if articles exist, toggle IsActive, show article count |
| Tag Management | `/api/tag` | Manage tags | Prevent duplicate names, search, show articles per tag |
| Account Management | `/api/account` | Manage accounts | Cannot delete accounts with created articles, require old password for changes, filter by role |
| AI Tag Suggestion | `/api/ai/suggest-tags` | Suggest tags for content | Display as chips/badges, allow quick selection |
| Notification Center | `/hubs/notifications` | Live notifications | Toast or 🔔 icon, keep last 10 notifications |
| Offline Mode | - | When API is unreachable | Banner “Offline Mode”, show cached data, disable CRUD |
| Audit Log | `/api/auditlog` | Track changes | Show User, Action, Entity, Timestamp, Before/After JSON, filterable |


**General Notes:**  

- Confirmation prompt for all CRUD actions  

- Loading indicator for all API calls  

- Responsive and consistent UI/UX  

- All data retrieval **via HttpClient**, no direct DB access  



---



## UX & Functional Requirements  



- Loading indicator for all API operations  

- Toast/Alert for success or failure  

- Responsive design for desktop and mobile  

- Automatic JWT refresh before expiration  

- Log all API requests: endpoint, time, status  

- Accessibility: keyboard navigation, high color contrast  

- Performance: API response < 1 second, optimized caching  



---



## Configuration & Running  



### Backend  

- Open solutions: `FUNewsManagement_CoreAPI.sln`, `FUNewsManagement_AnalyticsAPI.sln`, `FUNewsManagement_AIAPI.sln`  

- Configure `appsettings.json` for database connection  

- Run EF migrations and seed database if needed  



### Frontend  

- Open solution: `FUNewsManagement_FE.sln`  

- Configure API base URLs in `appsettings.json`  

- Run project and login with test accounts  



---



## Test Accounts  



| Role  | Email | Password |
|-------|-------|----------|
| Admin | admin@FUNewsManagement.org | @@abc123@@ |
| Staff | staff@FUNewsManagement.org | @1 |
| Lecturer | lecturer@FUNewsManagement.org | @1 |



