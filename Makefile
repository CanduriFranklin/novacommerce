SHELL := /usr/bin/env bash

.PHONY: help build test clean compose-up compose-down tf-init tf-plan tf-apply helm-lint

help:
	@echo "NovaCommerce - Make targets"
	@echo " build          - Build all .NET projects"
	@echo " test           - Run unit tests"
	@echo " compose-up     - Start local stack (RabbitMQ, Redis, SQL Server)"
	@echo " compose-down   - Stop local stack"
	@echo " tf-init        - Terraform init (infrastructure/terraform)"
	@echo " tf-plan        - Terraform plan"
	@echo " tf-apply       - Terraform apply"

build:
	dotnet build --nologo --configuration Release

test:
	dotnet test --nologo --configuration Release --collect:"XPlat Code Coverage"

compose-up:
	docker compose -f compose.yaml up -d

compose-down:
	docker compose -f compose.yaml down -v

tf-init:
	cd infrastructure/terraform && terraform init

tf-plan:
	cd infrastructure/terraform && terraform plan

tf-apply:
	cd infrastructure/terraform && terraform apply -auto-approve
