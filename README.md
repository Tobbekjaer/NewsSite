# NewsSite Authorization - Secure Software Development (PoC)

## Introduction

This project is my proof-of-concept solution for the **Secure Software Development** assignment focused on authorization and access control.

The main goal of this assignment was to go beyond simple authentication and basic role checks, and instead implement **fine-grained authorization** following the principle of least privilege.

The system models a simple news site with articles and comments, where different user roles have different levels of access.

---

## Tech Stack

- .NET 8 Web API  
- ASP.NET Core Identity  
- JWT Bearer authentication  
- EF Core with SQLite  
- Swagger for testing  

The solution follows a simplified Clean Architecture structure with separate projects for Domain, Infrastructure, and API.

---

## Authorization Model

The following policy is implemented:

### Guest
- Can read articles  
- Can read comments  

### Subscriber
- Can create comments  
- Can update/delete their own comments  

### Writer
- Can create articles  
- Can update/delete their own articles  
- Can delete comments on their own articles  

### Editor
- Can update/delete any article  
- Can update/delete any comment  

I use:
- Role-based authorization for coarse access control  
- Resource-based authorization handlers (`IAuthorizationService`) for ownership rules  

This ensures that access is not only based on role, but also on *who owns the resource*.

---

## How to Run the Project

### Requirements
- .NET 8 SDK

### Run locally
```bash
dotnet restore
dotnet build
dotnet run --project NewsSite.Api
```

## Open Swagger
Open Swagger in your browser:

[http://localhost:5038/swagger](http://localhost:5038/swagger)

---

## Test Users (Seeded in Development)

| Email                  | Role        |
|------------------------|------------|
| subscriber@test.com    | Subscriber |
| writer@test.com        | Writer     |
| editor@test.com        | Editor     |

All users use the password:
```bash
Password1!
```

---

## How to Test with JWT

1. Call `POST /auth/login`
2. Copy the returned token
3. Click **Authorize** in Swagger
4. Enter:

```bash
Bearer {your_token}
```


You can now test the authorization rules.

---

## Notes

The focus of this project is **authorization design**, not advanced authentication or UI.

I would appreciate feedback on:

- My use of resource-based authorization handlers  
- Separation of concerns in relation to Clean Architecture  
- Whether the authorization model is implemented in a clear and secure way  
- Any edge cases I may have missed  

Thank you for reviewing my solution.


