using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiBookStore.ViewModels;

namespace MauiBookStore.Views
{
    public partial class LoginPage
    {
        public LoginPage(LoginViewModel loginViewModel)
        {
            BindingContext = loginViewModel;
            InitializeComponent();
        }
    }
}