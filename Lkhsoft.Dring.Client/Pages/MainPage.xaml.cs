using Lkhsoft.Dring.Client.Models;
using Lkhsoft.Dring.Client.PageModels;

namespace Lkhsoft.Dring.Client.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}