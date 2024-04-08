using System;

namespace Bannerlord.BLSE.Features.ContinueSaveFile.Patches;

internal class InformationManagerConfirmInquiryHandler : IDisposable
{
    public InformationManagerConfirmInquiryHandler() => InformationManagerPatch.SkipChange = true;
    public void Dispose() => InformationManagerPatch.SkipChange = false;
}