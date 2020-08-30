using System;
using System.Collections.Generic;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class MasterPage : ContentPage
    {
        MasterPageViewModel masterPageViewModel;

        public ListView ListView
        {
            get { return MenuItemsListView; }
        }
        
        public MasterPage()
        {
            InitializeComponent();

            BindingContext = masterPageViewModel = new MasterPageViewModel
            {
                Navigation = Navigation
            };
        }

        //void MasterPageItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        //{
        //    MenuItemsListView.SelectedItem = null;
        //}
    }
}
