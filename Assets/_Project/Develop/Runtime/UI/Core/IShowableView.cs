using DG.Tweening;

namespace Assets._Project.Develop.Runtime.UI.Core
{
    public interface IShowableView : IView
    {
        Tween Hide();

        Tween Show();    
    }
}