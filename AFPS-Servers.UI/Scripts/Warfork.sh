#!/bin/bash

# Combined Environment Setup and Server Management Script
# Run with sudo for initial setup, then use server functions

set -e

# Set default values for environment variables
DEBUG="${DEBUG:-false}"
VALIDATE_SERVER_FILES="${VALIDATE_SERVER_FILES:-false}"
WF_CUSTOM_CONFIGS_DIR="${WF_CUSTOM_CONFIGS_DIR:-/var/wf}"
WF_PARAMS="${WF_PARAMS:-+set dedicated 1 +set net_port 44400}"

# Server configuration
steam_dir="$HOME/Steam"
server_dir="$HOME/server"
server_installed_lock_file="$server_dir/installed.lock"
wf_dir="$server_dir/basewf"
wf_custom_configs_dir="${WF_CUSTOM_CONFIGS_DIR-"/var/wf"}"

setup_environment() {
    echo "Starting environment setup..."

    # Check if running as root for system setup
    if [[ $EUID -ne 0 ]]; then
       echo "Environment setup needs to be run with sudo for system package installation"
       echo "Usage: sudo $0 setup"
       exit 1
    fi

    # Update package lists
    echo "Updating package lists..."
    apt-get update

    # Install required packages
    echo "Installing required packages..."
    apt-get install -y --no-install-recommends \
        lib32gcc-s1 \
        lib32stdc++6 \
        wget \
        ca-certificates \
        rsync \
        unzip \
        tmux \
        jq \
        bc \
        binutils \
        ca-certificates \
        util-linux \
        python3 \
        curl \
        wget \
        file \
        tar \
        bzip2 \
        gzip \
        unzip \
        bsdmainutils \
        libcurl4 \
        libcurl3-gnutls \
        libcurl4-gnutls-dev \
        wait-for-it \
        cron \
        sudo \
        vim \
        locales

    # Configure locales
    echo "Configuring locales..."
    sed -i -e 's/# en_US.UTF-8 UTF-8/en_US.UTF-8 UTF-8/' /etc/locale.gen
    dpkg-reconfigure --frontend=noninteractive locales

    # Clean up package cache
    echo "Cleaning up package cache..."
    rm -rf /var/lib/apt/lists/*

    # Create Steam directories
    echo "Creating Steam directories..."
    mkdir -p $HOME/Steam
    mkdir -p $HOME/server
    mkdir -p $HOME/.steam

    echo "Downloading and installing SteamCMD..."
    cd $HOME/Steam
    wget -qO- https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz | tar zxf -

    echo "Running SteamCMD initial setup..."
    $HOME/Steam/steamcmd.sh +quit

    echo "Creating Steam SDK symlinks..."
    ln -sf $HOME/Steam/linux64 $HOME/.steam/sdk64
    ln -sf $HOME/Steam/linux32 $HOME/.steam/sdk32

    echo "Creating server directories..."
    mkdir -p /var/wf/{maps,progs/gametypes,configs}
    touch /var/wf/motd.txt

    echo "Environment setup completed successfully!"
    echo ""
    echo "Steam and server directories created in $HOME/"
    echo "SteamCMD is ready to use at $HOME/Steam/steamcmd.sh"
    echo ""
    echo "Usage:"
    echo "$0 install          # Install server"
    echo "$0 update           # Update server"
    echo "$0 start            # Start server"
    echo "$0 run              # Install/update and start server"
    echo ""
    echo "Environment variables:"
    echo "- DEBUG=true                    # Enable verbose output"
    echo "- VALIDATE_SERVER_FILES=true    # Validate files on updates"
    echo "- WF_CUSTOM_CONFIGS_DIR=/path   # Custom configs directory (default: /var/wf)"
    echo "- WF_PARAMS='--param value'     # Additional server parameters"
}

install() {
    echo '> Installing server ...'

    if [ "${DEBUG}" = "true" ]; then
        set -x
    fi

    $steam_dir/steamcmd.sh \
        +force_install_dir $server_dir \
        +login anonymous \
        +app_update 1136510 validate \
        +quit

    if [ "${DEBUG}" = "true" ]; then
        set +x
    fi

    echo '> Done'
    touch $server_installed_lock_file
}

sync_custom_files() {
    echo "> Checking for custom files at \"$wf_custom_configs_dir\" ..."

    if [ -d "$wf_custom_configs_dir" ]; then
        echo "> Found custom files. Syncing with \"${wf_dir}\" ..."

        if [ "${DEBUG}" = "true" ]; then
            set -x
        fi

        cp -asf $wf_custom_configs_dir/* $wf_dir # Copy custom files as soft links
        find $wf_dir -xtype l -delete            # Find and delete broken soft links

        if [ "${DEBUG}" = "true" ]; then
            set +x
        fi

        echo '> Done'
    else
        echo '> No custom files found'
    fi
}

get_session_name() {
    local port
    port=$(echo "$WF_PARAMS" | sed -n 's/.*net_port \([0-9]*\).*/\1/p')
    echo "wf-${port:-44400}"
}

start() {
    echo '> Starting server ...'

    local session_name
    session_name=$(get_session_name)
    local log_file="$server_dir/${session_name}.log"

    if tmux has-session -t "$session_name" 2>/dev/null; then
        echo "> Session '$session_name' is already running. Use restart to restart it."
        exit 1
    fi

    if [ "${DEBUG}" = "true" ]; then
        set -x
    fi

    tmux new-session -d -s "$session_name" \
        "cd $wf_dir/.. && ./wf_server.x86_64 $WF_PARAMS 2>&1 | tee -a $log_file"

    echo "> Server started in tmux session '$session_name'"
    echo "> Log: $log_file"
    echo "> Attach: tmux attach -t $session_name"
}

update() {
    if [ "${VALIDATE_SERVER_FILES-"false"}" = "true" ]; then
        echo '> Validating server files and checking for server update ...'
    else
        echo '> Checking for server update ...'
    fi

    if [ "${DEBUG}" = "true" ]; then
        set -x
    fi

    if [ "${VALIDATE_SERVER_FILES-"false"}" = "true" ]; then
        $steam_dir/steamcmd.sh \
            +force_install_dir $server_dir \
            +login anonymous \
            +app_update 1136510 validate \
            +quit
    else
        $steam_dir/steamcmd.sh \
            +force_install_dir $server_dir \
            +login anonymous \
            +app_update 1136510 \
            +quit
    fi

    if [ "${DEBUG}" = "true" ]; then
        set +x
    fi

    echo '> Done'
}

install_or_update() {
    if [ -f "$server_installed_lock_file" ]; then
        update
    else
        install
    fi
}

run_server() {
    # Check if environment is set up, if not, set it up first
    if [ ! -f "$steam_dir/steamcmd.sh" ]; then
        echo "SteamCMD not found. Setting up environment first..."
        setup_environment
        echo "Environment setup complete. Continuing with server setup..."
    fi

    if [ "${DEBUG}" = "true" ]; then
        set -x
    fi

    shopt -s extglob
    install_or_update
    sync_custom_files
    start
}

stop() {
    echo '> Stopping server ...'

    local session_name
    session_name=$(get_session_name)

    if tmux has-session -t "$session_name" 2>/dev/null; then
        tmux kill-session -t "$session_name"
        echo '> Done'
    else
        echo "> No session '$session_name' found"
    fi
}

restart() {
    stop
    sleep 2
    start
}

# Main execution logic
case "${1:-}" in
    "setup")
        setup_environment
        ;;
    "install")
        install
        ;;
    "update")
        update
        ;;
    "start")
        start
        ;;
    "restart")
        restart
        ;;
    "stop")
        stop
        ;;
    "run"|"")
        # Check if we need sudo for setup
        if [ ! -f "$steam_dir/steamcmd.sh" ] && [[ $EUID -ne 0 ]]; then
            echo "Environment not set up and not running as root."
            echo "Please run: sudo $0 run"
            exit 1
        fi
        run_server
        ;;
    *)
        echo "Usage: $0 {setup|install|update|start|run}"
        echo ""
        echo "Commands:"
        echo "  setup     - Install system packages and set up environment (requires sudo)"
        echo "  install   - Install game server"
        echo "  update    - Update game server"
        echo "  start     - Start game server"
        echo "  run       - Install/update and start server (default)"
        echo ""
        echo "Environment variables:"
        echo "  DEBUG=true                    # Enable verbose output"
        echo "  VALIDATE_SERVER_FILES=true    # Validate files on updates"
        echo "  WF_CUSTOM_CONFIGS_DIR=/path   # Custom configs directory (default: /var/wf)"
        echo "  WF_PARAMS='--param value'     # Additional server parameters"
        exit 1
        ;;
esac