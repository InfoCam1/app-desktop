namespace InfoCam.Views
{
    public interface IActionableView
    {
        void Filter(string query);
        void GenerateReport();
    }
}
