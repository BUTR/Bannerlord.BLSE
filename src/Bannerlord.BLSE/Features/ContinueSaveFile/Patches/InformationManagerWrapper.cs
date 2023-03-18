using TaleWorlds.Library;

namespace Bannerlord.BLSE.Features.ContinueSaveFile.Patches;

internal static class InformationManagerWrapper
{
    public static void ShowInquiry(string titleText, string text) => InformationManager.ShowInquiry(new InquiryData(titleText, text, false, false, null, null, null, null));
}