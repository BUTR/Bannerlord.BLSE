using System;
using System.IO;

namespace Bannerlord.BLSE.Loaders.AppDomainManager;

internal sealed class BLSEAppDomainManager : System.AppDomainManager
{
    public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
    {
        base.InitializeNewDomain(appDomainInfo);

        var isEpicGamesStore = false;
        try
        {
            isEpicGamesStore = File.Exists("../../Modules/Native/epic.target");
        }
        catch { /* ignore */ }
        
        if (!isEpicGamesStore)
            return;

        Shared.AppDomainManager.Initialize();
    }
}