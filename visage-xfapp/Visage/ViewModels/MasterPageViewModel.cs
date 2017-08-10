﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Visage.Models;
using Visage.Pages;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class MasterPageViewModel : BaseViewModel
    {
        public List<MasterPageItem> MasterPageItems
        {
            get;
            set;
        }

        //public ICommand ItemSelectedCommand
        //{
        //    get;
        //    set;
        //}

        public MasterPageViewModel()
        {
            LoadMasterPageItems();

            //ItemSelectedCommand = new Command<string>(ExecuteItemSelected);
        }

        void LoadMasterPageItems()
        {
            var masterPageItems = new List<MasterPageItem>();

			masterPageItems.Add(new MasterPageItem
			{
				Title = "You",
				TargetType = typeof(ProfilePage)
			});
			masterPageItems.Add(new MasterPageItem
			{
				Title = "Events",
                TargetType = typeof(EventsPage)
			});

            MasterPageItems = masterPageItems;
        }

        //public void ExecuteItemSelected(MasterPageItem selectedItem)
        //{
            
        //}
    }
}
