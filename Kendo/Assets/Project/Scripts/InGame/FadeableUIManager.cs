using System.Collections.Generic;

/// <summary>
/// �V�[�����̑S�Ă�FadeableUI�C���X�^���X���Ǘ�����ÓI�N���X�B
/// ����ɂ��AFindObjectsOfType���g�킸�ɁA�M�����̍����I�u�W�F�N�g�Ԃ̘A�g���\�ɂȂ�܂��B
/// </summary>
public static class FadeableUIManager
{
    // �S�Ă�FadeableUI�C���X�^���X���i�[����A�ǂ�����ł��A�N�Z�X�\�Ȍ��J���X�g
    public static readonly List<FadeableUI> Instances = new List<FadeableUI>();
}