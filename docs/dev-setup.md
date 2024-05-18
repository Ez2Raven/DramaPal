1. Install [Powershell 7.4.2](https://github.com/PowerShell/PowerShell/releases/tag/v7.4.2) for the specific platform
2. In a Powershell terminal, download and review the chocolatey installation script
    ```powershell
    # Define the URL of the script
    $url = "https://community.chocolatey.org/install.ps1"

    # Define the path where the script will be saved
    $filePath = "E:\workspace\psb-academy\fyp\docs\install-chocolatey.ps1"  # Adjust the path as needed

    # Download the script
    Invoke-WebRequest -Uri $url -OutFile $filePath
    ```
3. Launch a Powershell Terminal as an administrator to install Chocolatey
    ```powershell
    & $filePath
    ```
4. Install the development tools.
    ```powershell
    choco install git gh vscode docker-desktop -dy
    ```
5. Reboot the development machine.
    ```
    Restart-Computer -Force -Confirm
    ```
6. Run Docker Desktop if it is not running
7. Run VsCode on the code repository
8. Install the following extensions
    ```
    code --install-extension ms-vscode-remote.remote-containers
    ```