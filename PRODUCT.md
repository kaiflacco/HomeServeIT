# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Working inference from the current application: customers use the web app to request and follow IT services; technicians use it to review assigned work and update service progress; administrators coordinate requests, staff, customer records, billing, and operations.

## Product Purpose

Working inference from the current application: HomeServe IT coordinates IT support from a customer's service request through technician work, customer review, and billing. A useful outcome is that each role can understand the current state and take its next permitted action.

## Positioning

The current code brings customer requests, technician assignment and updates, customer approval, and administrative service operations into one role-aware web application. Treat this as an implementation description, not a confirmed market differentiator.

## Operating Context

Working inference from current routes and workflows: customers check request progress, appointments, quotations, invoices, devices, and support; technicians manage schedules and assigned jobs; administrators oversee jobs, technicians, finance, inventory, customer records, reports, support, and system settings.

## Capabilities and Constraints

- The repository implements an ASP.NET Core MVC web app with role-specific Customer, Technician, and Administrator areas.
- Existing service workflows include requests, assignment, quotation review, work progress, completion review, invoices, and notifications. Preserve existing workflow semantics and controls during visual redesign.
- The repository identifies this as a university prototype and instructs use of fictional data. Do not add real customer details, testimonials, performance statistics, or commercial claims.
- Existing application behavior and repository guidance are authoritative for this design task. This product summary is inferred from them and has not been confirmed by a product owner.

## Brand Commitments

The user named HomeServe IT. The existing application contains a HomeServe IT wordmark asset and a blue interface palette; retain recognizable product identity while improving the visual system. No additional voice or aesthetic preferences were provided.

## Evidence on Hand

- Existing interface, routes, role-specific workflows, and shared assets in `HomeServeIT.Web/`.
- `AGENTS.md` identifies the project as a university prototype and prohibits invented service claims or real customer data.
- No verified testimonials, market comparisons, or outcome metrics are established as design evidence.

## Product Principles

These principles are inferred from the current role boundaries and service workflow:

- Make the current request state and next action clear.
- Give each role a focused view of its responsibilities.
- Keep quotations and customer review visible at the right points in the service journey.
- Preserve clear, accessible controls across screen sizes.

## Accessibility & Inclusion

Preserve semantic structure, keyboard access, visible focus, readable contrast, and responsive layouts. These are supported by the existing repository's accessibility and responsive-interface guidance.
