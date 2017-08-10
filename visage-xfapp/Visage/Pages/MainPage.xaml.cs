using System;
using System.Collections.Generic;
using Visage.Models;
using Visage.ViewModels;
using Xamarin.Forms;

namespace Visage.Pages
{
    public partial class MainPage : MasterDetailPage
    {
		public MainPage()
		{
			InitializeComponent();

            masterPage.ListView.ItemSelected += OnItemSelected; 

            BindingContext = new MainPageViewModel
            {
                Navigation = Navigation
            };
		}

		void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			var item = e.SelectedItem as MasterPageItem;
			if (item != null)
			{
				Detail = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
				masterPage.ListView.SelectedItem = null;
				IsPresented = false;
			}
		}
    }
}
