# 🏗️ StackOverFlowClone

A simplified, scalable ASP.NET Core Web API imitating key features of StackOverflow:
- Questions & answers
- Voting, reputation
- Tags, bookmarks
- JWT authentication & roles (User / Moderator / Admin)
- Exception & request‑logging middleware
- SignalR real‑time notifications & group chat
- Background jobs with Hangfire (clean expired refresh tokens)
- Caching with IMemoryCache
- Rate limiting
- Health checks
- File uploads (images)
- Mentions (@username) system
- Integrate Chatgpt and Gemini 
---

## 📂 Table of Contents

1. [Demo / Screenshots](#-demo--screenshots)  
2. [Features](#-features)  
3. [Getting Started](#-getting-started)  
   - [Prerequisites](#prerequisites)  
   - [Setup & Run](#setup--run)  
4. [Configuration](#-configuration)  
5. [Project Structure](#-project-structure)  
6. [Authentication & Authorization](#-authentication--authorization)  
7. [Background Jobs (Hangfire)](#-background-jobs-hangfire)  
8. [Caching](#-caching)  
9. [SignalR Notifications & Chat](#-signalr-notifications--chat)  
10. [File Uploads](#-file-uploads)  
11. [Health Checks](#-health-checks)  
12. [Rate Limiting](#-rate-limiting)  
13. [How to Contribute](#-how-to-contribute)  
14. [License](#-license)  

---

## 💻 Demo / Screenshots

*(Add screenshots of Swagger UI, SignalR chat in action, file upload response, Hangfire dashboard, etc.)*

---

## ✨ Features

| Category                          | Status       | Notes                                           |
|-----------------------------------|--------------|-------------------------------------------------|
| ✅ JWT Authentication             | Complete     | Login, register, refresh tokens                 |
| ✅ Roles: User / Moderator / Admin | Complete     | Role‑based and permission‑based policies        |
| ✅ Questions & Answers             | Complete     | CRUD + pagination + filtering + sorting         |
| ✅ Voting & Reputation             | Complete     | Upvote/downvote questions & answers             |
| ✅ Tags (Many‑to‑Many)             | Complete     | Tag creation & assignment                       |
| ✅ Bookmarks                       | Complete     | Bookmark/unbookmark questions                   |
| ✅ Exception Middleware            | Complete     | Custom JSON error responses + Serilog logging   |
| ✅ Request Logging Middleware      | Complete     | Serilog file & console sinks                    |
| ✅ Caching                         | Complete  | IMemoryCache for top‑popular questions          |
| ✅ System Notifications            | Complete  | SignalR + DB store for mention / comment notifications |
| 🔲 Group Chat                      | Planned      | SignalR hub for global & private chats          |
| 🔲 Background Jobs (Hangfire)      | Partial      | Clean expired refresh tokens hourly             |
| ✅ Health Checks                   | Complete     | `/health` endpoint                              |
| ✅ Rate Limiting                   | Complete     | Fixed‑window rate limiting (100 req/min)        |
| ✅ File Uploads (Images)           | Complete     | `/api/upload/image` → stores under `wwwroot/uploads` |
| ✅ Mentions (@username) System     | Complete     | Detect mentions in comments & send notifications|

Legend: ✅ Done 🟨 Partial/In Progress 🔲 Planned

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)  
- SQL Server (Express / LocalDB / Azure)  
- [Node.js](https://nodejs.org/) (if you have any front‑end)  
- (Optional) [Hangfire Dashboard](https://www.hangfire.io/)  

### Setup & Run

1. **Clone the repo**  
   ```bash
   git clone https://github.com/your-username/StackOverFlowClone.git
   cd StackOverFlowClone
