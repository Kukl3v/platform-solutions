using Client.Services;
using Client.ViewModels;
using Client.Views;

namespace Client;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        var storage = new MatrixStorageService();
        var viewModel = new MainViewModel(
            storage,
            new MatrixColumnSorter(),
            new MatrixComparisonService(storage));
        Application.Run(new MainForm(viewModel, storage));
    }
}
