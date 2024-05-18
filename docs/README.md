# Todo list
* Security features of Free vs Paid tooling
* Total cost of ownership vs Security posture
* 

## Endpoint Security EDR
Protect and monitor developer's machine.

## List Of Software and Tools

| Purpose                       | Tool                | Website                                                       
|-------------------------------|---------------------|---------------------------------------------------------------
| Windows Package Management    | Chocolatey          | https://chocolatey.org/                                       
| Source Control                | Git                 | https://git-scm.com/                                          
|                               | Github CLI          | https://cli.github.com/                                       
| Code Editor                   | vscode              | https://code.visualstudio.com/                                
| Containerization              | Docker Desktop WSL2 | https://docs.docker.com/desktop/windows/wsl/                  
| Cross Platform Scripting      | Powershell          | https://github.com/PowerShell/PowerShell                      
| Azure CLI                     | Azure CLI           | https://learn.microsoft.com/en-us/cli/azure/install-azure-cli 
| Terminal                      | Windows Terminal    | https://github.com/microsoft/terminal                         

## Security for Choco
* Free vs Pro
    * https://chocolatey.org/compare
    * Pro has runtime anti-virus.
    * Free version supports own internal (private and/or authenticated) feeds
* Community free security review
    
    * https://docs.chocolatey.org/en-us/information/security#security-for-the-community-package-repository 
    * https://docs.chocolatey.org/en-us/community-repository/moderation/

* Organization use

    * https://docs.chocolatey.org/en-us/information/security#organizational-use-of-chocolatey

* Host private feed

    * https://docs.chocolatey.org/en-us/features/host-packages#local-folder-unc-share-cifs

## Docker Desktop Security
* Business vs free and cheaper plans
    * https://www.docker.com/pricing/
    * Hardened Docker Desktop
        * Enhanced container isolation and settings management including socket mount restrictions configuration.
    * Single Sign-On
    * Private Extensions Marketplace