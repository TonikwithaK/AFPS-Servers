# AFPS Server Manager

A web-based dashboard for managing [Warfork](https://store.steampowered.com/app/671610/Warfork/) game servers. Add servers, edit configs, upload maps and gametypes, and control server processes — all from a browser.

## Features

- **Server inventory** — Register SSH-accessible servers with encrypted credential storage
- **Config editor** — Visual editor for `dedicated_autoexec.cfg` with support for cvars, aliases, binds, and exec commands
- **Config presets** — Save and load config templates across servers
- **Map & gametype manager** — Bulk upload `.pk3` files to one or more servers via SFTP
- **SSH console** — Interactive terminal for direct server access
- **Bulk operations** — Restart or push changes to multiple servers at once
- **Real-time output** — Streaming SSH output during long-running operations

## Tech Stack

| | |
|---|---|
| **Framework** | ASP.NET Core Blazor (Server-side, .NET 8) |
| **Database** | SQLite via Entity Framework Core |
| **SSH / SFTP** | [SSH.NET](https://github.com/sshnet/SSH.NET) |
| **UI** | Bootstrap 5 |

## Project Structure

```
AFPS-Servers.UI/           # Blazor web app (main entry point)
AFPS-Servers.Service/      # Business logic — SSH, SFTP, config parsing, presets
AFPS-Servers.Data/         # EF Core entities and migrations
AFPS-Servers.Infrastructure/ # DI registration
AFPS-Servers/              # Console host (optional background worker)
AFPS-Server.Test/          # Unit tests
```

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SSH access to at least one Warfork server

### Setup

1. **Clone and restore**
   ```bash
   git clone <repo-url>
   cd AFPS-Servers
   dotnet restore
   ```

2. **Set the encryption key** (used to encrypt stored SSH passwords)
   ```bash
   cd AFPS-Servers.UI
   dotnet user-secrets init
   dotnet user-secrets set "Encryption:Key" "$(openssl rand -base64 32)"
   ```

3. **Run**
   ```bash
   dotnet run
   ```

   The app will be available at `https://localhost:5001`. The SQLite database (`servers.db`) and migrations are applied automatically on first run.

## Warfork.sh

`AFPS-Servers.UI/Scripts/Warfork.sh` is a standalone bash script for server lifecycle management. The app uploads and executes it on remote servers via SSH.

```bash
sudo ./Warfork.sh setup    # Install system deps + SteamCMD (first time only)
./Warfork.sh install       # Download Warfork server via SteamCMD
./Warfork.sh start         # Launch server in a detached tmux session
./Warfork.sh stop          # Stop server
./Warfork.sh restart       # Restart server
./Warfork.sh run           # install/update + start (default)
```

Key environment variables:

| Variable | Default | Description |
|---|---|---|
| `WF_PARAMS` | `+set dedicated 1 +set net_port 44400` | Server launch parameters |
| `WF_CUSTOM_CONFIGS_DIR` | `/var/wf` | Directory for custom maps/configs |
| `VALIDATE_SERVER_FILES` | `false` | Run SteamCMD validation on update |
| `DEBUG` | `false` | Verbose shell output |

## Security Notes

- SSH passwords are AES-encrypted before being stored in the database
- The encryption key is kept out of source control via [ASP.NET Core User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- In production, set `Encryption:Key` via an environment variable or secrets manager instead of user secrets
