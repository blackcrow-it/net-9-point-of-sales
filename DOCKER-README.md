# Docker Compose Setup for POS Backend

This document provides instructions for running the POS backend development environment using Docker Compose.

## Services

The `docker-compose.yml` file defines the following services:

### 1. PostgreSQL 16
- **Container Name**: `pos-postgres`
- **Port**: `5432`
- **Database**: `pos_db`
- **Username**: `postgres`
- **Password**: `postgres123`
- **Volume**: `postgres-data` (persists database data)

### 2. pgAdmin 4
- **Container Name**: `pos-pgadmin`
- **Port**: `5050`
- **URL**: http://localhost:5050
- **Email**: `admin@pos.local`
- **Password**: `admin123`
- **Volume**: `pgadmin-data` (persists configuration)

### 3. Redis 7
- **Container Name**: `pos-redis`
- **Port**: `6379`
- **Password**: None (authentication disabled for development)
- **Volume**: `redis-data` (persists cache data)

## Quick Start

### Start All Services
```bash
docker-compose up -d
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f postgres
docker-compose logs -f redis
docker-compose logs -f pgadmin
```

### Check Service Status
```bash
docker-compose ps
```

### Stop All Services
```bash
docker-compose down
```

### Stop and Remove Volumes (⚠️ Deletes all data)
```bash
docker-compose down -v
```

## Connecting to Services

### PostgreSQL Connection String
```
Host=localhost;Port=5432;Database=pos_db;Username=postgres;Password=postgres123
```

### Redis Connection String
```
localhost:6379
```

### pgAdmin Configuration
1. Open http://localhost:5050 in your browser
2. Login with:
   - Email: `admin@pos.local`
   - Password: `admin123`
3. Add PostgreSQL server:
   - **Name**: POS Database
   - **Host**: `postgres` (container name)
   - **Port**: `5432`
   - **Username**: `postgres`
   - **Password**: `postgres123`

## Health Checks

All services include health checks to ensure they're running properly:

- **PostgreSQL**: `pg_isready` command
- **Redis**: `redis-cli ping` command

## Network

All services are connected via the `pos-network` bridge network, allowing them to communicate with each other using container names.

## Data Persistence

Data is persisted in Docker volumes:
- `postgres-data`: PostgreSQL database files
- `pgadmin-data`: pgAdmin configuration
- `redis-data`: Redis persistence files

## Troubleshooting

### Port Already in Use
If you get a "port already in use" error, you can either:
1. Stop the conflicting service
2. Change the port mapping in `docker-compose.yml`

### Cannot Connect to Database
1. Ensure containers are running: `docker-compose ps`
2. Check container logs: `docker-compose logs postgres`
3. Verify health status: `docker inspect pos-postgres`

### Reset Everything
To start fresh:
```bash
docker-compose down -v
docker-compose up -d
```

## Production Notes

⚠️ **This configuration is for development only!**

For production:
- Use strong passwords
- Enable Redis authentication
- Configure PostgreSQL for production (connection pooling, tuning)
- Use Docker secrets for sensitive data
- Configure backup strategies
- Use external volumes for data persistence
