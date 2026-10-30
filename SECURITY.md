# Security Policy

## Supported versions

Security fixes are applied to the current default branch. No separate release-support schedule is currently published.

## Reporting a vulnerability

Please do not publish exploitable vulnerability details in a public issue. Use GitHub's private vulnerability reporting feature for this repository if it is enabled, or email [abdullahzulfiqar137@gmail.com](mailto:abdullahzulfiqar137@gmail.com) with the affected component, impact, and safe reproduction steps.

Never include production connection strings, payment-provider secrets, API keys, access tokens, customer data, or order information in a report.

## Payment and deployment warning

Do not use real payment credentials or customer data in local development. Before production use, independently review payment-provider integration, authorization, inventory consistency, order processing, secret storage, logging, and deployment configuration.
