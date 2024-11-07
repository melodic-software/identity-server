# Introduction

IdentityServer acts as our central identity provider, authorization server, and security token service
ensuring secure authentication and authorization for our applications. 

It was created by security experts [Dominick Baier](https://duendesoftware.com/company) and [Brock Allen](https://duendesoftware.com/company) 
and is certified by the [OpenID Foundation](https://openid.net/developers/certified/). 
IdentityServer is a powerful and flexible open source framework that allows us to implement security best practices in our identity and access management. 
It is particularly well-suited for ASP.NET Core applications, but can be integrated into other technologies as well.

For more information on IdentityServer, you can visit the [Duende Software GitHub page](https://github.com/duendesoftware) 
or the [Duende IdentityServer product page](https://duendesoftware.com/products/identityserver).

IdentityServer implements the OpenID Connect and OAuth 2.0 protocols, 
which are the most widely used standards for modern identity and authorization solutions. 
If you're new to these standards, you can review the latest specifications:

- [OAuth 2.0 Authorization Framework (RFC 6749)](https://datatracker.ietf.org/doc/html/rfc6749)
- [OpenID Connect Core 1.0 incorporating errata set 1](https://openid.net/specs/openid-connect-core-1_0.html)

It's the responsibility of an identity provider such as this to authenticate the user, and if needed, safely provide proof of identity to an application.

IdentityServer also currently acts as our IAM (Identity and Access Management) system, and implements:

- User registration
- Account management
- Password policies, strengths, and resets
- User lockouts
- MFA

Note that some of these concerns may be delegated to external providers, or may only apply to local user accounts.
The users here are application agnostic, which allows for a centralized representation of a user in the system.
The external identity providers can be used for automatic or manual account provisioning, 
and additional external logins can be linked.

## Application Structure and Setup

The application is based on a hybrid combination of the IdentityServer template, customized to meet our specific needs. 
It is built as a Razor Pages app, providing a modular and maintainable structure. 
We are using Entity Framework Core and SQL Server for both operational and configuration stores, which helps in managing tokens, consents, 
and other configurations in a robust and scalable manner.

For managing user accounts, passwords, and email confirmations, we are utilizing ASP.NET Identity. 
This integration provides a seamless experience for user management within our system. 
Multi-Factor Authentication (MFA) for local accounts is implemented with Authsignal instead of the built-in MFA provided by ASP.NET Identity.
We went this route to take advantage of enhanced security features like Passkeys.

# Project Setup Guide

## Prerequisites

Ensure you have Docker, and Docker Desktop installed.

## Startup Projects

Set `docker-compose.csproj` as the startup project.

## Application URLs:

* Identity Server: [https://localhost:5001](https://localhost:5001)
* Redis UI: [https://localhost:8001](https://localhost:8001)
* RabbitMQ Admin UI: [https://localhost:15672](https://localhost:15672)
* Jaeger UI: [https://localhost:16686](https://localhost:16686)
* Prometheus UI: [https://localhost:9090](https://localhost:9090)
* Grafana UI: [https://localhost:3000](https://localhost:3000)
* Seq UI: [https://localhost:8180](https://localhost:8180)

For the full list of URIs, review the `docker-compose.yml` file. 
You will find other relevant connection strings and configuration in the `appsettings.Development.Docker.json`.

## Required Configuration Keys and Secrets

### User Secrets
For local development, you need to set the following user secrets:

- `Authentication:Google:ClientSecret`
- `Authentication:Microsoft:ClientSecret`
- `Authentication:Spotify:ClientSecret`
- `Authsignal:ManagementApiSecret`
- `Authsignal:Secret`
- `Microsoft365Settings:ClientSecret`
- `Security:Authentication:OAuth2Introspection:ClientSecret`

### Example Values
The actual values are stored in Azure Key Vault.
Reach out to a team member for assistance or to request access to an environment-specific key vault.

# Terminology / Definitions

## Identity Management

**Identity and Access Management (IAM)**:  
A framework of policies, processes, and technologies that ensures the right individuals have access to the appropriate resources at the right times for the right reasons.

**Identity Provider (IdP)**:  
A system or service that performs authentication and issues security tokens containing user identity and other related information.

**Security Token Service (STS)**:  
A service that issues security tokens, such as identity tokens, access tokens, and refresh tokens. It is often part of the Authorization Server.

**Identity Claims**:  
Specific claims that relate to the user's identity, such as username, email address, and user ID. These are often included in identity tokens.

**Identity Resources**:  
Resources that represent identity-related information such as the user's profile, email, and roles. These are often protected by scopes in OpenID Connect.

## Authorization and Tokens

**Authorization Server**:  
A server responsible for authenticating users and issuing access tokens. It plays a central role in managing authorization within the OAuth 2.0 and OpenID Connect frameworks.

**Authorization**:  
The process of determining whether a user or system has the necessary permissions to access a resource or perform an action, typically governed by policies and roles.

**OAuth 2.0**:  
An open standard for authorization that allows third-party applications to access resources on behalf of a user by using tokens instead of sharing credentials.

**OpenID Connect (OIDC)**:  
An authentication layer on top of OAuth 2.0 that allows clients to verify the identity of an end-user based on the authentication performed by an Authorization Server, and to obtain basic profile information about the user.

**Scopes (Standardized and Custom)**:  
Scopes define the access privileges (permissions) associated with the tokens. Standardized scopes include common permissions like `openid`, `profile`, `email`, etc. Custom scopes can be defined to specify access to specific resources or actions within an API.

**Clients / Relying Party**:  
Applications or systems that request tokens from the Authorization Server to access resources on behalf of the user. Clients can be either public (unable to secure credentials) or confidential (capable of securing credentials).

**Public and Confidential Clients**:  
- **Public Clients**: Clients that cannot securely store credentials, such as single-page applications or mobile apps.  
- **Confidential Clients**: Clients that can securely store credentials, typically backend servers or applications.

**ID Token vs Access Token**:  
- **ID Token**: A token that contains information about the user's authentication, including claims such as the user's identity, issued by the Authorization Server after successful authentication. The ID token is intended for the client and is not meant for accessing resources.
- **Access Token**: A token that grants the bearer access to specific resources or APIs. It contains claims that specify the granted permissions and is used by the client to authenticate to resource servers.

**JWT (JSON Web Token)**:  
A compact, URL-safe means of representing claims to be transferred between two parties. JWTs are often used as access tokens in OAuth 2.0 and OpenID Connect. JWTs contain three parts: a header, a payload, and a signature.

- **amr (Authentication Methods Reference)**: A claim in a JWT that indicates the methods used to authenticate the user (e.g., password, OTP). This claim provides additional context on how the authentication was performed.
- **nonce**: A claim included in a JWT to mitigate replay attacks. It is a unique value used to associate a client session with an ID token, ensuring the token cannot be reused by an attacker.
- **at_hash**: A claim that provides a hash of the access token. It is used to verify that the access token returned to the client is the one actually issued by the Authorization Server.

**Reference Tokens**:  
Tokens that are not self-contained (like JWTs) but instead reference an internal record on the Authorization Server. 
The server must be contacted to validate these tokens.

**Token Introspection Endpoint**:  
An endpoint provided by the Authorization Server that allows clients or resource servers to query the status and metadata of a given token 
(e.g., whether it is active or expired). This is particularly useful for validating reference tokens.

## OpenID Connect Flows and Endpoints

**Authorization Code Flow**:  
A secure flow where the client receives an authorization code, which is then exchanged for an access token. Primarily used by confidential clients and requires client authentication.

**Implicit Flow**:  
A flow designed for public clients where tokens are returned directly in the redirect URI. This flow is less secure and is being deprecated in favor of more secure options like Authorization Code + PKCE.

**Hybrid Flow**:  
A combination of the Authorization Code Flow and Implicit Flow, where some tokens are returned directly and others are exchanged using the authorization code.

**Client Credentials Flow**:  
A flow where the client uses its own credentials to obtain an access token. Commonly used for machine-to-machine communication, where no user is involved.

**Resource Owner Password Credentials Flow**:  
A flow where the user provides their username and password directly to the client, which then exchanges them for tokens. This flow is generally not recommended due to security risks, such as exposing user credentials to clients.

**Authorization Code + PKCE**:  
An extension of the Authorization Code Flow that adds security by requiring the client to use a dynamically generated code challenge and verifier. PKCE (Proof Key for Code Exchange) is especially recommended for public clients like single-page applications.

**Device Authorization Flow**:  
A flow designed for devices with limited input capabilities (e.g., smart TVs) where the user authorizes the device using another device (e.g., a smartphone).

**Authorization Endpoint**:  
The endpoint on the Authorization Server where the client directs the user to authenticate and grant access. This is typically the starting point of the OAuth 2.0 and OpenID Connect flows.

**Token Endpoint**:  
The endpoint on the Authorization Server where the client exchanges an authorization code, refresh token, or client credentials for an access token.

**Redirection Endpoint**:  
The endpoint on the client (often referred to as the "redirect URI") where the Authorization Server redirects the user after completing the authentication process, typically passing back tokens or authorization codes.

**Discovery Document**:  
An endpoint that returns configuration information about the OpenID Connect provider in JSON format. This is typically found at `/.well-known/openid-configuration`.

## Advanced Concepts

**CIBA (Client Initiated Backchannel Authentication)**:  
A decoupled flow where the authentication request is initiated by the client but the user authentication happens on another device or system. This flow is useful in scenarios where the user may not be directly interacting with the client.

**Backend For Frontend (BFF) Pattern**:  
A design pattern where a backend server acts as an intermediary between the frontend and other services, handling the security-sensitive logic and data, thus improving security by reducing exposure on the client side.

**Nonce**:  
A unique value included in authentication requests to mitigate replay attacks. The nonce is typically used in OpenID Connect flows to ensure the token response is unique and has not been reused.

## Communication Types

**Front Channel Communication**:  
Communication that happens through the user agent (e.g., browser), typically less secure due to exposure to the user and the possibility of interception.

**Backchannel Communication**:  
Direct communication between servers or services, often more secure as it avoids exposure through the user's device or browser.

# Password Hashing

We have implemented a mechanism for transparent / silent upgrades in password hashing.
As new developments are made, we should be able to implement and roll forward as needed.
The implementation does not hash previous passwords, so the password shucking risk is mitigated.

## Hashing Algorithms

Here are some sample hashes for the hashers we have implemented.

## Argon2

$argon2id$v=19$m=19456,t=2,p=1$/UY4jIurnLfi1ZuiSBj9Iw$WJ7hopNi1oTFlpGE7WExEgr+gWXpFklMmKEJF6qAO98

## BCrypt

$2a$13$Gte27eoBXxZ0fGsaQuS1A.rp8//mZeHMCJ8dfIdkcEy.0qFRvyYm.

## PBKDF

This is what ASP.NET Identity uses out the box.

AQAAAAIACSfAAAAAEOwShUBMdpILU0VCN2FTBSX20jwNVSrkOYMsHG7T7Ax3fAyKQozu0BPSDZEUxYQImg==

# Other References

- [Securing ASP.NET Core with OAuth2 and OpenID Connect](https://app.pluralsight.com/library/courses/asp-dot-net-core-6-securing-oauth-2-openid-connect/table-of-contents)
- [OAuth 2.0 Security Best Current Practice](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics)

# TODO

- Review and implement all NIST guidelines.
- Require MFA challenge on any password resets, password changes.
- Ensure notifications go out when password resets, password changes occur.
- Apply password blacklisting when users sign up with a password, or change their password. 
  This will ensure we align with NIST guidelines that recommend screening passwords during "initial enrollment and when passwords are changed or reset" (NIST SP 800-63B, Section 5.1.1.2).
- Add any other related RFCs.
- Add the vocabulary definitions in proper groups. 
  These include factors of authentication (username and password, biometrics, smartphone or hardware token, transaction).
- Setup home realm discovery before deploying any client applications.
- Document the following security concerns and how we mitigate them:

  - CSRF (Cross Site Request Forgery)
  - Open redirect attacks
  - Token replay attacks
  - Session hijacking and fixation
  - Cross-Site Scripting (XSS) - Content Security Policies (CSP)
  - Auth code interception (use of PKCE)

- Authorization Policy Requirements and Handlers. https://app.pluralsight.com/ilx/video-courses/77f2d072-e8ab-4806-9310-dcc770bc1ce0/25249d6d-40d4-4f01-a39a-c0d48ef17057/ba933410-1932-43fe-bb7d-068ed7d0407a