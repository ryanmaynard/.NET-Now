.PHONY: help build test run clean restore migrate docker-build docker-up docker-down

help: ## Show this help message
	@echo 'Usage: make [target]'
	@echo ''
	@echo 'Available targets:'
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  %-15s %s\n", $$1, $$2}' $(MAKEFILE_LIST)

restore: ## Restore NuGet packages
	dotnet restore

build: restore ## Build the solution
	dotnet build --configuration Release

test: ## Run all tests
	dotnet test --configuration Release --verbosity normal

test-unit: ## Run unit tests only
	dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --configuration Release --verbosity normal

test-integration: ## Run integration tests only
	dotnet test tests/Web.IntegrationTests/Web.IntegrationTests.csproj --configuration Release --verbosity normal

run: ## Run the application locally
	dotnet run --project src/Web/Web.csproj

watch: ## Run the application with hot reload
	dotnet watch --project src/Web/Web.csproj

clean: ## Clean build artifacts
	dotnet clean
	rm -rf **/bin **/obj

migrate: ## Run database migrations
	dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj

migration-add: ## Add a new migration (usage: make migration-add NAME=MigrationName)
	dotnet ef migrations add $(NAME) --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj

migration-remove: ## Remove the last migration
	dotnet ef migrations remove --project src/Infrastructure/Infrastructure.csproj --startup-project src/Web/Web.csproj

docker-build: ## Build Docker image
	docker build -t dotnet-boilerplate:latest .

docker-up: ## Start Docker Compose services
	docker-compose up -d

docker-down: ## Stop Docker Compose services
	docker-compose down

docker-logs: ## View Docker Compose logs
	docker-compose logs -f

docker-clean: ## Remove Docker containers and volumes
	docker-compose down -v

format: ## Format code
	dotnet format

lint: ## Check code style
	dotnet format --verify-no-changes
