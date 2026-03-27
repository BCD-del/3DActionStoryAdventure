namespace Assets._Project.Develop.Runtime.Utilites.LoadingScreen
{
    public interface ILoadingScreen
    {
        bool IsShown { get; }

        void Show();
        void Hide();
    }
}