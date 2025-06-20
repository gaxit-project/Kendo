using System.Collections.Generic;

/// <summary>
/// シーン内の全てのFadeableUIインスタンスを管理する静的クラス。
/// これにより、FindObjectsOfTypeを使わずに、信頼性の高いオブジェクト間の連携が可能になります。
/// </summary>
public static class FadeableUIManager
{
    // 全てのFadeableUIインスタンスを格納する、どこからでもアクセス可能な公開リスト
    public static readonly List<FadeableUI> Instances = new List<FadeableUI>();
}