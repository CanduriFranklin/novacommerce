# 05: Authentication and Authorization Strategy

This document details the authentication and authorization mechanisms implemented across the NovaCommerce microservices, focusing on JWT-based security, role-based access control, and the refresh token mechanism for enhanced session management.

## Overview

Authentication is centralized within the dedicated `Identity` microservice, which issues JSON Web Tokens (JWTs) upon successful user login. These JWTs are then used by client applications to access protected resources in the `inventory` and `sales` microservices. Authorization is implemented using role-based access control (RBAC), where user roles are embedded in the JWT claims.

## Key Components

### 1. Identity Microservice

The `Identity` service is responsible for:

*   **User Registration**: Allows new users to create accounts.
*   **User Login**: Authenticates users and issues JWTs and Refresh Tokens.
*   **JWT Generation**: Creates cryptographically signed JWTs containing user claims (e.g., `CustomerId`, `Email`, `Role`).
*   **Session Management**: Stores session information, including issued JWTs and Refresh Tokens, and manages their revocation and expiry.
*   **Refresh Token Mechanism**: Provides an endpoint to exchange an expired JWT and a valid refresh token for a new pair of tokens, enabling longer-lived sessions without compromising security.

### 2. JSON Web Tokens (JWT)

*   **Structure**: JWTs are compact, URL-safe means of representing claims to be transferred between two parties. They contain a header, a payload (claims), and a signature.
*   **Claims**: The payload includes standard claims (e.g., `exp` for expiration) and custom claims such as `CustomerId`, `Email`, and `Role`. The `Role` claim is crucial for RBAC.
*   **Signature**: JWTs are signed using a shared secret (`JWT_SECRET`) to ensure their integrity and authenticity.
*   **Lifetime**: JWTs are short-lived (e.g., 1 hour) to minimize the impact of token compromise.

### 3. Refresh Token Mechanism

To provide a seamless user experience with longer sessions, a refresh token mechanism is implemented:

*   **Issuance**: Upon successful login, the `Identity` service issues both a short-lived JWT and a longer-lived Refresh Token (e.g., 7 days).
*   **Usage**: When a JWT expires, the client can send the expired JWT along with the Refresh Token to the `/api/v1/auth/refresh` endpoint.
*   **Validation**: The `Identity` service validates the Refresh Token (checks expiry, revocation status, and user association). If valid, it revokes the old Refresh Token and issues a new JWT and a new Refresh Token.
*   **Security**: Refresh Tokens are stored securely in the database and are single-use. If a Refresh Token is compromised and used, the original token is revoked, preventing further unauthorized use.

### 4. Role-Based Access Control (RBAC)

Authorization is enforced using roles embedded in the JWT claims:

*   **Role Assignment**: During user registration, a default role ("Customer") is assigned, but an "Admin" role can also be assigned for privileged users.
*   **`[Authorize]` Attribute**: ASP.NET Core's `[Authorize]` attribute is used on controller classes or individual actions to specify required roles (e.g., `[Authorize(Roles = "Admin")]`, `[Authorize(Roles = "Customer")]`).
*   **Principle of Least Privilege**: Access to sensitive operations (e.g., creating/updating/deleting products, confirming/cancelling orders) is restricted to users with the "Admin" role, while standard user operations (e.g., creating orders) are restricted to the "Customer" role.

### 5. API Gateway Integration

The API Gateway (`Ocelot`) is configured to:

*   **Validate JWTs**: It performs initial validation of incoming JWTs before routing requests to downstream microservices.
*   **Route Authentication Requests**: Directs `/api/v1/auth/*` requests to the `Identity` service.
*   **Pass Claims**: Forwards valid JWTs (and thus user claims) to downstream services, allowing them to perform granular authorization checks.

## Environment Variables

The `JWT_SECRET` environment variable is consumed by the `Identity` service (for signing/validating tokens) and by the `inventory` and `sales` services (for validating incoming tokens). This secret is never hardcoded or stored in configuration files.

This comprehensive authentication and authorization strategy ensures that the NovaCommerce platform is secure, adheres to the principle of least privilege, and provides a robust session management experience.
